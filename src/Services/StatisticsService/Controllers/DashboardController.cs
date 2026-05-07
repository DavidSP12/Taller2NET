using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatisticsService.Cache;
using StatisticsService.Services;

namespace StatisticsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IStatisticsService _stats;
    private readonly ICacheService _cache;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IStatisticsService stats, ICacheService cache, ILogger<DashboardController> logger)
    {
        _stats = stats;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>Get overall dashboard summary with key metrics</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        const string cacheKey = "stats:dashboard:summary";
        var cached = await _cache.GetAsync<DashboardSummaryDto>(cacheKey);
        if (cached is not null)
        {
            _logger.LogInformation("Dashboard summary served from cache");
            return Ok(new { Success = true, Data = cached, Source = "cache" });
        }

        var data = await _stats.GetDashboardSummaryAsync();
        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(2));
        return Ok(new { Success = true, Data = data, Source = "database" });
    }

    /// <summary>Get statistics per course</summary>
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourseStats()
    {
        const string cacheKey = "stats:courses";
        var cached = await _cache.GetAsync<IEnumerable<CourseStatsDto>>(cacheKey);
        if (cached is not null)
            return Ok(new { Success = true, Data = cached, Source = "cache" });

        var data = await _stats.GetCourseStatsAsync();
        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
        return Ok(new { Success = true, Data = data, Source = "database" });
    }

    /// <summary>Get statistics per student (paginated)</summary>
    [HttpGet("students")]
    public async Task<IActionResult> GetStudentStats([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var data = await _stats.GetStudentStatsAsync(page, pageSize);
        return Ok(new { Success = true, Data = data });
    }

    /// <summary>Get detailed statistics for a specific student</summary>
    [HttpGet("students/{studentId:int}")]
    public async Task<IActionResult> GetStudentStats(int studentId)
    {
        var cacheKey = $"stats:student:{studentId}";
        var cached = await _cache.GetAsync<StudentStatsDto>(cacheKey);
        if (cached is not null)
            return Ok(new { Success = true, Data = cached, Source = "cache" });

        var data = await _stats.GetStudentStatsByIdAsync(studentId);
        if (data is null) return NotFound(new { Success = false, Message = "Student not found" });

        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
        return Ok(new { Success = true, Data = data, Source = "database" });
    }

    /// <summary>Get top N courses by enrollment</summary>
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses([FromQuery] int top = 5)
    {
        var cacheKey = $"stats:top-courses:{top}";
        var cached = await _cache.GetAsync<IEnumerable<TopCourseDto>>(cacheKey);
        if (cached is not null)
            return Ok(new { Success = true, Data = cached, Source = "cache" });

        var data = await _stats.GetTopCoursesAsync(top);
        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
        return Ok(new { Success = true, Data = data, Source = "database" });
    }

    /// <summary>Get attendance summary (optionally filtered by course)</summary>
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance([FromQuery] int? courseId = null)
    {
        var cacheKey = courseId.HasValue ? $"stats:attendance:course:{courseId}" : "stats:attendance:global";
        var cached = await _cache.GetAsync<AttendanceSummaryDto>(cacheKey);
        if (cached is not null)
            return Ok(new { Success = true, Data = cached, Source = "cache" });

        var data = await _stats.GetAttendanceSummaryAsync(courseId);
        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
        return Ok(new { Success = true, Data = data, Source = "database" });
    }

    /// <summary>Get grade summary (optionally filtered by course)</summary>
    [HttpGet("grades")]
    public async Task<IActionResult> GetGrades([FromQuery] int? courseId = null)
    {
        var cacheKey = courseId.HasValue ? $"stats:grades:course:{courseId}" : "stats:grades:global";
        var cached = await _cache.GetAsync<GradeSummaryDto>(cacheKey);
        if (cached is not null)
            return Ok(new { Success = true, Data = cached, Source = "cache" });

        var data = await _stats.GetGradeSummaryAsync(courseId);
        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
        return Ok(new { Success = true, Data = data, Source = "database" });
    }

    /// <summary>Get recent platform activity</summary>
    [HttpGet("activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int count = 10)
    {
        var data = await _stats.GetRecentActivityAsync(count);
        return Ok(new { Success = true, Data = data });
    }

    /// <summary>Get statistics grouped by academic program</summary>
    [HttpGet("programs")]
    public async Task<IActionResult> GetProgramStats()
    {
        const string cacheKey = "stats:programs";
        var cached = await _cache.GetAsync<IEnumerable<ProgramStatsDto>>(cacheKey);
        if (cached is not null)
            return Ok(new { Success = true, Data = cached, Source = "cache" });

        var data = await _stats.GetProgramStatsAsync();
        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
        return Ok(new { Success = true, Data = data, Source = "database" });
    }
}
