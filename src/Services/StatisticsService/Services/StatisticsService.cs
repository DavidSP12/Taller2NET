using Microsoft.EntityFrameworkCore;
using Taller2NET.Shared.Models;
using StatisticsService.Data;

namespace StatisticsService.Services;

public interface IStatisticsService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<IEnumerable<CourseStatsDto>> GetCourseStatsAsync();
    Task<IEnumerable<StudentStatsDto>> GetStudentStatsAsync(int page = 1, int pageSize = 20);
    Task<StudentStatsDto?> GetStudentStatsByIdAsync(int studentId);
    Task<IEnumerable<TopCourseDto>> GetTopCoursesAsync(int top = 5);
    Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int? courseId = null);
    Task<GradeSummaryDto> GetGradeSummaryAsync(int? courseId = null);
    Task<IEnumerable<RecentActivityDto>> GetRecentActivityAsync(int count = 10);
    Task<IEnumerable<ProgramStatsDto>> GetProgramStatsAsync();
}

public class StatisticsService : IStatisticsService
{
    private readonly StatisticsDbContext _context;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(StatisticsDbContext context, ILogger<StatisticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        _logger.LogInformation("Generating dashboard summary");

        var totalStudents = await _context.Students.CountAsync();
        var activeStudents = await _context.Students.CountAsync(s => s.IsActive);
        var totalCourses = await _context.Courses.CountAsync();
        var activeCourses = await _context.Courses.CountAsync(c => c.IsActive);
        var totalEnrollments = await _context.Enrollments.CountAsync();
        var activeEnrollments = await _context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Active);

        var grades = await _context.Grades.Select(g => (double)g.Value).ToListAsync();
        var globalAvg = grades.Any() ? grades.Average() : 0;

        var attendances = await _context.Attendances.Select(a => a.Status).ToListAsync();
        var attendanceRate = attendances.Any()
            ? (double)attendances.Count(a => a == AttendanceStatus.Present || a == AttendanceStatus.Late) / attendances.Count * 100
            : 0;

        return new DashboardSummaryDto(
            totalStudents, activeStudents,
            totalCourses, activeCourses,
            totalEnrollments, activeEnrollments,
            Math.Round(globalAvg, 2),
            Math.Round(attendanceRate, 2),
            DateTime.UtcNow);
    }

    public async Task<IEnumerable<CourseStatsDto>> GetCourseStatsAsync()
    {
        var courses = await _context.Courses
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Grades)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Attendances)
            .Where(c => c.IsActive)
            .ToListAsync();

        return courses.Select(c =>
        {
            var activeEnrollments = c.Enrollments.Where(e => e.Status == EnrollmentStatus.Active).ToList();
            var allGrades = activeEnrollments.SelectMany(e => e.Grades).Select(g => (double)g.Value).ToList();
            var allAttendances = activeEnrollments.SelectMany(e => e.Attendances).ToList();

            var avgGrade = allGrades.Any() ? allGrades.Average() : 0;
            var attendanceRate = allAttendances.Any()
                ? (double)allAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                  / allAttendances.Count * 100
                : 0;

            return new CourseStatsDto(
                c.Id, c.Code, c.Name, c.Teacher,
                activeEnrollments.Count, c.MaxStudents,
                c.MaxStudents > 0 ? Math.Round((double)activeEnrollments.Count / c.MaxStudents * 100, 2) : 0,
                Math.Round(avgGrade, 2),
                Math.Round(attendanceRate, 2));
        });
    }

    public async Task<IEnumerable<StudentStatsDto>> GetStudentStatsAsync(int page = 1, int pageSize = 20)
    {
        var students = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Grades)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Attendances)
            .Where(s => s.IsActive)
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return students.Select(s => BuildStudentStats(s));
    }

    public async Task<StudentStatsDto?> GetStudentStatsByIdAsync(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Grades)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Attendances)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        return student is null ? null : BuildStudentStats(student);
    }

    public async Task<IEnumerable<TopCourseDto>> GetTopCoursesAsync(int top = 5)
    {
        var courses = await _context.Courses
            .Include(c => c.Enrollments).ThenInclude(e => e.Grades)
            .Where(c => c.IsActive)
            .ToListAsync();

        return courses
            .Select(c =>
            {
                var activeEnrollments = c.Enrollments.Where(e => e.Status == EnrollmentStatus.Active).ToList();
                var avgGrade = activeEnrollments.SelectMany(e => e.Grades).Any()
                    ? activeEnrollments.SelectMany(e => e.Grades).Average(g => (double)g.Value)
                    : 0;
                return new TopCourseDto(c.Id, c.Code, c.Name, activeEnrollments.Count, Math.Round(avgGrade, 2));
            })
            .OrderByDescending(c => c.EnrolledCount)
            .Take(top);
    }

    public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int? courseId = null)
    {
        var query = _context.Attendances.AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(a => a.Enrollment!.CourseId == courseId.Value);
        }

        var attendances = await query.Select(a => a.Status).ToListAsync();
        var total = attendances.Count;
        if (total == 0)
            return new AttendanceSummaryDto(0, 0, 0, 0, 0, 0);

        var presentCount = attendances.Count(a => a == AttendanceStatus.Present);
        var absentCount = attendances.Count(a => a == AttendanceStatus.Absent);
        var lateCount = attendances.Count(a => a == AttendanceStatus.Late);
        var excusedCount = attendances.Count(a => a == AttendanceStatus.Excused);
        var rate = Math.Round((double)(presentCount + lateCount) / total * 100, 2);

        return new AttendanceSummaryDto(total, presentCount, absentCount, lateCount, excusedCount, rate);
    }

    public async Task<GradeSummaryDto> GetGradeSummaryAsync(int? courseId = null)
    {
        var query = _context.Grades.AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(g => g.Enrollment!.CourseId == courseId.Value);
        }

        var grades = await query.Select(g => (double)g.Value).ToListAsync();
        if (!grades.Any())
            return new GradeSummaryDto(0, 0, 0, 0, 0);

        return new GradeSummaryDto(
            Math.Round(grades.Average(), 2),
            Math.Round(grades.Max(), 2),
            Math.Round(grades.Min(), 2),
            Math.Round((double)grades.Count(g => g >= PassThreshold) / grades.Count * 100, 2),
            grades.Count);
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentActivityAsync(int count = 10)
    {
        var recentEnrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .Select(e => new RecentActivityDto(
                "Enrollment",
                $"{e.Student!.FirstName} {e.Student.LastName} enrolled in {e.Course!.Name}",
                e.CreatedAt))
            .ToListAsync();

        var recentGrades = await _context.Grades
            .Include(g => g.Enrollment).ThenInclude(e => e!.Student)
            .Include(g => g.Enrollment).ThenInclude(e => e!.Course)
            .OrderByDescending(g => g.CreatedAt)
            .Take(count)
            .Select(g => new RecentActivityDto(
                "Grade",
                $"Grade {g.Value:F1} ({g.Type}) for {g.Enrollment!.Student!.FirstName} {g.Enrollment.Student.LastName} in {g.Enrollment.Course!.Name}",
                g.CreatedAt))
            .ToListAsync();

        return recentEnrollments.Concat(recentGrades)
            .OrderByDescending(a => a.Timestamp)
            .Take(count);
    }

    public async Task<IEnumerable<ProgramStatsDto>> GetProgramStatsAsync()
    {
        var students = await _context.Students
            .Include(s => s.Enrollments).ThenInclude(e => e.Grades)
            .Include(s => s.Enrollments).ThenInclude(e => e.Attendances)
            .Where(s => s.IsActive)
            .ToListAsync();

        return students.GroupBy(s => s.Program).Select(g =>
        {
            var allGrades = g.SelectMany(s => s.Enrollments.SelectMany(e => e.Grades)).Select(gr => (double)gr.Value).ToList();
            var allAttendances = g.SelectMany(s => s.Enrollments.SelectMany(e => e.Attendances)).ToList();
            var avgGrade = allGrades.Any() ? allGrades.Average() : 0;
            var attendanceRate = allAttendances.Any()
                ? (double)allAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                  / allAttendances.Count * 100
                : 0;

            return new ProgramStatsDto(g.Key, g.Count(), Math.Round(avgGrade, 2), Math.Round(attendanceRate, 2));
        }).OrderByDescending(p => p.StudentCount);
    }

    private static readonly double PassThreshold = 3.0;

    private static readonly (double MinGrade, string Label)[] PerformanceLevels =
    {
        (4.5, "Excelente"),
        (3.5, "Bueno"),
        (3.0, "Aprobado"),
        (2.0, "En riesgo")
    };

    private static StudentStatsDto BuildStudentStats(Student s)
    {
        var activeEnrollments = s.Enrollments.Where(e => e.Status == EnrollmentStatus.Active).ToList();
        var allGrades = activeEnrollments.SelectMany(e => e.Grades).ToList();
        var allAttendances = activeEnrollments.SelectMany(e => e.Attendances).ToList();

        double weightedAvg = 0;
        if (allGrades.Any())
        {
            var totalWeight = allGrades.Sum(g => (double)g.Weight);
            weightedAvg = totalWeight > 0
                ? allGrades.Sum(g => (double)g.Value * (double)g.Weight) / totalWeight
                : allGrades.Average(g => (double)g.Value);
        }

        var attendanceRate = allAttendances.Any()
            ? (double)allAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
              / allAttendances.Count * 100
            : 0;

        var performance = PerformanceLevels
            .FirstOrDefault(p => weightedAvg >= p.MinGrade).Label ?? "Reprobado";

        return new StudentStatsDto(
            s.Id, s.StudentCode, $"{s.FirstName} {s.LastName}",
            s.Program, s.Semester, activeEnrollments.Count,
            Math.Round(weightedAvg, 2),
            Math.Round(attendanceRate, 2),
            performance);
    }
}
