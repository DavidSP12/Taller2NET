using Npgsql;
using Taller2NET.Shared.Models;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace AcademicService.Repositories;

public interface ICourseRepository
{
    Task<(IEnumerable<Course> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null);
    Task<Course?> GetByIdAsync(int id);
    Task<Course?> GetByCodeAsync(string code);
    Task<Course> CreateAsync(Course course);
    Task<Course> UpdateAsync(Course course);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public class CourseRepository : ICourseRepository
{
    private readonly string _connectionString;

    public CourseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

    public async Task<(IEnumerable<Course> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var items = new List<Course>();
        var where = "WHERE \"IsActive\" = TRUE";
        if (!string.IsNullOrWhiteSpace(search))
        {
            where += " AND (LOWER(\"Name\") LIKE @p OR LOWER(\"Code\") LIKE @p OR LOWER(\"Teacher\") LIKE @p)";
        }

        var offset = (page - 1) * pageSize;
        var countSql = $"SELECT COUNT(*) FROM \"Courses\" {where}";
        var sql = $"SELECT \"Id\", \"Code\", \"Name\", \"Description\", \"Credits\", \"Teacher\", \"Semester\", \"MaxStudents\", \"Schedule\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\" FROM \"Courses\" {where} ORDER BY \"Code\" LIMIT @limit OFFSET @offset";

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
                items.Add(new Course
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Credits = reader.GetInt32(reader.GetOrdinal("Credits")),
                    Teacher = reader.GetString(reader.GetOrdinal("Teacher")),
                    Semester = reader.GetInt32(reader.GetOrdinal("Semester")),
                    MaxStudents = reader.GetInt32(reader.GetOrdinal("MaxStudents")),
                    Schedule = reader.GetString(reader.GetOrdinal("Schedule")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("UpdatedAt"))
                });
            }
        }

        return (items, total);
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        const string sql = "SELECT \"Id\", \"Code\", \"Name\", \"Description\", \"Credits\", \"Teacher\", \"Semester\", \"MaxStudents\", \"Schedule\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\" FROM \"Courses\" WHERE \"Id\" = @id";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new Course
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Credits = reader.GetInt32(reader.GetOrdinal("Credits")),
                Teacher = reader.GetString(reader.GetOrdinal("Teacher")),
                Semester = reader.GetInt32(reader.GetOrdinal("Semester")),
                MaxStudents = reader.GetInt32(reader.GetOrdinal("MaxStudents")),
                Schedule = reader.GetString(reader.GetOrdinal("Schedule")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("UpdatedAt"))
            };
        }

        return null;
    }

    public async Task<Course?> GetByCodeAsync(string code)
    {
        const string sql = "SELECT \"Id\", \"Code\", \"Name\" FROM \"Courses\" WHERE \"Code\" = @code";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@code", NpgsqlTypes.NpgsqlDbType.Text, code);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new Course
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            };
        }

        return null;
    }

    public async Task<Course> CreateAsync(Course course)
    {
        const string sql = "INSERT INTO \"Courses\" (\"Code\", \"Name\", \"Description\", \"Credits\", \"Teacher\", \"Semester\", \"MaxStudents\", \"Schedule\", \"IsActive\", \"CreatedAt\") VALUES (@code,@name,@desc,@credits,@teacher,@semester,@max,@schedule,@isActive,@created) RETURNING \"Id\"";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@code", NpgsqlTypes.NpgsqlDbType.Text, course.Code);
        cmd.Parameters.AddWithValue("@name", NpgsqlTypes.NpgsqlDbType.Text, course.Name);
        cmd.Parameters.AddWithValue("@desc", NpgsqlTypes.NpgsqlDbType.Text, course.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("@credits", NpgsqlTypes.NpgsqlDbType.Integer, course.Credits);
        cmd.Parameters.AddWithValue("@teacher", NpgsqlTypes.NpgsqlDbType.Text, course.Teacher);
        cmd.Parameters.AddWithValue("@semester", NpgsqlTypes.NpgsqlDbType.Integer, course.Semester);
        cmd.Parameters.AddWithValue("@max", NpgsqlTypes.NpgsqlDbType.Integer, course.MaxStudents);
        cmd.Parameters.AddWithValue("@schedule", NpgsqlTypes.NpgsqlDbType.Text, course.Schedule ?? string.Empty);
        cmd.Parameters.AddWithValue("@isActive", NpgsqlTypes.NpgsqlDbType.Boolean, course.IsActive);
        cmd.Parameters.AddWithValue("@created", NpgsqlTypes.NpgsqlDbType.TimestampTz, course.CreatedAt);

        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        course.Id = newId;
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        const string sql = "UPDATE \"Courses\" SET \"Code\" = @code, \"Name\" = @name, \"Description\" = @desc, \"Credits\" = @credits, \"Teacher\" = @teacher, \"Semester\" = @semester, \"MaxStudents\" = @max, \"Schedule\" = @schedule, \"IsActive\" = @isActive, \"UpdatedAt\" = @updated WHERE \"Id\" = @id";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@code", NpgsqlTypes.NpgsqlDbType.Text, course.Code);
        cmd.Parameters.AddWithValue("@name", NpgsqlTypes.NpgsqlDbType.Text, course.Name);
        cmd.Parameters.AddWithValue("@desc", NpgsqlTypes.NpgsqlDbType.Text, course.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("@credits", NpgsqlTypes.NpgsqlDbType.Integer, course.Credits);
        cmd.Parameters.AddWithValue("@teacher", NpgsqlTypes.NpgsqlDbType.Text, course.Teacher);
        cmd.Parameters.AddWithValue("@semester", NpgsqlTypes.NpgsqlDbType.Integer, course.Semester);
        cmd.Parameters.AddWithValue("@max", NpgsqlTypes.NpgsqlDbType.Integer, course.MaxStudents);
        cmd.Parameters.AddWithValue("@schedule", NpgsqlTypes.NpgsqlDbType.Text, course.Schedule ?? string.Empty);
        cmd.Parameters.AddWithValue("@isActive", NpgsqlTypes.NpgsqlDbType.Boolean, course.IsActive);
        cmd.Parameters.AddWithValue("@updated", NpgsqlTypes.NpgsqlDbType.TimestampTz, course.UpdatedAt ?? DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, course.Id);

        await cmd.ExecuteNonQueryAsync();
        return course;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "UPDATE \"Courses\" SET \"IsActive\" = FALSE, \"UpdatedAt\" = @updated WHERE \"Id\" = @id";
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
        const string sql = "SELECT EXISTS(SELECT 1 FROM \"Courses\" WHERE \"Id\" = @id)";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        return Convert.ToBoolean(await cmd.ExecuteScalarAsync());
    }
}
