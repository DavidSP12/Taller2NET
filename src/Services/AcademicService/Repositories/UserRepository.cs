using Npgsql;
using Taller2NET.Shared.Models;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace AcademicService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required");
    }

    private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = "SELECT \"Id\", \"Username\", \"Email\", \"PasswordHash\", \"Role\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\" FROM \"Users\" WHERE \"Username\" = @username";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", NpgsqlTypes.NpgsqlDbType.Text, username);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("UpdatedAt"))
            };
        }
        return null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT \"Id\", \"Username\", \"Email\", \"PasswordHash\", \"Role\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\" FROM \"Users\" WHERE \"Email\" = @email";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", NpgsqlTypes.NpgsqlDbType.Text, email);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("UpdatedAt"))
            };
        }
        return null;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = "SELECT \"Id\", \"Username\", \"Email\", \"PasswordHash\", \"Role\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\" FROM \"Users\" WHERE \"Id\" = @id";
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Integer, id);
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetFieldValue<DateTime?>(reader.GetOrdinal("UpdatedAt"))
            };
        }
        return null;
    }

    public async Task<User> CreateAsync(User user)
    {
        const string sql = "INSERT INTO \"Users\" (\"Username\", \"Email\", \"PasswordHash\", \"Role\", \"IsActive\", \"CreatedAt\") VALUES (@username,@email,@pwd,@role,@isActive,@created) RETURNING \"Id\"";

        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", NpgsqlTypes.NpgsqlDbType.Text, user.Username);
        cmd.Parameters.AddWithValue("@email", NpgsqlTypes.NpgsqlDbType.Text, user.Email);
        cmd.Parameters.AddWithValue("@pwd", NpgsqlTypes.NpgsqlDbType.Text, user.PasswordHash);
        cmd.Parameters.AddWithValue("@role", NpgsqlTypes.NpgsqlDbType.Text, user.Role);
        cmd.Parameters.AddWithValue("@isActive", NpgsqlTypes.NpgsqlDbType.Boolean, user.IsActive);
        cmd.Parameters.AddWithValue("@created", NpgsqlTypes.NpgsqlDbType.TimestampTz, user.CreatedAt);

        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        user.Id = newId;
        return user;
    }
}
