using Npgsql;
using Taller2NET.Shared.Models;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace AcademicService.Repositories;

public interface IEnrollmentRepository
{
    Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment> UpdateAsync(Enrollment enrollment);
}

public interface IAttendanceRepository
{
    Task<IEnumerable<Attendance>> GetByEnrollmentAsync(int enrollmentId);
    Task<Attendance> CreateAsync(Attendance attendance);
    Task<IEnumerable<Attendance>> CreateRangeAsync(IEnumerable<Attendance> attendances);
}

public interface IGradeRepository
{
    Task<IEnumerable<Grade>> GetByEnrollmentAsync(int enrollmentId);
    Task<Grade> CreateAsync(Grade grade);
    Task<Grade> UpdateAsync(Grade grade);
}

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly string _connectionString;

    public EnrollmentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId)
    {
        var list = new List<Enrollment>();
         const string sql = @"SELECT e.id AS enrollment_id, e.studentid, e.courseid, e.status, e.enrolledat, e.withdrawnat, e.createdat,
                          s.id AS student_id, s.firstname, s.lastname, s.studentcode, s.email,
                          c.id AS course_id, c.code, c.name, c.maxstudents
                      FROM enrollments e
                      JOIN students s ON e.studentid = s.id
                      JOIN courses c ON e.courseid = c.id
                      WHERE e.studentid = @studentId
                      ORDER BY e.createdat DESC";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@studentId", NpgsqlTypes.NpgsqlDbType.Integer, studentId);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        while (await reader.ReadAsync())
        {
            var enrollment = new Enrollment
            {
                Id = reader.GetInt32(reader.GetOrdinal("enrollment_id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("studentid")),
                CourseId = reader.GetInt32(reader.GetOrdinal("courseid")),
                Status = Enum.Parse<EnrollmentStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                EnrolledAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("EnrolledAt")),
                WithdrawnAt = reader.IsDBNull(reader.GetOrdinal("WithdrawnAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("WithdrawnAt")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("createdat")),
                Student = new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("student_id")),
                    FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                    LastName = reader.GetString(reader.GetOrdinal("lastname")),
                    StudentCode = reader.GetString(reader.GetOrdinal("studentcode")),
                    Email = reader.GetString(reader.GetOrdinal("email"))
                },
                Course = new Course
                {
                    Id = reader.GetInt32(reader.GetOrdinal("course_id")),
                    Code = reader.GetString(reader.GetOrdinal("code")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    MaxStudents = reader.GetInt32(reader.GetOrdinal("maxstudents"))
                }
            };
            list.Add(enrollment);
        }
        return list;
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId)
    {
        var list = new List<Enrollment>();
        const string sql = @"SELECT e.id AS enrollment_id, e.studentid, e.courseid, e.status, e.enrolledat, e.withdrawnat, e.createdat,
                         s.id AS student_id, s.firstname, s.lastname, s.studentcode, s.email
                      FROM enrollments e
                      JOIN students s ON e.studentid = s.id
                      WHERE e.courseid = @courseId
                      ORDER BY e.createdat DESC";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@courseId", NpgsqlTypes.NpgsqlDbType.Integer, courseId);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        while (await reader.ReadAsync())
        {
            var enrollment = new Enrollment
            {
                Id = reader.GetInt32(reader.GetOrdinal("enrollment_id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("studentid")),
                CourseId = reader.GetInt32(reader.GetOrdinal("courseid")),
                Status = Enum.Parse<EnrollmentStatus>(reader.GetString(reader.GetOrdinal("status"))),
                EnrolledAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("enrolledat")),
                WithdrawnAt = reader.IsDBNull(reader.GetOrdinal("withdrawnat")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("withdrawnat")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("createdat")),
                Student = new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("student_id")),
                    FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                    LastName = reader.GetString(reader.GetOrdinal("lastname")),
                    StudentCode = reader.GetString(reader.GetOrdinal("studentcode")),
                    Email = reader.GetString(reader.GetOrdinal("email"))
                }
            };
            list.Add(enrollment);
        }
        return list;
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
         const string sql = @"SELECT e.id AS enrollment_id, e.studentid, e.courseid, e.status, e.enrolledat, e.withdrawnat, e.createdat,
                          s.id AS student_id, s.firstname, s.lastname, s.studentcode, s.email,
                          c.id AS course_id, c.code, c.name, c.maxstudents
                      FROM enrollments e
                      JOIN students s ON e.studentid = s.id
                      JOIN courses c ON e.courseid = c.id
                      WHERE e.id = @id LIMIT 1";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new Enrollment
            {
                Id = reader.GetInt32(reader.GetOrdinal("enrollment_id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("studentid")),
                CourseId = reader.GetInt32(reader.GetOrdinal("courseid")),
                Status = Enum.Parse<EnrollmentStatus>(reader.GetString(reader.GetOrdinal("status"))),
                EnrolledAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("enrolledat")),
                WithdrawnAt = reader.IsDBNull(reader.GetOrdinal("withdrawnat")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("withdrawnat")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("createdat")),
                Student = new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("student_id")),
                    FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                    LastName = reader.GetString(reader.GetOrdinal("lastname")),
                    StudentCode = reader.GetString(reader.GetOrdinal("studentcode")),
                    Email = reader.GetString(reader.GetOrdinal("email"))
                },
                Course = new Course
                {
                    Id = reader.GetInt32(reader.GetOrdinal("course_id")),
                    Code = reader.GetString(reader.GetOrdinal("code")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    MaxStudents = reader.GetInt32(reader.GetOrdinal("maxstudents"))
                }
            };
        }
        return null;
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId)
    {
        const string sql = "SELECT * FROM \"Enrollments\" WHERE \"StudentId\" = @studentId AND \"CourseId\" = @courseId LIMIT 1";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@studentId", NpgsqlTypes.NpgsqlDbType.Integer, studentId);
        cmd.Parameters.AddWithValue("@courseId", NpgsqlTypes.NpgsqlDbType.Integer, courseId);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new Enrollment
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                CourseId = reader.GetInt32(reader.GetOrdinal("CourseId")),
                Status = Enum.Parse<EnrollmentStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                EnrolledAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("EnrolledAt")),
                WithdrawnAt = reader.IsDBNull(reader.GetOrdinal("WithdrawnAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("WithdrawnAt")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt"))
            };
        }
        return null;
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        const string sql = "INSERT INTO \"Enrollments\" (\"StudentId\", \"CourseId\", \"Status\", \"EnrolledAt\", \"CreatedAt\") VALUES (@studentId,@courseId,@status,@enrolled,@created) RETURNING \"Id\"";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@studentId", NpgsqlTypes.NpgsqlDbType.Integer, enrollment.StudentId);
        cmd.Parameters.AddWithValue("@courseId", NpgsqlTypes.NpgsqlDbType.Integer, enrollment.CourseId);
        cmd.Parameters.AddWithValue("@status", NpgsqlTypes.NpgsqlDbType.Text, enrollment.Status.ToString());
        cmd.Parameters.AddWithValue("@enrolled", NpgsqlTypes.NpgsqlDbType.TimestampTz, enrollment.EnrolledAt);
        cmd.Parameters.AddWithValue("@created", NpgsqlTypes.NpgsqlDbType.TimestampTz, enrollment.CreatedAt);
        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        enrollment.Id = newId;
        return enrollment;
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        const string sql = "UPDATE \"Enrollments\" SET \"Status\" = @status, \"EnrolledAt\" = @enrolled, \"WithdrawnAt\" = @withdrawn WHERE \"Id\" = @id";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@status", NpgsqlTypes.NpgsqlDbType.Text, enrollment.Status.ToString());
        cmd.Parameters.AddWithValue("@enrolled", NpgsqlTypes.NpgsqlDbType.TimestampTz, enrollment.EnrolledAt);
        if (enrollment.WithdrawnAt.HasValue) cmd.Parameters.AddWithValue("@withdrawn", NpgsqlTypes.NpgsqlDbType.TimestampTz, enrollment.WithdrawnAt.Value); else cmd.Parameters.AddWithValue("@withdrawn", DBNull.Value);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, enrollment.Id);
        await cmd.ExecuteNonQueryAsync();
        return enrollment;
    }
}

public class AttendanceRepository : IAttendanceRepository
{
    private readonly string _connectionString;

    public AttendanceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<Attendance>> GetByEnrollmentAsync(int enrollmentId)
    {
        var list = new List<Attendance>();
        const string sql = "SELECT \"Id\", \"EnrollmentId\", \"Date\", \"Status\", \"Notes\", \"CreatedAt\" FROM \"Attendances\" WHERE \"EnrollmentId\" = @enrollmentId ORDER BY \"Date\" DESC";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@enrollmentId", NpgsqlTypes.NpgsqlDbType.Integer, enrollmentId);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        while (await reader.ReadAsync())
        {
            list.Add(new Attendance
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                EnrollmentId = reader.GetInt32(reader.GetOrdinal("EnrollmentId")),
                Date = reader.GetFieldValue<DateTime>(reader.GetOrdinal("Date")),
                Status = Enum.Parse<AttendanceStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt"))
            });
        }
        return list;
    }

    public async Task<Attendance> CreateAsync(Attendance attendance)
    {
        const string sql = "INSERT INTO \"Attendances\" (\"EnrollmentId\", \"Date\", \"Status\", \"Notes\", \"CreatedAt\") VALUES (@enrollmentId,@date,@status,@notes,@created) RETURNING \"Id\"";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@enrollmentId", NpgsqlTypes.NpgsqlDbType.Integer, attendance.EnrollmentId);
        cmd.Parameters.AddWithValue("@date", NpgsqlTypes.NpgsqlDbType.TimestampTz, attendance.Date);
        cmd.Parameters.AddWithValue("@status", NpgsqlTypes.NpgsqlDbType.Text, attendance.Status.ToString());
        cmd.Parameters.AddWithValue("@notes", NpgsqlTypes.NpgsqlDbType.Text, (object?)attendance.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@created", NpgsqlTypes.NpgsqlDbType.TimestampTz, attendance.CreatedAt);
        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        attendance.Id = newId;
        return attendance;
    }

    public async Task<IEnumerable<Attendance>> CreateRangeAsync(IEnumerable<Attendance> attendances)
    {
        var list = attendances.ToList();
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var tran = await conn.BeginTransactionAsync();
        try
        {
            foreach (var a in list)
            {
                const string sql = "INSERT INTO \"Attendances\" (\"EnrollmentId\", \"Date\", \"Status\", \"Notes\", \"CreatedAt\") VALUES (@enrollmentId,@date,@status,@notes,@created) RETURNING \"Id\"";
                using var cmd = new NpgsqlCommand(sql, conn, tran as NpgsqlTransaction);
                cmd.Parameters.AddWithValue("@enrollmentId", NpgsqlTypes.NpgsqlDbType.Integer, a.EnrollmentId);
                cmd.Parameters.AddWithValue("@date", NpgsqlTypes.NpgsqlDbType.TimestampTz, a.Date);
                cmd.Parameters.AddWithValue("@status", NpgsqlTypes.NpgsqlDbType.Text, a.Status.ToString());
                cmd.Parameters.AddWithValue("@notes", NpgsqlTypes.NpgsqlDbType.Text, (object?)a.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@created", NpgsqlTypes.NpgsqlDbType.TimestampTz, a.CreatedAt);
                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                a.Id = newId;
            }
            await tran.CommitAsync();
            return list;
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }
}

public class GradeRepository : IGradeRepository
{
    private readonly string _connectionString;

    public GradeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<Grade>> GetByEnrollmentAsync(int enrollmentId)
    {
        var list = new List<Grade>();
        const string sql = "SELECT \"Id\", \"EnrollmentId\", \"Type\", \"Value\", \"Weight\", \"Description\", \"EvaluatedAt\", \"CreatedAt\" FROM \"Grades\" WHERE \"EnrollmentId\" = @enrollmentId ORDER BY \"EvaluatedAt\" DESC";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@enrollmentId", NpgsqlTypes.NpgsqlDbType.Integer, enrollmentId);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        while (await reader.ReadAsync())
        {
            list.Add(new Grade
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                EnrollmentId = reader.GetInt32(reader.GetOrdinal("EnrollmentId")),
                Type = Enum.Parse<GradeType>(reader.GetString(reader.GetOrdinal("Type"))),
                Value = reader.GetDecimal(reader.GetOrdinal("Value")),
                Weight = reader.GetDecimal(reader.GetOrdinal("Weight")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                EvaluatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("EvaluatedAt")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt"))
            });
        }
        return list;
    }

    public async Task<Grade> CreateAsync(Grade grade)
    {
        const string sql = "INSERT INTO \"Grades\" (\"EnrollmentId\", \"Type\", \"Value\", \"Weight\", \"Description\", \"EvaluatedAt\", \"CreatedAt\") VALUES (@enrollmentId,@type,@value,@weight,@desc,@evaluated,@created) RETURNING \"Id\"";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@enrollmentId", NpgsqlTypes.NpgsqlDbType.Integer, grade.EnrollmentId);
        cmd.Parameters.AddWithValue("@type", NpgsqlTypes.NpgsqlDbType.Text, grade.Type.ToString());
        cmd.Parameters.AddWithValue("@value", NpgsqlTypes.NpgsqlDbType.Numeric, grade.Value);
        cmd.Parameters.AddWithValue("@weight", NpgsqlTypes.NpgsqlDbType.Numeric, grade.Weight);
        cmd.Parameters.AddWithValue("@desc", NpgsqlTypes.NpgsqlDbType.Text, (object?)grade.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@evaluated", NpgsqlTypes.NpgsqlDbType.TimestampTz, grade.EvaluatedAt);
        cmd.Parameters.AddWithValue("@created", NpgsqlTypes.NpgsqlDbType.TimestampTz, grade.CreatedAt);
        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        grade.Id = newId;
        return grade;
    }

    public async Task<Grade> UpdateAsync(Grade grade)
    {
        const string sql = "UPDATE \"Grades\" SET \"Type\" = @type, \"Value\" = @value, \"Weight\" = @weight, \"Description\" = @desc, \"EvaluatedAt\" = @evaluated WHERE \"Id\" = @id";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@type", NpgsqlTypes.NpgsqlDbType.Text, grade.Type.ToString());
        cmd.Parameters.AddWithValue("@value", NpgsqlTypes.NpgsqlDbType.Numeric, grade.Value);
        cmd.Parameters.AddWithValue("@weight", NpgsqlTypes.NpgsqlDbType.Numeric, grade.Weight);
        cmd.Parameters.AddWithValue("@desc", NpgsqlTypes.NpgsqlDbType.Text, (object?)grade.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@evaluated", NpgsqlTypes.NpgsqlDbType.TimestampTz, grade.EvaluatedAt);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, grade.Id);
        await cmd.ExecuteNonQueryAsync();
        return grade;
    }
}
