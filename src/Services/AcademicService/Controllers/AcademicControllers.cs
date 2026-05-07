using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller2NET.Shared.DTOs;
using AcademicService.Services;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>Enroll a student in a course</summary>
    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto dto)
    {
        var result = await _service.EnrollAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetByStudent), new { studentId = dto.StudentId }, result) : BadRequest(result);
    }

    /// <summary>Get enrollments for a student</summary>
    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> GetByStudent(int studentId)
    {
        var result = await _service.GetByStudentAsync(studentId);
        return Ok(result);
    }

    /// <summary>Get enrollments for a course</summary>
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetByCourse(int courseId)
    {
        var result = await _service.GetByCourseAsync(courseId);
        return Ok(result);
    }

    /// <summary>Withdraw from a course</summary>
    [HttpPut("{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int id)
    {
        var result = await _service.WithdrawAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendancesController : ControllerBase
{
    private readonly IAttendanceService _service;

    public AttendancesController(IAttendanceService service) => _service = service;

    /// <summary>Record attendance for an enrollment</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Record([FromBody] CreateAttendanceDto dto)
    {
        var result = await _service.RecordAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetByEnrollment), new { enrollmentId = dto.EnrollmentId }, result) : BadRequest(result);
    }

    /// <summary>Get attendance records for an enrollment</summary>
    [HttpGet("enrollment/{enrollmentId:int}")]
    public async Task<IActionResult> GetByEnrollment(int enrollmentId)
    {
        var result = await _service.GetByEnrollmentAsync(enrollmentId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GradesController : ControllerBase
{
    private readonly IGradeService _service;

    public GradesController(IGradeService service) => _service = service;

    /// <summary>Add a grade for an enrollment</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> AddGrade([FromBody] CreateGradeDto dto)
    {
        var result = await _service.AddGradeAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetByEnrollment), new { enrollmentId = dto.EnrollmentId }, result) : BadRequest(result);
    }

    /// <summary>Get grades for an enrollment</summary>
    [HttpGet("enrollment/{enrollmentId:int}")]
    public async Task<IActionResult> GetByEnrollment(int enrollmentId)
    {
        var result = await _service.GetByEnrollmentAsync(enrollmentId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
