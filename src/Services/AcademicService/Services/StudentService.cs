using Taller2NET.Shared.DTOs;
using Taller2NET.Shared.Models;
using Taller2NET.Shared.Responses;
using AcademicService.Repositories;

namespace AcademicService.Services;

public interface IStudentService
{
    Task<PagedResponse<StudentDto>> GetAllAsync(int page, int pageSize, string? search = null);
    Task<ApiResponse<StudentDto>> GetByIdAsync(int id);
    Task<ApiResponse<StudentDto>> CreateAsync(CreateStudentDto dto);
    Task<ApiResponse<StudentDto>> UpdateAsync(int id, UpdateStudentDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo) => _repo = repo;

    public async Task<PagedResponse<StudentDto>> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var (items, total) = await _repo.GetAllAsync(page, pageSize, search);
        return new PagedResponse<StudentDto>
        {
            Success = true,
            Message = "Students retrieved",
            Data = items.Select(MapToDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ApiResponse<StudentDto>> GetByIdAsync(int id)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null) return ApiResponse<StudentDto>.Fail("Student not found");
        return ApiResponse<StudentDto>.Ok(MapToDto(student));
    }

    public async Task<ApiResponse<StudentDto>> CreateAsync(CreateStudentDto dto)
    {
        var existing = await _repo.GetByCodeAsync(dto.StudentCode);
        if (existing is not null) return ApiResponse<StudentDto>.Fail("Student code already exists");

        var student = new Student
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            StudentCode = dto.StudentCode,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Program = dto.Program,
            Semester = dto.Semester,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(student);
        return ApiResponse<StudentDto>.Ok(MapToDto(created), "Student created successfully");
    }

    public async Task<ApiResponse<StudentDto>> UpdateAsync(int id, UpdateStudentDto dto)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null) return ApiResponse<StudentDto>.Fail("Student not found");

        student.FirstName = dto.FirstName;
        student.LastName = dto.LastName;
        student.Email = dto.Email;
        student.Phone = dto.Phone;
        student.Program = dto.Program;
        student.Semester = dto.Semester;
        student.IsActive = dto.IsActive;

        var updated = await _repo.UpdateAsync(student);
        return ApiResponse<StudentDto>.Ok(MapToDto(updated), "Student updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        if (!result) return ApiResponse<bool>.Fail("Student not found");
        return ApiResponse<bool>.Ok(true, "Student deactivated successfully");
    }

    private static StudentDto MapToDto(Student s) => new(
        s.Id, s.FirstName, s.LastName, s.StudentCode, s.Email,
        s.Phone, s.DateOfBirth, s.Program, s.Semester, s.IsActive, s.CreatedAt);
}
