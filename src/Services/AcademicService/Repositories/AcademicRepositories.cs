using Microsoft.EntityFrameworkCore;
using Taller2NET.Shared.Models;
using AcademicService.Data;

namespace AcademicService.Repositories;

public interface IEnrollmentRepository
{
    Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment> UpdateAsync(Enrollment enrollment);
}

public interface IAttendanceRepository
{
    Task<IEnumerable<Attendance>> GetByEnrollmentAsync(int enrollmentId);
    Task<Attendance> CreateAsync(Attendance attendance);
    Task<IEnumerable<Attendance>> CreateRangeAsync(IEnumerable<Attendance> attendances);
}

public interface IGradeRepository
{
    Task<IEnumerable<Grade>> GetByEnrollmentAsync(int enrollmentId);
    Task<Grade> CreateAsync(Grade grade);
    Task<Grade> UpdateAsync(Grade grade);
}

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AcademicDbContext _context;

    public EnrollmentRepository(AcademicDbContext context) => _context = context;

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId) =>
        await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(int courseId) =>
        await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();

    public async Task<Enrollment?> GetByIdAsync(int id) =>
        await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId) =>
        await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }
}

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AcademicDbContext _context;

    public AttendanceRepository(AcademicDbContext context) => _context = context;

    public async Task<IEnumerable<Attendance>> GetByEnrollmentAsync(int enrollmentId) =>
        await _context.Attendances
            .Where(a => a.EnrollmentId == enrollmentId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

    public async Task<Attendance> CreateAsync(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<IEnumerable<Attendance>> CreateRangeAsync(IEnumerable<Attendance> attendances)
    {
        var list = attendances.ToList();
        _context.Attendances.AddRange(list);
        await _context.SaveChangesAsync();
        return list;
    }
}

public class GradeRepository : IGradeRepository
{
    private readonly AcademicDbContext _context;

    public GradeRepository(AcademicDbContext context) => _context = context;

    public async Task<IEnumerable<Grade>> GetByEnrollmentAsync(int enrollmentId) =>
        await _context.Grades
            .Where(g => g.EnrollmentId == enrollmentId)
            .OrderByDescending(g => g.EvaluatedAt)
            .ToListAsync();

    public async Task<Grade> CreateAsync(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task<Grade> UpdateAsync(Grade grade)
    {
        _context.Grades.Update(grade);
        await _context.SaveChangesAsync();
        return grade;
    }
}
