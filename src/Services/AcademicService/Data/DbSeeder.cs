using Microsoft.EntityFrameworkCore;
using Taller2NET.Shared.Models;

namespace AcademicService.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AcademicDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync()) return;

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@taller2.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var teacherUser = new User
        {
            Username = "teacher01",
            Email = "teacher01@taller2.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
            Role = "Teacher",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(adminUser, teacherUser);
        await context.SaveChangesAsync();

        var students = Enumerable.Range(1, 15).Select(i => new User
        {
            Username = $"student{i:D2}",
            Email = $"student{i:D2}@taller2.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            Role = "Student",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await context.Users.AddRangeAsync(students);
        await context.SaveChangesAsync();

        var studentEntities = Enumerable.Range(1, 15).Select(i => new Student
        {
            UserId = students[i - 1].Id,
            FirstName = GetFirstName(i),
            LastName = GetLastName(i),
            StudentCode = $"EST{2024000 + i}",
            Email = $"student{i:D2}@taller2.edu",
            Phone = $"+57310{1000000 + i}",
            DateOfBirth = new DateTime(2000 + (i % 5), (i % 12) + 1, (i % 28) + 1),
            Program = GetProgram(i),
            Semester = (i % 8) + 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await context.Students.AddRangeAsync(studentEntities);
        await context.SaveChangesAsync();

        var courses = new List<Course>
        {
            new() { Code = "CS101", Name = "Introducción a la Programación", Description = "Fundamentos de programación con Python", Credits = 3, Teacher = "Dr. García", Semester = 1, MaxStudents = 30, Schedule = "Lun/Mié 08:00-10:00", IsActive = true },
            new() { Code = "CS201", Name = "Estructuras de Datos", Description = "Árboles, grafos y algoritmos", Credits = 4, Teacher = "Dra. López", Semester = 2, MaxStudents = 25, Schedule = "Mar/Jue 10:00-12:00", IsActive = true },
            new() { Code = "CS301", Name = "Base de Datos", Description = "Diseño y gestión de bases de datos relacionales", Credits = 3, Teacher = "Dr. Martínez", Semester = 3, MaxStudents = 28, Schedule = "Lun/Mié 14:00-16:00", IsActive = true },
            new() { Code = "CS401", Name = "Redes de Computadores", Description = "Protocolos y arquitectura de redes", Credits = 3, Teacher = "Dra. Rodríguez", Semester = 4, MaxStudents = 25, Schedule = "Mar/Jue 14:00-16:00", IsActive = true },
            new() { Code = "CS501", Name = "Ingeniería de Software", Description = "Metodologías ágiles y desarrollo de software", Credits = 4, Teacher = "Dr. Hernández", Semester = 5, MaxStudents = 30, Schedule = "Vie 08:00-12:00", IsActive = true },
            new() { Code = "MAT101", Name = "Cálculo Diferencial", Description = "Límites, derivadas e integrales", Credits = 4, Teacher = "Dr. Sánchez", Semester = 1, MaxStudents = 35, Schedule = "Lun/Mié/Vie 07:00-08:00", IsActive = true },
            new() { Code = "MAT201", Name = "Álgebra Lineal", Description = "Matrices, vectores y transformaciones lineales", Credits = 3, Teacher = "Dra. Gómez", Semester = 2, MaxStudents = 30, Schedule = "Mar/Jue 07:00-09:00", IsActive = true },
        };

        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();

        var enrollments = new List<Enrollment>();
        var random = new Random(42);

        foreach (var student in studentEntities)
        {
            var courseSample = courses.OrderBy(_ => random.Next()).Take(random.Next(2, 5)).ToList();
            foreach (var course in courseSample)
            {
                enrollments.Add(new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.Id,
                    Status = EnrollmentStatus.Active,
                    EnrolledAt = DateTime.UtcNow.AddDays(-random.Next(10, 90)),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.Enrollments.AddRangeAsync(enrollments);
        await context.SaveChangesAsync();

        var attendances = new List<Attendance>();
        var grades = new List<Grade>();

        foreach (var enrollment in enrollments)
        {
            for (int day = 1; day <= 10; day++)
            {
                var statusRoll = random.Next(100);
                var status = statusRoll < 80 ? AttendanceStatus.Present
                           : statusRoll < 90 ? AttendanceStatus.Late
                           : statusRoll < 95 ? AttendanceStatus.Excused
                           : AttendanceStatus.Absent;

                attendances.Add(new Attendance
                {
                    EnrollmentId = enrollment.Id,
                    Date = DateTime.UtcNow.AddDays(-day * 7),
                    Status = status,
                    CreatedAt = DateTime.UtcNow
                });
            }

            grades.Add(new Grade { EnrollmentId = enrollment.Id, Type = GradeType.Quiz, Value = (decimal)(random.NextDouble() * 4 + 1), Weight = 10, EvaluatedAt = DateTime.UtcNow.AddDays(-60), CreatedAt = DateTime.UtcNow });
            grades.Add(new Grade { EnrollmentId = enrollment.Id, Type = GradeType.Assignment, Value = (decimal)(random.NextDouble() * 4 + 1), Weight = 20, EvaluatedAt = DateTime.UtcNow.AddDays(-45), CreatedAt = DateTime.UtcNow });
            grades.Add(new Grade { EnrollmentId = enrollment.Id, Type = GradeType.Midterm, Value = (decimal)(random.NextDouble() * 4 + 1), Weight = 30, EvaluatedAt = DateTime.UtcNow.AddDays(-30), CreatedAt = DateTime.UtcNow });
            grades.Add(new Grade { EnrollmentId = enrollment.Id, Type = GradeType.Project, Value = (decimal)(random.NextDouble() * 4 + 1), Weight = 20, EvaluatedAt = DateTime.UtcNow.AddDays(-15), CreatedAt = DateTime.UtcNow });
            grades.Add(new Grade { EnrollmentId = enrollment.Id, Type = GradeType.Final, Value = (decimal)(random.NextDouble() * 4 + 1), Weight = 20, EvaluatedAt = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow });
        }

        await context.Attendances.AddRangeAsync(attendances);
        await context.Grades.AddRangeAsync(grades);
        await context.SaveChangesAsync();
    }

    private static string GetFirstName(int i) => i switch
    {
        1 => "Carlos", 2 => "María", 3 => "Juan", 4 => "Laura", 5 => "Andrés",
        6 => "Valentina", 7 => "Sebastián", 8 => "Camila", 9 => "Felipe", 10 => "Isabella",
        11 => "Daniel", 12 => "Sofía", 13 => "Mateo", 14 => "Mariana", 15 => "Santiago",
        _ => $"Estudiante{i}"
    };

    private static string GetLastName(int i) => i switch
    {
        1 => "García", 2 => "Rodríguez", 3 => "Martínez", 4 => "López", 5 => "González",
        6 => "Pérez", 7 => "Sánchez", 8 => "Ramírez", 9 => "Torres", 10 => "Flores",
        11 => "Rivera", 12 => "Gómez", 13 => "Díaz", 14 => "Reyes", 15 => "Cruz",
        _ => $"Apellido{i}"
    };

    private static string GetProgram(int i) => (i % 4) switch
    {
        0 => "Ingeniería de Sistemas",
        1 => "Ingeniería Informática",
        2 => "Ciencias de la Computación",
        _ => "Ingeniería de Software"
    };
}
