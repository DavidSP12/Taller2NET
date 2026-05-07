using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller2NET.Shared.DTOs;
using AcademicService.Services;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service) => _service = service;

    /// <summary>Get paginated list of students</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    /// <summary>Get student by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new student</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>Update student information</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Deactivate a student</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
