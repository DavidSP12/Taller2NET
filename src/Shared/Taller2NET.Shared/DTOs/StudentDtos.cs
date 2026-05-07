namespace Taller2NET.Shared.DTOs;

public record StudentDto(
    int Id,
    string FirstName,
    string LastName,
    string StudentCode,
    string Email,
    string? Phone,
    DateTime DateOfBirth,
    string Program,
    int Semester,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateStudentDto(
    int UserId,
    string FirstName,
    string LastName,
    string StudentCode,
    string Email,
    string? Phone,
    DateTime DateOfBirth,
    string Program,
    int Semester
);

public record UpdateStudentDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Program,
    int Semester,
    bool IsActive
);
