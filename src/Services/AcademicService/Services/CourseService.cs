using Taller2NET.Shared.DTOs;
using Taller2NET.Shared.Models;
using Taller2NET.Shared.Responses;
using AcademicService.Repositories;

namespace AcademicService.Services;

public interface ICourseService
{
    Task<PagedResponse<CourseDto>> GetAllAsync(int page, int pageSize, string? search = null);
    Task<ApiResponse<CourseDto>> GetByIdAsync(int id);
    Task<ApiResponse<CourseDto>> CreateAsync(CreateCourseDto dto);
    Task<ApiResponse<CourseDto>> UpdateAsync(int id, UpdateCourseDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;

    public CourseService(ICourseRepository repo) => _repo = repo;

    public async Task<PagedResponse<CourseDto>> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var (items, total) = await _repo.GetAllAsync(page, pageSize, search);
        return new PagedResponse<CourseDto>
        {
            Success = true,
            Message = "Courses retrieved",
            Data = items.Select(MapToDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ApiResponse<CourseDto>> GetByIdAsync(int id)
    {
        var course = await _repo.GetByIdAsync(id);
        if (course is null) return ApiResponse<CourseDto>.Fail("Course not found");
        return ApiResponse<CourseDto>.Ok(MapToDto(course));
    }

    public async Task<ApiResponse<CourseDto>> CreateAsync(CreateCourseDto dto)
    {
        var existing = await _repo.GetByCodeAsync(dto.Code);
        if (existing is not null) return ApiResponse<CourseDto>.Fail("Course code already exists");

        var course = new Course
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            Credits = dto.Credits,
            Teacher = dto.Teacher,
            Semester = dto.Semester,
            MaxStudents = dto.MaxStudents,
            Schedule = dto.Schedule,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(course);
        return ApiResponse<CourseDto>.Ok(MapToDto(created), "Course created successfully");
    }

    public async Task<ApiResponse<CourseDto>> UpdateAsync(int id, UpdateCourseDto dto)
    {
        var course = await _repo.GetByIdAsync(id);
        if (course is null) return ApiResponse<CourseDto>.Fail("Course not found");

        course.Name = dto.Name;
        course.Description = dto.Description;
        course.Credits = dto.Credits;
        course.Teacher = dto.Teacher;
        course.MaxStudents = dto.MaxStudents;
        course.Schedule = dto.Schedule;
        course.IsActive = dto.IsActive;

        var updated = await _repo.UpdateAsync(course);
        return ApiResponse<CourseDto>.Ok(MapToDto(updated), "Course updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        if (!result) return ApiResponse<bool>.Fail("Course not found");
        return ApiResponse<bool>.Ok(true, "Course deactivated successfully");
    }

    private static CourseDto MapToDto(Course c) => new(
        c.Id, c.Code, c.Name, c.Description, c.Credits, c.Teacher, c.Semester,
        c.MaxStudents, c.Schedule, c.IsActive,
        c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active));
}
