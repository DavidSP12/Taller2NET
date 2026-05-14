using Npgsql;
using Taller2NET.Shared.Models;

namespace AcademicService.Repositories;

public class AdoCourseRepository : ICourseRepository
{
    private readonly string _connectionString;

    public AdoCourseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    public async Task<(IEnumerable<Course> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var parameters = new List<NpgsqlParameter>();
        var whereClause = string.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause = "WHERE (c.\"Name\" ILIKE @search OR c.\"Code\" ILIKE @search OR c.\"Teacher\" ILIKE @search)";
            parameters.Add(new NpgsqlParameter("@search", $"%{search}%"));
        }

        var totalCount = await ExecuteCountAsync($"SELECT COUNT(*) FROM \"Courses\" c {whereClause}", parameters.ToArray());

        var sql = $"""
            SELECT c."Id", c."Code", c."Name", c."Description", c."Credits", c."Teacher", c."Semester", c."MaxStudents",
                   c."Schedule", c."IsActive", c."CreatedAt", c."UpdatedAt",
                   COALESCE(COUNT(e."Id"), 0) AS "EnrolledCount"
            FROM "Courses" c
            LEFT JOIN "Enrollments" e ON e."CourseId" = c."Id" AND e."Status" = 'Active'
            {whereClause}
            GROUP BY c."Id"
            ORDER BY c."Code"
            LIMIT @limit OFFSET @offset
            """;

        var queryParameters = new List<NpgsqlParameter>(parameters)
        {
            new("@limit", pageSize),
            new("@offset", (page - 1) * pageSize)
        };

        return (await QueryCoursesAsync(sql, queryParameters.ToArray()), totalCount);
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT c."Id", c."Code", c."Name", c."Description", c."Credits", c."Teacher", c."Semester", c."MaxStudents",
                   c."Schedule", c."IsActive", c."CreatedAt", c."UpdatedAt",
                   COALESCE(COUNT(e."Id"), 0) AS "EnrolledCount"
            FROM "Courses" c
            LEFT JOIN "Enrollments" e ON e."CourseId" = c."Id" AND e."Status" = 'Active'
            WHERE c."Id" = @id
            GROUP BY c."Id"
            """;

        var items = await QueryCoursesAsync(sql, new NpgsqlParameter("@id", id));
        return items.FirstOrDefault();
    }

    public async Task<Course?> GetByCodeAsync(string code)
    {
        const string sql = """
            SELECT c."Id", c."Code", c."Name", c."Description", c."Credits", c."Teacher", c."Semester", c."MaxStudents",
                   c."Schedule", c."IsActive", c."CreatedAt", c."UpdatedAt",
                   COALESCE(COUNT(e."Id"), 0) AS "EnrolledCount"
            FROM "Courses" c
            LEFT JOIN "Enrollments" e ON e."CourseId" = c."Id" AND e."Status" = 'Active'
            WHERE c."Code" = @code
            GROUP BY c."Id"
            """;

        var items = await QueryCoursesAsync(sql, new NpgsqlParameter("@code", code));
        return items.FirstOrDefault();
    }

    public async Task<Course> CreateAsync(Course course)
    {
        const string sql = """
            INSERT INTO "Courses" ("Code", "Name", "Description", "Credits", "Teacher", "Semester", "MaxStudents", "Schedule", "IsActive", "CreatedAt")
            VALUES (@code, @name, @description, @credits, @teacher, @semester, @maxStudents, @schedule, @isActive, @createdAt)
            RETURNING "Id"
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        AddCourseParameters(cmd, course);
        course.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        const string sql = """
            UPDATE "Courses"
            SET "Name" = @name,
                "Description" = @description,
                "Credits" = @credits,
                "Teacher" = @teacher,
                "Semester" = @semester,
                "MaxStudents" = @maxStudents,
                "Schedule" = @schedule,
                "IsActive" = @isActive,
                "UpdatedAt" = @updatedAt
            WHERE "Id" = @id
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", course.Id);
        cmd.Parameters.AddWithValue("@name", course.Name);
        cmd.Parameters.AddWithValue("@description", course.Description);
        cmd.Parameters.AddWithValue("@credits", course.Credits);
        cmd.Parameters.AddWithValue("@teacher", course.Teacher);
        cmd.Parameters.AddWithValue("@semester", course.Semester);
        cmd.Parameters.AddWithValue("@maxStudents", course.MaxStudents);
        cmd.Parameters.AddWithValue("@schedule", course.Schedule);
        cmd.Parameters.AddWithValue("@isActive", course.IsActive);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        await cmd.ExecuteNonQueryAsync();
        return course;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            UPDATE "Courses"
            SET "IsActive" = FALSE,
                "UpdatedAt" = @updatedAt
            WHERE "Id" = @id
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT 1 FROM \"Courses\" WHERE \"Id\" = @id";
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteScalarAsync() is not null;
    }

    private async Task<List<Course>> QueryCoursesAsync(string sql, params NpgsqlParameter[] parameters)
    {
        var items = new List<Course>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new Course
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                Description = reader.GetString(3),
                Credits = reader.GetInt32(4),
                Teacher = reader.GetString(5),
                Semester = reader.GetInt32(6),
                MaxStudents = reader.GetInt32(7),
                Schedule = reader.GetString(8),
                IsActive = reader.GetBoolean(9),
                CreatedAt = reader.GetDateTime(10),
                UpdatedAt = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                EnrolledCount = reader.GetInt32(12)
            });
        }

        return items;
    }

    private static void AddCourseParameters(NpgsqlCommand cmd, Course course)
    {
        cmd.Parameters.AddWithValue("@code", course.Code);
        cmd.Parameters.AddWithValue("@name", course.Name);
        cmd.Parameters.AddWithValue("@description", course.Description);
        cmd.Parameters.AddWithValue("@credits", course.Credits);
        cmd.Parameters.AddWithValue("@teacher", course.Teacher);
        cmd.Parameters.AddWithValue("@semester", course.Semester);
        cmd.Parameters.AddWithValue("@maxStudents", course.MaxStudents);
        cmd.Parameters.AddWithValue("@schedule", course.Schedule);
        cmd.Parameters.AddWithValue("@isActive", course.IsActive);
        cmd.Parameters.AddWithValue("@createdAt", course.CreatedAt);
    }

    private async Task<int> ExecuteCountAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}