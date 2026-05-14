using Npgsql;
using Taller2NET.Shared.Models;

namespace AcademicService.Repositories;

public interface IEnrollmentCrudRepository
{
    Task<IEnumerable<Enrollment>> GetAllAsync();
    Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment> UpdateAsync(Enrollment enrollment);
    Task<bool> DeleteAsync(int id);
}

public interface IAttendanceCrudRepository
{
    Task<IEnumerable<Attendance>> GetAllAsync();
    Task<IEnumerable<Attendance>> GetByEnrollmentAsync(int enrollmentId);
    Task<Attendance?> GetByIdAsync(int id);
    Task<Attendance> CreateAsync(Attendance attendance);
    Task<Attendance> UpdateAsync(Attendance attendance);
    Task<bool> DeleteAsync(int id);
}

public interface IGradeCrudRepository
{
    Task<IEnumerable<Grade>> GetAllAsync();
    Task<IEnumerable<Grade>> GetByEnrollmentAsync(int enrollmentId);
    Task<Grade?> GetByIdAsync(int id);
    Task<Grade> CreateAsync(Grade grade);
    Task<Grade> UpdateAsync(Grade grade);
    Task<bool> DeleteAsync(int id);
}

public class EnrollmentCrudRepository : IEnrollmentCrudRepository
{
    private readonly string _connectionString;

    public EnrollmentCrudRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync() =>
        await QueryEnrollmentsAsync("""
            SELECT e."Id", e."StudentId", e."CourseId", e."Status", e."EnrolledAt", e."WithdrawnAt", e."CreatedAt",
                   s."FirstName", s."LastName",
                   c."Code", c."Name"
            FROM "Enrollments" e
            JOIN "Students" s ON s."Id" = e."StudentId"
            JOIN "Courses" c ON c."Id" = e."CourseId"
            ORDER BY e."Id" DESC
            """);

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId) =>
        await QueryEnrollmentsAsync("""
            SELECT e."Id", e."StudentId", e."CourseId", e."Status", e."EnrolledAt", e."WithdrawnAt", e."CreatedAt",
                   s."FirstName", s."LastName",
                   c."Code", c."Name"
            FROM "Enrollments" e
            JOIN "Students" s ON s."Id" = e."StudentId"
            JOIN "Courses" c ON c."Id" = e."CourseId"
            WHERE e."StudentId" = @studentId
            ORDER BY e."Id" DESC
            """, new NpgsqlParameter("@studentId", studentId));

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId) =>
        await QueryEnrollmentsAsync("""
            SELECT e."Id", e."StudentId", e."CourseId", e."Status", e."EnrolledAt", e."WithdrawnAt", e."CreatedAt",
                   s."FirstName", s."LastName",
                   c."Code", c."Name"
            FROM "Enrollments" e
            JOIN "Students" s ON s."Id" = e."StudentId"
            JOIN "Courses" c ON c."Id" = e."CourseId"
            WHERE e."CourseId" = @courseId
            ORDER BY e."Id" DESC
            """, new NpgsqlParameter("@courseId", courseId));

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        var result = await QueryEnrollmentsAsync("""
            SELECT e."Id", e."StudentId", e."CourseId", e."Status", e."EnrolledAt", e."WithdrawnAt", e."CreatedAt",
                   s."FirstName", s."LastName",
                   c."Code", c."Name"
            FROM "Enrollments" e
            JOIN "Students" s ON s."Id" = e."StudentId"
            JOIN "Courses" c ON c."Id" = e."CourseId"
            WHERE e."Id" = @id
            """, new NpgsqlParameter("@id", id));
        return result.FirstOrDefault();
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId)
    {
        var result = await QueryEnrollmentsAsync("""
            SELECT e."Id", e."StudentId", e."CourseId", e."Status", e."EnrolledAt", e."WithdrawnAt", e."CreatedAt",
                   s."FirstName", s."LastName",
                   c."Code", c."Name"
            FROM "Enrollments" e
            JOIN "Students" s ON s."Id" = e."StudentId"
            JOIN "Courses" c ON c."Id" = e."CourseId"
            WHERE e."StudentId" = @studentId AND e."CourseId" = @courseId
            """,
            new NpgsqlParameter("@studentId", studentId),
            new NpgsqlParameter("@courseId", courseId));
        return result.FirstOrDefault();
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            INSERT INTO "Enrollments" ("StudentId", "CourseId", "Status", "EnrolledAt", "WithdrawnAt", "CreatedAt")
            VALUES (@studentId, @courseId, @status, @enrolledAt, @withdrawnAt, @createdAt)
            RETURNING "Id"
            """, conn);
        cmd.Parameters.AddWithValue("@studentId", enrollment.StudentId);
        cmd.Parameters.AddWithValue("@courseId", enrollment.CourseId);
        cmd.Parameters.AddWithValue("@status", enrollment.Status.ToString());
        cmd.Parameters.AddWithValue("@enrolledAt", enrollment.EnrolledAt);
        cmd.Parameters.AddWithValue("@withdrawnAt", enrollment.WithdrawnAt is null ? DBNull.Value : enrollment.WithdrawnAt.Value);
        cmd.Parameters.AddWithValue("@createdAt", enrollment.CreatedAt);

        enrollment.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return enrollment;
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            UPDATE "Enrollments"
            SET "Status" = @status, "WithdrawnAt" = @withdrawnAt
            WHERE "Id" = @id
            """, conn);
        cmd.Parameters.AddWithValue("@id", enrollment.Id);
        cmd.Parameters.AddWithValue("@status", enrollment.Status.ToString());
        cmd.Parameters.AddWithValue("@withdrawnAt", enrollment.WithdrawnAt is null ? DBNull.Value : enrollment.WithdrawnAt.Value);
        await cmd.ExecuteNonQueryAsync();
        return enrollment;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("""DELETE FROM "Enrollments" WHERE "Id" = @id""", conn);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private async Task<IEnumerable<Enrollment>> QueryEnrollmentsAsync(string sql, params NpgsqlParameter[] parameters)
    {
        var items = new List<Enrollment>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var status = Enum.TryParse<EnrollmentStatus>(reader.GetString(3), true, out var parsedStatus)
                ? parsedStatus
                : EnrollmentStatus.Active;

            items.Add(new Enrollment
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                CourseId = reader.GetInt32(2),
                Status = status,
                EnrolledAt = reader.GetDateTime(4),
                WithdrawnAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                CreatedAt = reader.GetDateTime(6),
                Student = new Student
                {
                    Id = reader.GetInt32(1),
                    FirstName = reader.GetString(7),
                    LastName = reader.GetString(8)
                },
                Course = new Course
                {
                    Id = reader.GetInt32(2),
                    Code = reader.GetString(9),
                    Name = reader.GetString(10)
                }
            });
        }

        return items;
    }
}

public class AttendanceCrudRepository : IAttendanceCrudRepository
{
    private readonly string _connectionString;

    public AttendanceCrudRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    public async Task<IEnumerable<Attendance>> GetAllAsync() =>
        await QueryAttendancesAsync("""
            SELECT "Id", "EnrollmentId", "Date", "Status", "Notes", "CreatedAt"
            FROM "Attendances"
            ORDER BY "Date" DESC, "Id" DESC
            """);

    public async Task<IEnumerable<Attendance>> GetByEnrollmentAsync(int enrollmentId) =>
        await QueryAttendancesAsync("""
            SELECT "Id", "EnrollmentId", "Date", "Status", "Notes", "CreatedAt"
            FROM "Attendances"
            WHERE "EnrollmentId" = @enrollmentId
            ORDER BY "Date" DESC, "Id" DESC
            """, new NpgsqlParameter("@enrollmentId", enrollmentId));

    public async Task<Attendance?> GetByIdAsync(int id)
    {
        var result = await QueryAttendancesAsync("""
            SELECT "Id", "EnrollmentId", "Date", "Status", "Notes", "CreatedAt"
            FROM "Attendances"
            WHERE "Id" = @id
            """, new NpgsqlParameter("@id", id));
        return result.FirstOrDefault();
    }

    public async Task<Attendance> CreateAsync(Attendance attendance)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            INSERT INTO "Attendances" ("EnrollmentId", "Date", "Status", "Notes", "CreatedAt")
            VALUES (@enrollmentId, @date, @status, @notes, @createdAt)
            RETURNING "Id"
            """, conn);
        cmd.Parameters.AddWithValue("@enrollmentId", attendance.EnrollmentId);
        cmd.Parameters.AddWithValue("@date", attendance.Date);
        cmd.Parameters.AddWithValue("@status", attendance.Status.ToString());
        cmd.Parameters.AddWithValue("@notes", attendance.Notes is null ? DBNull.Value : attendance.Notes);
        cmd.Parameters.AddWithValue("@createdAt", attendance.CreatedAt);
        attendance.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return attendance;
    }

    public async Task<Attendance> UpdateAsync(Attendance attendance)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            UPDATE "Attendances"
            SET "Date" = @date, "Status" = @status, "Notes" = @notes
            WHERE "Id" = @id
            """, conn);
        cmd.Parameters.AddWithValue("@id", attendance.Id);
        cmd.Parameters.AddWithValue("@date", attendance.Date);
        cmd.Parameters.AddWithValue("@status", attendance.Status.ToString());
        cmd.Parameters.AddWithValue("@notes", attendance.Notes is null ? DBNull.Value : attendance.Notes);
        await cmd.ExecuteNonQueryAsync();
        return attendance;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("""DELETE FROM "Attendances" WHERE "Id" = @id""", conn);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private async Task<IEnumerable<Attendance>> QueryAttendancesAsync(string sql, params NpgsqlParameter[] parameters)
    {
        var items = new List<Attendance>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var status = Enum.TryParse<AttendanceStatus>(reader.GetString(3), true, out var parsedStatus)
                ? parsedStatus
                : AttendanceStatus.Present;

            items.Add(new Attendance
            {
                Id = reader.GetInt32(0),
                EnrollmentId = reader.GetInt32(1),
                Date = reader.GetDateTime(2),
                Status = status,
                Notes = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }

        return items;
    }
}

public class GradeCrudRepository : IGradeCrudRepository
{
    private readonly string _connectionString;

    public GradeCrudRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    public async Task<IEnumerable<Grade>> GetAllAsync() =>
        await QueryGradesAsync("""
            SELECT "Id", "EnrollmentId", "Type", "Value", "Weight", "Description", "EvaluatedAt", "CreatedAt"
            FROM "Grades"
            ORDER BY "EvaluatedAt" DESC, "Id" DESC
            """);

    public async Task<IEnumerable<Grade>> GetByEnrollmentAsync(int enrollmentId) =>
        await QueryGradesAsync("""
            SELECT "Id", "EnrollmentId", "Type", "Value", "Weight", "Description", "EvaluatedAt", "CreatedAt"
            FROM "Grades"
            WHERE "EnrollmentId" = @enrollmentId
            ORDER BY "EvaluatedAt" DESC, "Id" DESC
            """, new NpgsqlParameter("@enrollmentId", enrollmentId));

    public async Task<Grade?> GetByIdAsync(int id)
    {
        var result = await QueryGradesAsync("""
            SELECT "Id", "EnrollmentId", "Type", "Value", "Weight", "Description", "EvaluatedAt", "CreatedAt"
            FROM "Grades"
            WHERE "Id" = @id
            """, new NpgsqlParameter("@id", id));
        return result.FirstOrDefault();
    }

    public async Task<Grade> CreateAsync(Grade grade)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            INSERT INTO "Grades" ("EnrollmentId", "Type", "Value", "Weight", "Description", "EvaluatedAt", "CreatedAt")
            VALUES (@enrollmentId, @type, @value, @weight, @description, @evaluatedAt, @createdAt)
            RETURNING "Id"
            """, conn);
        cmd.Parameters.AddWithValue("@enrollmentId", grade.EnrollmentId);
        cmd.Parameters.AddWithValue("@type", grade.Type.ToString());
        cmd.Parameters.AddWithValue("@value", grade.Value);
        cmd.Parameters.AddWithValue("@weight", grade.Weight);
        cmd.Parameters.AddWithValue("@description", grade.Description is null ? DBNull.Value : grade.Description);
        cmd.Parameters.AddWithValue("@evaluatedAt", grade.EvaluatedAt);
        cmd.Parameters.AddWithValue("@createdAt", grade.CreatedAt);
        grade.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return grade;
    }

    public async Task<Grade> UpdateAsync(Grade grade)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            UPDATE "Grades"
            SET "Type" = @type, "Value" = @value, "Weight" = @weight, "Description" = @description, "EvaluatedAt" = @evaluatedAt
            WHERE "Id" = @id
            """, conn);
        cmd.Parameters.AddWithValue("@id", grade.Id);
        cmd.Parameters.AddWithValue("@type", grade.Type.ToString());
        cmd.Parameters.AddWithValue("@value", grade.Value);
        cmd.Parameters.AddWithValue("@weight", grade.Weight);
        cmd.Parameters.AddWithValue("@description", grade.Description is null ? DBNull.Value : grade.Description);
        cmd.Parameters.AddWithValue("@evaluatedAt", grade.EvaluatedAt);
        await cmd.ExecuteNonQueryAsync();
        return grade;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("""DELETE FROM "Grades" WHERE "Id" = @id""", conn);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private async Task<IEnumerable<Grade>> QueryGradesAsync(string sql, params NpgsqlParameter[] parameters)
    {
        var items = new List<Grade>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var type = Enum.TryParse<GradeType>(reader.GetString(2), true, out var parsedType)
                ? parsedType
                : GradeType.Assignment;

            items.Add(new Grade
            {
                Id = reader.GetInt32(0),
                EnrollmentId = reader.GetInt32(1),
                Type = type,
                Value = reader.GetDecimal(3),
                Weight = reader.GetDecimal(4),
                Description = reader.IsDBNull(5) ? null : reader.GetString(5),
                EvaluatedAt = reader.GetDateTime(6),
                CreatedAt = reader.GetDateTime(7)
            });
        }

        return items;
    }
}
