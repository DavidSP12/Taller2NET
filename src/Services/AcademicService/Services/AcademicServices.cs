using Taller2NET.Shared.DTOs;
using Taller2NET.Shared.Models;
using Taller2NET.Shared.Responses;
using AcademicService.Repositories;

namespace AcademicService.Services;

public interface IEnrollmentService
{
    Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetAllAsync();
    Task<ApiResponse<EnrollmentDto>> GetByIdAsync(int enrollmentId);
    Task<ApiResponse<EnrollmentDto>> EnrollAsync(CreateEnrollmentDto dto);
    Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetByStudentAsync(int studentId);
    Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetByCourseAsync(int courseId);
    Task<ApiResponse<EnrollmentDto>> UpdateAsync(int enrollmentId, UpdateEnrollmentDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int enrollmentId);
    Task<ApiResponse<bool>> WithdrawAsync(int enrollmentId);
}

public interface IAttendanceService
{
    Task<ApiResponse<IEnumerable<AttendanceDto>>> GetAllAsync();
    Task<ApiResponse<AttendanceDto>> GetByIdAsync(int attendanceId);
    Task<ApiResponse<AttendanceDto>> RecordAsync(CreateAttendanceDto dto);
    Task<ApiResponse<IEnumerable<AttendanceDto>>> GetByEnrollmentAsync(int enrollmentId);
    Task<ApiResponse<AttendanceDto>> UpdateAsync(int attendanceId, UpdateAttendanceDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int attendanceId);
}

public interface IGradeService
{
    Task<ApiResponse<IEnumerable<GradeDto>>> GetAllAsync();
    Task<ApiResponse<GradeDto>> GetByIdAsync(int gradeId);
    Task<ApiResponse<GradeDto>> AddGradeAsync(CreateGradeDto dto);
    Task<ApiResponse<IEnumerable<GradeDto>>> GetByEnrollmentAsync(int enrollmentId);
    Task<ApiResponse<GradeDto>> UpdateAsync(int gradeId, UpdateGradeDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int gradeId);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentCrudRepository _enrollRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly ICourseRepository _courseRepo;

    public EnrollmentService(IEnrollmentCrudRepository enrollRepo, IStudentRepository studentRepo, ICourseRepository courseRepo)
    {
        _enrollRepo = enrollRepo;
        _studentRepo = studentRepo;
        _courseRepo = courseRepo;
    }

    public async Task<ApiResponse<IEnumerable<EnrollmentDto>>> GetAllAsync()
    {
        var enrollments = await _enrollRepo.GetAllAsync();
        return ApiResponse<IEnumerable<EnrollmentDto>>.Ok(enrollments.Select(e =>
            MapToDto(e, e.Student!, e.Course!)));
    }

    public async Task<ApiResponse<EnrollmentDto>> GetByIdAsync(int enrollmentId)
    {
        var enrollment = await _enrollRepo.GetByIdAsync(enrollmentId);
        if (enrollment is null) return ApiResponse<EnrollmentDto>.Fail("Enrollment not found");
        return ApiResponse<EnrollmentDto>.Ok(MapToDto(enrollment, enrollment.Student!, enrollment.Course!));
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

    public async Task<ApiResponse<EnrollmentDto>> UpdateAsync(int enrollmentId, UpdateEnrollmentDto dto)
    {
        var enrollment = await _enrollRepo.GetByIdAsync(enrollmentId);
        if (enrollment is null) return ApiResponse<EnrollmentDto>.Fail("Enrollment not found");

        enrollment.Status = dto.Status;
        enrollment.WithdrawnAt = dto.WithdrawnAt?.ToUniversalTime();
        await _enrollRepo.UpdateAsync(enrollment);

        var updated = await _enrollRepo.GetByIdAsync(enrollmentId);
        if (updated is null) return ApiResponse<EnrollmentDto>.Fail("Enrollment not found");
        return ApiResponse<EnrollmentDto>.Ok(MapToDto(updated, updated.Student!, updated.Course!), "Enrollment updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int enrollmentId)
    {
        var deleted = await _enrollRepo.DeleteAsync(enrollmentId);
        return deleted
            ? ApiResponse<bool>.Ok(true, "Enrollment deleted successfully")
            : ApiResponse<bool>.Fail("Enrollment not found");
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
    private readonly IAttendanceCrudRepository _repo;
    private readonly IEnrollmentCrudRepository _enrollRepo;

    public AttendanceService(IAttendanceCrudRepository repo, IEnrollmentCrudRepository enrollRepo)
    {
        _repo = repo;
        _enrollRepo = enrollRepo;
    }

    public async Task<ApiResponse<IEnumerable<AttendanceDto>>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return ApiResponse<IEnumerable<AttendanceDto>>.Ok(items
            .Where(item => item.Enrollment?.Student is not null && item.Enrollment.Course is not null)
            .Select(item => new AttendanceDto(
                item.Id, item.Enrollment!.Id, item.Enrollment.Student!.Id, $"{item.Enrollment.Student.FirstName} {item.Enrollment.Student.LastName}",
                item.Enrollment.Course!.Id, item.Enrollment.Course.Name, item.Date, item.Status, item.Notes)));
    }

    public async Task<ApiResponse<AttendanceDto>> GetByIdAsync(int attendanceId)
    {
        var attendance = await _repo.GetByIdAsync(attendanceId);
        if (attendance is null) return ApiResponse<AttendanceDto>.Fail("Attendance not found");
        if (attendance.Enrollment?.Student is null || attendance.Enrollment.Course is null) return ApiResponse<AttendanceDto>.Fail("Enrollment not found");

        return ApiResponse<AttendanceDto>.Ok(new AttendanceDto(
            attendance.Id, attendance.Enrollment.Id, attendance.Enrollment.Student.Id, $"{attendance.Enrollment.Student.FirstName} {attendance.Enrollment.Student.LastName}",
            attendance.Enrollment.Course.Id, attendance.Enrollment.Course.Name, attendance.Date, attendance.Status, attendance.Notes));
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

    public async Task<ApiResponse<AttendanceDto>> UpdateAsync(int attendanceId, UpdateAttendanceDto dto)
    {
        var attendance = await _repo.GetByIdAsync(attendanceId);
        if (attendance is null) return ApiResponse<AttendanceDto>.Fail("Attendance not found");

        attendance.Date = dto.Date.ToUniversalTime();
        attendance.Status = dto.Status;
        attendance.Notes = dto.Notes;
        await _repo.UpdateAsync(attendance);

        var enrollment = await _enrollRepo.GetByIdAsync(attendance.EnrollmentId);
        if (enrollment?.Student is null || enrollment.Course is null) return ApiResponse<AttendanceDto>.Fail("Enrollment not found");

        return ApiResponse<AttendanceDto>.Ok(new AttendanceDto(
            attendance.Id, enrollment.Id, enrollment.Student.Id, $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
            enrollment.Course.Id, enrollment.Course.Name, attendance.Date, attendance.Status, attendance.Notes), "Attendance updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int attendanceId)
    {
        var deleted = await _repo.DeleteAsync(attendanceId);
        return deleted
            ? ApiResponse<bool>.Ok(true, "Attendance deleted successfully")
            : ApiResponse<bool>.Fail("Attendance not found");
    }
}

public class GradeService : IGradeService
{
    private readonly IGradeCrudRepository _repo;
    private readonly IEnrollmentCrudRepository _enrollRepo;

    public GradeService(IGradeCrudRepository repo, IEnrollmentCrudRepository enrollRepo)
    {
        _repo = repo;
        _enrollRepo = enrollRepo;
    }

    public async Task<ApiResponse<IEnumerable<GradeDto>>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return ApiResponse<IEnumerable<GradeDto>>.Ok(items
            .Where(item => item.Enrollment?.Student is not null && item.Enrollment.Course is not null)
            .Select(item => new GradeDto(
                item.Id, item.Enrollment!.Id, item.Enrollment.Student!.Id, $"{item.Enrollment.Student.FirstName} {item.Enrollment.Student.LastName}",
                item.Enrollment.Course!.Id, item.Enrollment.Course.Name, item.Type, item.Value, item.Weight, item.Description, item.EvaluatedAt)));
    }

    public async Task<ApiResponse<GradeDto>> GetByIdAsync(int gradeId)
    {
        var grade = await _repo.GetByIdAsync(gradeId);
        if (grade is null) return ApiResponse<GradeDto>.Fail("Grade not found");
        if (grade.Enrollment?.Student is null || grade.Enrollment.Course is null) return ApiResponse<GradeDto>.Fail("Enrollment not found");

        return ApiResponse<GradeDto>.Ok(new GradeDto(
            grade.Id, grade.Enrollment.Id, grade.Enrollment.Student.Id, $"{grade.Enrollment.Student.FirstName} {grade.Enrollment.Student.LastName}",
            grade.Enrollment.Course.Id, grade.Enrollment.Course.Name, grade.Type, grade.Value, grade.Weight, grade.Description, grade.EvaluatedAt));
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

    public async Task<ApiResponse<GradeDto>> UpdateAsync(int gradeId, UpdateGradeDto dto)
    {
        var grade = await _repo.GetByIdAsync(gradeId);
        if (grade is null) return ApiResponse<GradeDto>.Fail("Grade not found");

        grade.Type = dto.Type;
        grade.Value = dto.Value;
        grade.Weight = dto.Weight;
        grade.Description = dto.Description;
        grade.EvaluatedAt = dto.EvaluatedAt.ToUniversalTime();
        await _repo.UpdateAsync(grade);

        var enrollment = await _enrollRepo.GetByIdAsync(grade.EnrollmentId);
        if (enrollment?.Student is null || enrollment.Course is null) return ApiResponse<GradeDto>.Fail("Enrollment not found");

        return ApiResponse<GradeDto>.Ok(new GradeDto(
            grade.Id, enrollment.Id, enrollment.Student.Id, $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
            enrollment.Course.Id, enrollment.Course.Name, grade.Type, grade.Value, grade.Weight, grade.Description, grade.EvaluatedAt), "Grade updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int gradeId)
    {
        var deleted = await _repo.DeleteAsync(gradeId);
        return deleted
            ? ApiResponse<bool>.Ok(true, "Grade deleted successfully")
            : ApiResponse<bool>.Fail("Grade not found");
    }
}
