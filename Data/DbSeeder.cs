using LMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string InstructorRole = "Instructor";
    public const string StudentRole = "Student";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { AdminRole, InstructorRole, StudentRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (config["SeedDemoData"] != "true")
            return;

        var admin = await CreateUserAsync(userManager, "admin@lms.com", "Admin User", "Admin@123", AdminRole);
        var instructor = await CreateUserAsync(userManager, "instructor@lms.com", "Sara Khan", "Instructor@123", InstructorRole);
        var student = await CreateUserAsync(userManager, "student@lms.com", "Ali Raza", "Student@123", StudentRole);

        if (await db.Courses.AnyAsync())
        {
            await DemoContent.AddMoreAsync(services);
            return;
        }
        var course1 = new Course
        {
            Title = "C# Fundamentals",
            Description = "Learn the basics of C#. Variables, loops, methods and classes.",
            Category = "Programming",
            Level = "Beginner",
            IsPublished = true,
            InstructorId = instructor.Id,
            Lessons = new List<Lesson>
            {
                new() { Title = "Introduction to C#", Content = "What C# is and how to write your first program.", Order = 1 },
                new() { Title = "Variables and Types", Content = "Learn int, string, bool and double.", Order = 2 },
                new() { Title = "Loops and Conditions", Content = "Use if, for and while.", Order = 3 }
            }
        };

        var course2 = new Course
        {
            Title = "ASP.NET Core MVC",
            Description = "Build web apps with controllers, views and Entity Framework.",
            Category = "Web Development",
            Level = "Intermediate",
            IsPublished = true,
            InstructorId = instructor.Id,
            Lessons = new List<Lesson>
            {
                new() { Title = "MVC Overview", Content = "How models, views and controllers work together.", Order = 1 },
                new() { Title = "Routing", Content = "How URLs map to controller actions.", Order = 2 },
                new() { Title = "Entity Framework Core", Content = "Save and read data with EF Core.", Order = 3 }
            }
        };

        var course3 = new Course
        {
            Title = "SQL Basics",
            Description = "Write queries to read and change data.",
            Category = "Database",
            Level = "Beginner",
            IsPublished = false,
            InstructorId = instructor.Id,
            Lessons = new List<Lesson>
            {
                new() { Title = "SELECT Queries", Content = "Read rows from a table.", Order = 1 }
            }
        };

        db.Courses.AddRange(course1, course2, course3);
        await db.SaveChangesAsync();

        var quiz = new Quiz
        {
            CourseId = course1.Id,
            Title = "C# Basics Quiz",
            PassMark = 50,
            Questions = new List<Question>
            {
                new()
                {
                    Text = "Which keyword declares a whole number variable?",
                    Options = new List<AnswerOption>
                    {
                        new() { Text = "int", IsCorrect = true },
                        new() { Text = "string", IsCorrect = false },
                        new() { Text = "bool", IsCorrect = false }
                    }
                },
                new()
                {
                    Text = "Which loop runs while a condition is true?",
                    Options = new List<AnswerOption>
                    {
                        new() { Text = "while", IsCorrect = true },
                        new() { Text = "switch", IsCorrect = false },
                        new() { Text = "using", IsCorrect = false }
                    }
                }
            }
        };

        var assignment = new Assignment
        {
            CourseId = course1.Id,
            Title = "Build a Calculator",
            Instructions = "Write a console calculator that adds, subtracts, multiplies and divides.",
            DueDate = DateTime.UtcNow.AddDays(7),
            MaxScore = 100
        };

        db.Quizzes.Add(quiz);
        db.Assignments.Add(assignment);
        db.Enrollments.Add(new Enrollment { CourseId = course1.Id, StudentId = student.Id });
        await db.SaveChangesAsync();
        await DemoContent.AddMoreAsync(services);
    }

     internal static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
            return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Could not create " + email + ": " + string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}