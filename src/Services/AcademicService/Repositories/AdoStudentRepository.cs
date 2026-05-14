using Npgsql;
using Taller2NET.Shared.Models;

namespace AcademicService.Repositories;

public class AdoStudentRepository : IStudentRepository
{
    private readonly string _connectionString;

    public AdoStudentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    public async Task<(IEnumerable<Student> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var parameters = new List<NpgsqlParameter>();
        var whereClause = string.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause = "WHERE (s.\"FirstName\" ILIKE @search OR s.\"LastName\" ILIKE @search OR s.\"StudentCode\" ILIKE @search OR s.\"Email\" ILIKE @search)";
            parameters.Add(new NpgsqlParameter("@search", $"%{search}%"));
        }

        var totalCount = await ExecuteCountAsync($"SELECT COUNT(*) FROM \"Students\" s {whereClause}", parameters.ToArray());

        var sql = $"""
            SELECT s."Id", s."UserId", s."FirstName", s."LastName", s."StudentCode", s."Email", s."Phone",
                   s."DateOfBirth", s."Program", s."Semester", s."IsActive", s."CreatedAt", s."UpdatedAt"
            FROM "Students" s
            {whereClause}
            ORDER BY s."LastName", s."FirstName"
            LIMIT @limit OFFSET @offset
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        if (!string.IsNullOrWhiteSpace(search))
        {
            cmd.Parameters.AddWithValue("@search", $"%{search}%");
        }
        cmd.Parameters.AddWithValue("@limit", pageSize);
        cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

        var items = new List<Student>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(MapStudent(reader));
        }

        return (items, totalCount);
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT s."Id", s."UserId", s."FirstName", s."LastName", s."StudentCode", s."Email", s."Phone",
                   s."DateOfBirth", s."Program", s."Semester", s."IsActive", s."CreatedAt", s."UpdatedAt"
            FROM "Students" s
            WHERE s."Id" = @id
            """;

        var items = await QueryStudentsAsync(sql, new NpgsqlParameter("@id", id));
        return items.FirstOrDefault();
    }

    public async Task<Student?> GetByCodeAsync(string code)
    {
        const string sql = """
            SELECT s."Id", s."UserId", s."FirstName", s."LastName", s."StudentCode", s."Email", s."Phone",
                   s."DateOfBirth", s."Program", s."Semester", s."IsActive", s."CreatedAt", s."UpdatedAt"
            FROM "Students" s
            WHERE s."StudentCode" = @code
            """;

        var items = await QueryStudentsAsync(sql, new NpgsqlParameter("@code", code));
        return items.FirstOrDefault();
    }

    public async Task<Student> CreateAsync(Student student)
    {
        const string sql = """
            INSERT INTO "Students" ("UserId", "FirstName", "LastName", "StudentCode", "Email", "Phone", "DateOfBirth", "Program", "Semester", "IsActive", "CreatedAt")
            VALUES (@userId, @firstName, @lastName, @studentCode, @email, @phone, @dateOfBirth, @program, @semester, @isActive, @createdAt)
            RETURNING "Id"
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        AddStudentParameters(cmd, student);
        student.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        const string sql = """
            UPDATE "Students"
            SET "FirstName" = @firstName,
                "LastName" = @lastName,
                "Email" = @email,
                "Phone" = @phone,
                "DateOfBirth" = @dateOfBirth,
                "Program" = @program,
                "Semester" = @semester,
                "IsActive" = @isActive,
                "UpdatedAt" = @updatedAt
            WHERE "Id" = @id
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", student.Id);
        cmd.Parameters.AddWithValue("@firstName", student.FirstName);
        cmd.Parameters.AddWithValue("@lastName", student.LastName);
        cmd.Parameters.AddWithValue("@email", student.Email);
        cmd.Parameters.AddWithValue("@phone", (object?)student.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dateOfBirth", student.DateOfBirth);
        cmd.Parameters.AddWithValue("@program", student.Program);
        cmd.Parameters.AddWithValue("@semester", student.Semester);
        cmd.Parameters.AddWithValue("@isActive", student.IsActive);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        await cmd.ExecuteNonQueryAsync();
        return student;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            UPDATE "Students"
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
        const string sql = "SELECT 1 FROM \"Students\" WHERE \"Id\" = @id";
        return await ExecuteExistsAsync(sql, new NpgsqlParameter("@id", id));
    }

    private async Task<List<Student>> QueryStudentsAsync(string sql, params NpgsqlParameter[] parameters)
    {
        var items = new List<Student>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(MapStudent(reader));
        }

        return items;
    }

    private static Student MapStudent(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        UserId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
        FirstName = reader.GetString(2),
        LastName = reader.GetString(3),
        StudentCode = reader.GetString(4),
        Email = reader.GetString(5),
        Phone = reader.IsDBNull(6) ? null : reader.GetString(6),
        DateOfBirth = reader.GetDateTime(7),
        Program = reader.GetString(8),
        Semester = reader.GetInt32(9),
        IsActive = reader.GetBoolean(10),
        CreatedAt = reader.GetDateTime(11),
        UpdatedAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12)
    };

    private static void AddStudentParameters(NpgsqlCommand cmd, Student student)
    {
        cmd.Parameters.AddWithValue("@userId", student.UserId);
        cmd.Parameters.AddWithValue("@firstName", student.FirstName);
        cmd.Parameters.AddWithValue("@lastName", student.LastName);
        cmd.Parameters.AddWithValue("@studentCode", student.StudentCode);
        cmd.Parameters.AddWithValue("@email", student.Email);
        cmd.Parameters.AddWithValue("@phone", (object?)student.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dateOfBirth", student.DateOfBirth);
        cmd.Parameters.AddWithValue("@program", student.Program);
        cmd.Parameters.AddWithValue("@semester", student.Semester);
        cmd.Parameters.AddWithValue("@isActive", student.IsActive);
        cmd.Parameters.AddWithValue("@createdAt", student.CreatedAt);
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

    private async Task<bool> ExecuteExistsAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        var result = await cmd.ExecuteScalarAsync();
        return result is not null;
    }
}