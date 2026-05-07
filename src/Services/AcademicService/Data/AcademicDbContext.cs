using Microsoft.EntityFrameworkCore;
using Taller2NET.Shared.Models;

namespace AcademicService.Data;

public class AcademicDbContext : DbContext
{
    public AcademicDbContext(DbContextOptions<AcademicDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.Role).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.StudentCode).IsUnique();
            e.HasIndex(s => s.Email).IsUnique();
            e.Property(s => s.StudentCode).HasMaxLength(20).IsRequired();
            e.Property(s => s.FirstName).HasMaxLength(100).IsRequired();
            e.Property(s => s.LastName).HasMaxLength(100).IsRequired();
            e.Property(s => s.Email).HasMaxLength(200).IsRequired();
            e.Property(s => s.Program).HasMaxLength(200).IsRequired();
            e.HasOne(s => s.User)
             .WithOne(u => u.Student)
             .HasForeignKey<Student>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Code).HasMaxLength(20).IsRequired();
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Teacher).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.HasKey(en => en.Id);
            e.HasIndex(en => new { en.StudentId, en.CourseId }).IsUnique();
            e.Property(en => en.Status).HasConversion<string>();
            e.HasOne(en => en.Student)
             .WithMany(s => s.Enrollments)
             .HasForeignKey(en => en.StudentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(en => en.Course)
             .WithMany(c => c.Enrollments)
             .HasForeignKey(en => en.CourseId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Attendance>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => new { a.EnrollmentId, a.Date });
            e.Property(a => a.Status).HasConversion<string>();
            e.HasOne(a => a.Enrollment)
             .WithMany(en => en.Attendances)
             .HasForeignKey(a => a.EnrollmentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Grade>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Value).HasPrecision(5, 2);
            e.Property(g => g.Weight).HasPrecision(5, 2);
            e.Property(g => g.Type).HasConversion<string>();
            e.HasOne(g => g.Enrollment)
             .WithMany(en => en.Grades)
             .HasForeignKey(g => g.EnrollmentId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
