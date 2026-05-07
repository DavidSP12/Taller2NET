using Taller2NET.Shared.DTOs;
using Taller2NET.Shared.Models;
using Taller2NET.Shared.Responses;
using AcademicService.Repositories;

namespace AcademicService.Services;

public interface IEnrollmentService
{
    Task<ApiResponse<EnrollmentDto>> EnrollAsync(CreateEnrollmentDto dto);
    Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetByStudentAsync(int studentId);
    Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetByCourseAsync(int courseId);
    Task<ApiResponse<bool>> WithdrawAsync(int enrollmentId);
}

public interface IAttendanceService
{
    Task<ApiResponse<AttendanceDto>> RecordAsync(CreateAttendanceDto dto);
    Task<ApiResponse<IEnumerable<AttendanceDto>>> GetByEnrollmentAsync(int enrollmentId);
}

public interface IGradeService
{
    Task<ApiResponse<GradeDto>> AddGradeAsync(CreateGradeDto dto);
    Task<ApiResponse<IEnumerable<GradeDto>>> GetByEnrollmentAsync(int enrollmentId);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly ICourseRepository _courseRepo;

    public EnrollmentService(IEnrollmentRepository enrollRepo, IStudentRepository studentRepo, ICourseRepository courseRepo)
    {
        _enrollRepo = enrollRepo;
        _studentRepo = studentRepo;
        _courseRepo = courseRepo;
    }

    public async Task<ApiResponse<EnrollmentDto>> EnrollAsync(CreateEnrollmentDto dto)
    {
        var student = await _studentRepo.GetByIdAsync(dto.StudentId);
        if (student is null) return ApiResponse<EnrollmentDto>.Fail("Student not found");

        var course = await _courseRepo.GetByIdAsync(dto.CourseId);
        if (course is null) return ApiResponse<EnrollmentDto>.Fail("Course not found");

        var existing = await _enrollRepo.GetByStudentAndCourseAsync(dto.StudentId, dto.CourseId);
        if (existing is not null && existing.Status == EnrollmentStatus.Active)
            return ApiResponse<EnrollmentDto>.Fail("Student already enrolled in this course");

        if (course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active) >= course.MaxStudents)
            return ApiResponse<EnrollmentDto>.Fail("Course is full");

        var enrollment = new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _enrollRepo.CreateAsync(enrollment);
        return ApiResponse<EnrollmentDto>.Ok(MapToDto(created, student, course), "Enrolled successfully");
    }

    public async Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetByStudentAsync(int studentId)
    {
        var enrollments = await _enrollRepo.GetByStudentAsync(studentId);
        return ApiResponse<IEnumerable<EnrollmentDto>>.Ok(enrollments.Select(e =>
            MapToDto(e, e.Student!, e.Course!)));
    }

    public async Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetByCourseAsync(int courseId)
    {
        var enrollments = await _enrollRepo.GetByCourseAsync(courseId);
        return ApiResponse<IEnumerable<EnrollmentDto>>.Ok(enrollments.Select(e =>
            MapToDto(e, e.Student!, e.Course!)));
    }

    public async Task<ApiResponse<bool>> WithdrawAsync(int enrollmentId)
    {
        var enrollment = await _enrollRepo.GetByIdAsync(enrollmentId);
        if (enrollment is null) return ApiResponse<bool>.Fail("Enrollment not found");
        if (enrollment.Status != EnrollmentStatus.Active)
            return ApiResponse<bool>.Fail("Enrollment is not active");

        enrollment.Status = EnrollmentStatus.Withdrawn;
        enrollment.WithdrawnAt = DateTime.UtcNow;
        await _enrollRepo.UpdateAsync(enrollment);
        return ApiResponse<bool>.Ok(true, "Withdrawn successfully");
    }

    private static EnrollmentDto MapToDto(Enrollment e, Student s, Course c) => new(
        e.Id, s.Id, $"{s.FirstName} {s.LastName}", c.Id, c.Name, c.Code, e.Status, e.EnrolledAt, e.WithdrawnAt);
}

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repo;
    private readonly IEnrollmentRepository _enrollRepo;

    public AttendanceService(IAttendanceRepository repo, IEnrollmentRepository enrollRepo)
    {
        _repo = repo;
        _enrollRepo = enrollRepo;
    }

    public async Task<ApiResponse<AttendanceDto>> RecordAsync(CreateAttendanceDto dto)
    {
        var enrollment = await _enrollRepo.GetByIdAsync(dto.EnrollmentId);
        if (enrollment is null) return ApiResponse<AttendanceDto>.Fail("Enrollment not found");

        var attendance = new Attendance
        {
            EnrollmentId = dto.EnrollmentId,
            Date = dto.Date.ToUniversalTime(),
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(attendance);
        var student = enrollment.Student!;
        var course = enrollment.Course!;

        return ApiResponse<AttendanceDto>.Ok(new AttendanceDto(
            created.Id, enrollment.Id, student.Id, $"{student.FirstName} {student.LastName}",
            course.Id, course.Name, created.Date, created.Status, created.Notes));
    }

    public async Task<ApiResponse<IEnumerable<AttendanceDto>>> GetByEnrollmentAsync(int enrollmentId)
    {
        var items = await _repo.GetByEnrollmentAsync(enrollmentId);
        var enrollment = await _enrollRepo.GetByIdAsync(enrollmentId);
        if (enrollment is null) return ApiResponse<IEnumerable<AttendanceDto>>.Fail("Enrollment not found");

        var student = enrollment.Student!;
        var course = enrollment.Course!;

        return ApiResponse<IEnumerable<AttendanceDto>>.Ok(items.Select(a => new AttendanceDto(
            a.Id, enrollment.Id, student.Id, $"{student.FirstName} {student.LastName}",
            course.Id, course.Name, a.Date, a.Status, a.Notes)));
    }
}

public class GradeService : IGradeService
{
    private readonly IGradeRepository _repo;
    private readonly IEnrollmentRepository _enrollRepo;

    public GradeService(IGradeRepository repo, IEnrollmentRepository enrollRepo)
    {
        _repo = repo;
        _enrollRepo = enrollRepo;
    }

    public async Task<ApiResponse<GradeDto>> AddGradeAsync(CreateGradeDto dto)
    {
        var enrollment = await _enrollRepo.GetByIdAsync(dto.EnrollmentId);
        if (enrollment is null) return ApiResponse<GradeDto>.Fail("Enrollment not found");

        var grade = new Grade
        {
            EnrollmentId = dto.EnrollmentId,
            Type = dto.Type,
            Value = dto.Value,
            Weight = dto.Weight,
            Description = dto.Description,
            EvaluatedAt = dto.EvaluatedAt.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(grade);
        var student = enrollment.Student!;
        var course = enrollment.Course!;

        return ApiResponse<GradeDto>.Ok(new GradeDto(
            created.Id, enrollment.Id, student.Id, $"{student.FirstName} {student.LastName}",
            course.Id, course.Name, created.Type, created.Value, created.Weight,
            created.Description, created.EvaluatedAt));
    }

    public async Task<ApiResponse<IEnumerable<GradeDto>>> GetByEnrollmentAsync(int enrollmentId)
    {
        var enrollment = await _enrollRepo.GetByIdAsync(enrollmentId);
        if (enrollment is null) return ApiResponse<IEnumerable<GradeDto>>.Fail("Enrollment not found");

        var grades = await _repo.GetByEnrollmentAsync(enrollmentId);
        var student = enrollment.Student!;
        var course = enrollment.Course!;

        return ApiResponse<IEnumerable<GradeDto>>.Ok(grades.Select(g => new GradeDto(
            g.Id, enrollment.Id, student.Id, $"{student.FirstName} {student.LastName}",
            course.Id, course.Name, g.Type, g.Value, g.Weight, g.Description, g.EvaluatedAt)));
    }
}
