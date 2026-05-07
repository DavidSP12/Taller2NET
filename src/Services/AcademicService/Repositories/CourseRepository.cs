using Microsoft.EntityFrameworkCore;
using Taller2NET.Shared.Models;
using AcademicService.Data;

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
    private readonly AcademicDbContext _context;

    public CourseRepository(AcademicDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Course> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search) ||
                c.Teacher.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Enrollments)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Course?> GetByIdAsync(int id) =>
        await _context.Courses
            .Include(c => c.Enrollments).ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Course?> GetByCodeAsync(string code) =>
        await _context.Courses.FirstOrDefaultAsync(c => c.Code == code);

    public async Task<Course> CreateAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is null) return false;
        course.IsActive = false;
        course.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Courses.AnyAsync(c => c.Id == id);
}
