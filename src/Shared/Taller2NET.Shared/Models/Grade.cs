namespace Taller2NET.Shared.Models;

public class Grade
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public GradeType Type { get; set; }
    public decimal Value { get; set; }
    public decimal Weight { get; set; }
    public string? Description { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Enrollment? Enrollment { get; set; }
}

public enum GradeType
{
    Quiz = 1,
    Midterm = 2,
    Final = 3,
    Project = 4,
    Assignment = 5
}
