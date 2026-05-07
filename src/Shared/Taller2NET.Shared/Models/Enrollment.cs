namespace Taller2NET.Shared.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? WithdrawnAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }
    public Course? Course { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}

public enum EnrollmentStatus
{
    Active = 1,
    Withdrawn = 2,
    Completed = 3,
    Failed = 4
}
