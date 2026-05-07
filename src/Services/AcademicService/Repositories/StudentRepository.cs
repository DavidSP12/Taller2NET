using Microsoft.EntityFrameworkCore;
using Taller2NET.Shared.Models;
using AcademicService.Data;

namespace AcademicService.Repositories;

public interface IStudentRepository
{
    Task<(IEnumerable<Student> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null);
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByCodeAsync(string code);
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public class StudentRepository : IStudentRepository
{
    private readonly AcademicDbContext _context;

    public StudentRepository(AcademicDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Student> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.Students.Include(s => s.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(search) ||
                s.LastName.ToLower().Contains(search) ||
                s.StudentCode.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Student?> GetByIdAsync(int id) =>
        await _context.Students.Include(s => s.User)
            .Include(s => s.Enrollments).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Student?> GetByCodeAsync(string code) =>
        await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == code);

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        student.UpdatedAt = DateTime.UtcNow;
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null) return false;
        student.IsActive = false;
        student.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _context.Students.AnyAsync(s => s.Id == id);
}
