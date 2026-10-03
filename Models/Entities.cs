using Microsoft.AspNetCore.Identity;

namespace LMS.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = "Beginner";
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string InstructorId { get; set; } = string.Empty;
    public ApplicationUser? Instructor { get; set; }

    public List<Lesson> Lessons { get; set; } = new();
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<Quiz> Quizzes { get; set; } = new();
    public List<Assignment> Assignments { get; set; } = new();
}

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int Order { get; set; }
}

public class Enrollment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public class LessonProgress
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class Quiz
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public string Title { get; set; } = string.Empty;
    public int PassMark { get; set; } = 50;

    public List<Question> Questions { get; set; } = new();
    public List<QuizAttempt> Attempts { get; set; } = new();
}

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public string Text { get; set; } = string.Empty;
    public List<AnswerOption> Options { get; set; } = new();
}

public class AnswerOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }

    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class QuizAttempt
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public int Score { get; set; }
    public int Total { get; set; }
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;
}

public class Assignment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public int MaxScore { get; set; } = 100;

    public List<Submission> Submissions { get; set; } = new();
}

public class Submission
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public int? Score { get; set; }
    public string? Feedback { get; set; }
}