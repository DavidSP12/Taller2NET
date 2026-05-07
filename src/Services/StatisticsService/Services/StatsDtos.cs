namespace StatisticsService.Services;

public record DashboardSummaryDto(
    int TotalStudents,
    int ActiveStudents,
    int TotalCourses,
    int ActiveCourses,
    int TotalEnrollments,
    int ActiveEnrollments,
    double GlobalAverageGrade,
    double GlobalAttendanceRate,
    DateTime GeneratedAt
);

public record CourseStatsDto(
    int CourseId,
    string CourseCode,
    string CourseName,
    string Teacher,
    int EnrolledCount,
    int MaxStudents,
    double OccupancyRate,
    double AverageGrade,
    double AttendanceRate
);

public record StudentStatsDto(
    int StudentId,
    string StudentCode,
    string FullName,
    string Program,
    int Semester,
    int EnrolledCourses,
    double WeightedAverage,
    double AttendanceRate,
    string PerformanceLevel
);

public record AttendanceSummaryDto(
    int TotalSessions,
    int PresentCount,
    int AbsentCount,
    int LateCount,
    int ExcusedCount,
    double AttendanceRate
);

public record GradeSummaryDto(
    double Average,
    double Highest,
    double Lowest,
    double PassRate,
    int TotalGrades
);

public record TopCourseDto(
    int CourseId,
    string Code,
    string Name,
    int EnrolledCount,
    double AverageGrade
);

public record RecentActivityDto(
    string Type,
    string Description,
    DateTime Timestamp
);

public record ProgramStatsDto(
    string Program,
    int StudentCount,
    double AverageGrade,
    double AttendanceRate
);
