using Taller2NET.Shared.Models;

namespace Taller2NET.Shared.DTOs;

public record EnrollmentDto(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string CourseCode,
    EnrollmentStatus Status,
    DateTime EnrolledAt,
    DateTime? WithdrawnAt
);

public record CreateEnrollmentDto(
    int StudentId,
    int CourseId
);

public record UpdateEnrollmentDto(
    EnrollmentStatus Status,
    DateTime? WithdrawnAt
);

public record AttendanceDto(
    int Id,
    int EnrollmentId,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    DateTime Date,
    AttendanceStatus Status,
    string? Notes
);

public record CreateAttendanceDto(
    int EnrollmentId,
    DateTime Date,
    AttendanceStatus Status,
    string? Notes
);

public record UpdateAttendanceDto(
    DateTime Date,
    AttendanceStatus Status,
    string? Notes
);

public record GradeDto(
    int Id,
    int EnrollmentId,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    GradeType Type,
    decimal Value,
    decimal Weight,
    string? Description,
    DateTime EvaluatedAt
);

public record CreateGradeDto(
    int EnrollmentId,
    GradeType Type,
    decimal Value,
    decimal Weight,
    string? Description,
    DateTime EvaluatedAt
);

public record UpdateGradeDto(
    GradeType Type,
    decimal Value,
    decimal Weight,
    string? Description,
    DateTime EvaluatedAt
);
