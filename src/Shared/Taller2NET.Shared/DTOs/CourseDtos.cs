namespace Taller2NET.Shared.DTOs;

public record CourseDto(
    int Id,
    string Code,
    string Name,
    string Description,
    int Credits,
    string Teacher,
    int Semester,
    int MaxStudents,
    string Schedule,
    bool IsActive,
    int EnrolledCount
);

public record CreateCourseDto(
    string Code,
    string Name,
    string Description,
    int Credits,
    string Teacher,
    int Semester,
    int MaxStudents,
    string Schedule
);

public record UpdateCourseDto(
    string Name,
    string Description,
    int Credits,
    string Teacher,
    int MaxStudents,
    string Schedule,
    bool IsActive
);
