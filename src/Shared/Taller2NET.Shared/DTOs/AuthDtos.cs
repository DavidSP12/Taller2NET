namespace Taller2NET.Shared.DTOs;

public record LoginDto(string Username, string Password);

public record RegisterDto(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string StudentCode,
    DateTime DateOfBirth,
    string Program,
    int Semester
);

public record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    string Role,
    DateTime ExpiresAt
);
