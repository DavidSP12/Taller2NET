namespace Taller2NET.Shared.Models;

public class Attendance
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Enrollment? Enrollment { get; set; }
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    Excused = 4
}
