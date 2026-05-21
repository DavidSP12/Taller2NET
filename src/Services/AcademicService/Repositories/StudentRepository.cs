using Npgsql;
using Taller2NET.Shared.Models;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace AcademicService.Repositories;

public interface IStudentRepository
{
    Task<(IEnumerable<Student> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null);
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByCodeAsync(string code);
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public class StudentRepository : IStudentRepository
{
    private readonly string _connectionString;

    public StudentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

    public async Task<(IEnumerable<Student> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var items = new List<Student>();
        var where = "WHERE \"IsActive\" = TRUE";
        if (!string.IsNullOrWhiteSpace(search))
        {
            where += " AND (LOWER(\"FirstName\") LIKE @p OR LOWER(\"LastName\") LIKE @p OR LOWER(\"StudentCode\") LIKE @p OR LOWER(\"Email\") LIKE @p)";
        }

        var offset = (page - 1) * pageSize;
        var countSql = $"SELECT COUNT(*) FROM \"Students\" {where}";
        var sql = $"SELECT \"Id\", \"UserId\", \"FirstName\", \"LastName\", \"StudentCode\", \"Email\", \"Phone\", \"DateOfBirth\", \"Program\", \"Semester\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\" FROM \"Students\" {where} ORDER BY \"LastName\", \"FirstName\" LIMIT @limit OFFSET @offset";

        using var conn = GetConnection();
        await conn.OpenAsync();

        int total = 0;
        using (var countCmd = new NpgsqlCommand(countSql, conn))
        {
            if (!string.IsNullOrWhiteSpace(search)) countCmd.Parameters.AddWithValue("@p", NpgsqlTypes.NpgsqlDbType.Text, $"%{search.ToLower()}%");
            total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
        }

        using (var cmd = new NpgsqlCommand(sql, conn))
        {
            if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@p", NpgsqlTypes.NpgsqlDbType.Text, $"%{search.ToLower()}%");
            cmd.Parameters.AddWithValue("@limit", NpgsqlTypes.NpgsqlDbType.Integer, pageSize);
            cmd.Parameters.AddWithValue("@offset", NpgsqlTypes.NpgsqlDbType.Integer, offset);

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            while (await reader.ReadAsync())
            {
                items.Add(new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    StudentCode = reader.GetString(reader.GetOrdinal("StudentCode")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    DateOfBirth = reader.GetFieldValue<DateTime>(reader.GetOrdinal("DateOfBirth")),
                    Program = reader.GetString(reader.GetOrdinal("Program")),
                    Semester = reader.GetInt32(reader.GetOrdinal("Semester")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("UpdatedAt"))
                });
            }
        }

        return (items, total);
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        const string sql = "SELECT \"Id\", \"UserId\", \"FirstName\", \"LastName\", \"StudentCode\", \"Email\", \"Phone\", \"DateOfBirth\", \"Program\", \"Semester\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\" FROM \"Students\" WHERE \"Id\" = @id";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new Student
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                StudentCode = reader.GetString(reader.GetOrdinal("StudentCode")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                DateOfBirth = reader.GetFieldValue<DateTime>(reader.GetOrdinal("DateOfBirth")),
                Program = reader.GetString(reader.GetOrdinal("Program")),
                Semester = reader.GetInt32(reader.GetOrdinal("Semester")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("UpdatedAt"))
            };
        }

        return null;
    }

    public async Task<Student?> GetByCodeAsync(string code)
    {
        const string sql = "SELECT \"Id\", \"StudentCode\", \"FirstName\", \"LastName\" FROM \"Students\" WHERE \"StudentCode\" = @code";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@code", NpgsqlTypes.NpgsqlDbType.Text, code);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new Student
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StudentCode = reader.GetString(reader.GetOrdinal("StudentCode")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName"))
            };
        }

        return null;
    }

    public async Task<Student> CreateAsync(Student student)
    {
        const string sql = "INSERT INTO \"Students\" (\"UserId\", \"FirstName\", \"LastName\", \"StudentCode\", \"Email\", \"Phone\", \"DateOfBirth\", \"Program\", \"Semester\", \"IsActive\", \"CreatedAt\") VALUES (@userId,@first,@last,@code,@email,@phone,@dob,@program,@semester,@isActive,@created) RETURNING \"Id\"";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", NpgsqlTypes.NpgsqlDbType.Integer, student.UserId);
        cmd.Parameters.AddWithValue("@first", NpgsqlTypes.NpgsqlDbType.Text, student.FirstName);
        cmd.Parameters.AddWithValue("@last", NpgsqlTypes.NpgsqlDbType.Text, student.LastName);
        cmd.Parameters.AddWithValue("@code", NpgsqlTypes.NpgsqlDbType.Text, student.StudentCode);
        cmd.Parameters.AddWithValue("@email", NpgsqlTypes.NpgsqlDbType.Text, student.Email);
        cmd.Parameters.AddWithValue("@phone", NpgsqlTypes.NpgsqlDbType.Text, (object?)student.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dob", NpgsqlTypes.NpgsqlDbType.TimestampTz, student.DateOfBirth);
        cmd.Parameters.AddWithValue("@program", NpgsqlTypes.NpgsqlDbType.Text, student.Program);
        cmd.Parameters.AddWithValue("@semester", NpgsqlTypes.NpgsqlDbType.Integer, student.Semester);
        cmd.Parameters.AddWithValue("@isActive", NpgsqlTypes.NpgsqlDbType.Boolean, student.IsActive);
        cmd.Parameters.AddWithValue("@created", NpgsqlTypes.NpgsqlDbType.TimestampTz, student.CreatedAt);

        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        student.Id = newId;
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        student.UpdatedAt = DateTime.UtcNow;
        const string sql = "UPDATE \"Students\" SET \"FirstName\" = @first, \"LastName\" = @last, \"StudentCode\" = @code, \"Email\" = @email, \"Phone\" = @phone, \"DateOfBirth\" = @dob, \"Program\" = @program, \"Semester\" = @semester, \"IsActive\" = @isActive, \"UpdatedAt\" = @updated WHERE \"Id\" = @id";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@first", NpgsqlTypes.NpgsqlDbType.Text, student.FirstName);
        cmd.Parameters.AddWithValue("@last", NpgsqlTypes.NpgsqlDbType.Text, student.LastName);
        cmd.Parameters.AddWithValue("@code", NpgsqlTypes.NpgsqlDbType.Text, student.StudentCode);
        cmd.Parameters.AddWithValue("@email", NpgsqlTypes.NpgsqlDbType.Text, student.Email);
        cmd.Parameters.AddWithValue("@phone", NpgsqlTypes.NpgsqlDbType.Text, (object?)student.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dob", NpgsqlTypes.NpgsqlDbType.TimestampTz, student.DateOfBirth);
        cmd.Parameters.AddWithValue("@program", NpgsqlTypes.NpgsqlDbType.Text, student.Program);
        cmd.Parameters.AddWithValue("@semester", NpgsqlTypes.NpgsqlDbType.Integer, student.Semester);
        cmd.Parameters.AddWithValue("@isActive", NpgsqlTypes.NpgsqlDbType.Boolean, student.IsActive);
        cmd.Parameters.AddWithValue("@updated", NpgsqlTypes.NpgsqlDbType.TimestampTz, student.UpdatedAt ?? DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, student.Id);

        await cmd.ExecuteNonQueryAsync();
        return student;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "UPDATE \"Students\" SET \"IsActive\" = FALSE, \"UpdatedAt\" = @updated WHERE \"Id\" = @id";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@updated", NpgsqlTypes.NpgsqlDbType.TimestampTz, DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        var affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM \"Students\" WHERE \"Id\" = @id)";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        return Convert.ToBoolean(await cmd.ExecuteScalarAsync());
    }
}
