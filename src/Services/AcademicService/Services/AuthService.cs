using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Taller2NET.Shared.DTOs;
using Taller2NET.Shared.Models;
using AcademicService.Repositories;

namespace AcademicService.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IStudentRepository studentRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _studentRepo = studentRepo;
        _config = config;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByUsernameAsync(dto.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;
        if (!user.IsActive) return null;

        return GenerateToken(user);
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.GetByUsernameAsync(dto.Username) is not null) return null;
        if (await _userRepo.GetByEmailAsync(dto.Email) is not null) return null;

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Student",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user);

        var student = new Student
        {
            UserId = user.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            StudentCode = dto.StudentCode,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            Program = dto.Program,
            Semester = dto.Semester,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _studentRepo.CreateAsync(student);

        return GenerateToken(user);
    }

    private AuthResponseDto GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponseDto(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            Username: user.Username,
            Email: user.Email,
            Role: user.Role,
            ExpiresAt: expires);
    }
}
