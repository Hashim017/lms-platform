using System.ComponentModel.DataAnnotations;
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

public class CourseFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Level { get; set; } = "Beginner";

    [Display(Name = "Published")]
    public bool IsPublished { get; set; }
}

public class LessonFormViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Url, Display(Name = "Video URL")]
    public string? VideoUrl { get; set; }

    [Range(1, 1000)]
    public int Order { get; set; } = 1;
}

public class HomeViewModel
{
    public int CourseCount { get; set; }
    public int LessonCount { get; set; }
    public int StudentCount { get; set; }
    public int InstructorCount { get; set; }
    public List<Course> Featured { get; set; } = new();
}

public class MyCourseItem
{
    public Course Course { get; set; } = null!;
    public int TotalLessons { get; set; }
    public int DoneLessons { get; set; }
    public bool IsCompleted { get; set; }
    public int? ResumeLessonId { get; set; }
    public int Percent => TotalLessons == 0 ? 0 : DoneLessons * 100 / TotalLessons;
}

public class LessonPageViewModel
{
    public Course Course { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public List<Lesson> Lessons { get; set; } = new();
    public HashSet<int> DoneIds { get; set; } = new();
    public bool IsEnrolled { get; set; }
    public Lesson? Prev { get; set; }
    public Lesson? Next { get; set; }
    public int Percent => Lessons.Count == 0 ? 0 : DoneIds.Count * 100 / Lessons.Count;
}

public class QuizFormViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 100), Display(Name = "Pass mark in percent")]
    public int PassMark { get; set; } = 50;
}

public class QuestionFormViewModel
{
    public int QuizId { get; set; }

    [Required, StringLength(500)]
    public string Text { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Option1 { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Option2 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Option3 { get; set; }

    [StringLength(200)]
    public string? Option4 { get; set; }

    [Range(1, 4)]
    public int CorrectIndex { get; set; } = 1;
}

public class QuizEditViewModel
{
    public Quiz Quiz { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public List<QuizAttempt> Attempts { get; set; } = new();
}

public class QuizTakeViewModel
{
    public Quiz Quiz { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public List<QuizAttempt> Attempts { get; set; } = new();
    public int BestPercent => Attempts.Count == 0
        ? 0
        : Attempts.Max(a => a.Total == 0 ? 0 : a.Score * 100 / a.Total);
}

public class ReviewItem
{
    public string Question { get; set; } = string.Empty;
    public List<AnswerOption> Options { get; set; } = new();
    public int SelectedId { get; set; }
    public int CorrectId { get; set; }
}

public class QuizResultViewModel
{
    public Quiz Quiz { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public int Score { get; set; }
    public int Total { get; set; }
    public bool Passed { get; set; }
    public List<ReviewItem> Items { get; set; } = new();
    public int Percent => Total == 0 ? 0 : Score * 100 / Total;
}

public class AssignmentFormViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Instructions { get; set; } = string.Empty;

    [Display(Name = "Due date")]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);

    [Range(1, 1000), Display(Name = "Max score")]
    public int MaxScore { get; set; } = 100;
}

public class AssignmentPageViewModel
{
    public Assignment Assignment { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Submission? Mine { get; set; }
    public List<Submission> All { get; set; } = new();
    public bool CanManage { get; set; }
    public bool IsEnrolled { get; set; }
}

public class DayCount
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopCourse
{
    public string Title { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public int Learners { get; set; }
}

public class AdminDashboardViewModel
{
    public int Users { get; set; }
    public int Students { get; set; }
    public int Instructors { get; set; }
    public int Courses { get; set; }
    public int Published { get; set; }
    public int Enrollments { get; set; }
    public int Completed { get; set; }
    public int QuizAttempts { get; set; }
    public int Submissions { get; set; }
    public List<DayCount> PerDay { get; set; } = new();
    public List<TopCourse> Top { get; set; } = new();
    public int CompletionRate => Enrollments == 0 ? 0 : Completed * 100 / Enrollments;
}

public class UserRow
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
    public DateTime CreatedAt { get; set; }
    public int Enrolled { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(80), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}