using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[Authorize]
public class QuizzesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuizzesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private bool CanManage(Course course)
    {
        if (User.IsInRole("Admin")) return true;
        return User.IsInRole("Instructor") && course.InstructorId == _userManager.GetUserId(User);
    }

    public async Task<IActionResult> Take(int id)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions).ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz?.Course == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var canManage = CanManage(quiz.Course);
        var enrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == quiz.CourseId && e.StudentId == userId);

        if (!quiz.Course.IsPublished && !canManage) return NotFound();

        if (!enrolled && !canManage)
        {
            TempData["Error"] = "Enroll in this course to take its quizzes.";
            return RedirectToAction("Details", "Courses", new { id = quiz.CourseId });
        }

        quiz.Questions = quiz.Questions.OrderBy(q => q.Id).ToList();

        var attempts = await _db.QuizAttempts
            .Where(a => a.QuizId == id && a.StudentId == userId)
            .OrderByDescending(a => a.TakenAt)
            .ToListAsync();

        ViewBag.CanManage = canManage;
        ViewBag.CanSubmit = enrolled;

        return View(new QuizTakeViewModel { Quiz = quiz, Course = quiz.Course, Attempts = attempts });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, Dictionary<int, int> answers)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions).ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz?.Course == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var enrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == quiz.CourseId && e.StudentId == userId);
        if (!enrolled) return Forbid();

        var questions = quiz.Questions.OrderBy(q => q.Id).ToList();
        if (questions.Count == 0) return RedirectToAction(nameof(Take), new { id });

        var items = new List<ReviewItem>();
        var score = 0;

        foreach (var q in questions)
        {
            answers.TryGetValue(q.Id, out var selected);
            var correct = q.Options.FirstOrDefault(o => o.IsCorrect);
            if (correct != null && selected == correct.Id) score++;

            items.Add(new ReviewItem
            {
                Question = q.Text,
                Options = q.Options.OrderBy(o => o.Id).ToList(),
                SelectedId = selected,
                CorrectId = correct?.Id ?? 0
            });
        }

        _db.QuizAttempts.Add(new QuizAttempt
        {
            QuizId = id,
            StudentId = userId,
            Score = score,
            Total = questions.Count
        });
        await _db.SaveChangesAsync();

        var percent = score * 100 / questions.Count;

        return View("Result", new QuizResultViewModel
        {
            Quiz = quiz,
            Course = quiz.Course,
            Score = score,
            Total = questions.Count,
            Passed = percent >= quiz.PassMark,
            Items = items
        });
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        return View(new QuizFormViewModel { CourseId = courseId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(QuizFormViewModel model)
    {
        var course = await _db.Courses.FindAsync(model.CourseId);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        if (!ModelState.IsValid) return View(model);

        var quiz = new Quiz
        {
            CourseId = model.CourseId,
            Title = model.Title,
            PassMark = model.PassMark
        };
        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Quiz created. Now add your questions.";
        return RedirectToAction(nameof(Edit), new { id = quiz.Id });
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions).ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz?.Course == null) return NotFound();
        if (!CanManage(quiz.Course)) return Forbid();

        var attempts = await _db.QuizAttempts
            .Include(a => a.Student)
            .Where(a => a.QuizId == id)
            .OrderByDescending(a => a.TakenAt)
            .Take(50)
            .ToListAsync();

        return View(new QuizEditViewModel { Quiz = quiz, Course = quiz.Course, Attempts = attempts });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Update(QuizFormViewModel model)
    {
        var quiz = await _db.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.Id == model.Id);
        if (quiz?.Course == null) return NotFound();
        if (!CanManage(quiz.Course)) return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Give the quiz a title and a pass mark between 1 and 100.";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        quiz.Title = model.Title;
        quiz.PassMark = model.PassMark;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Quiz saved.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> AddQuestion(QuestionFormViewModel model)
    {
        var quiz = await _db.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.Id == model.QuizId);
        if (quiz?.Course == null) return NotFound();
        if (!CanManage(quiz.Course)) return Forbid();

        var texts = new string?[] { model.Option1, model.Option2, model.Option3, model.Option4 };

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(texts[model.CorrectIndex - 1]))
        {
            TempData["Error"] = "Write the question, fill at least 2 options and pick a correct option that has text.";
            return RedirectToAction(nameof(Edit), new { id = model.QuizId });
        }

        var question = new Question { QuizId = quiz.Id, Text = model.Text.Trim() };
        for (var i = 0; i < 4; i++)
        {
            if (string.IsNullOrWhiteSpace(texts[i])) continue;
            question.Options.Add(new AnswerOption
            {
                Text = texts[i]!.Trim(),
                IsCorrect = i + 1 == model.CorrectIndex
            });
        }

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Question added.";
        return RedirectToAction(nameof(Edit), new { id = model.QuizId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _db.Questions
            .Include(q => q.Quiz).ThenInclude(z => z!.Course)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question?.Quiz?.Course == null) return NotFound();
        if (!CanManage(question.Quiz.Course)) return Forbid();

        var quizId = question.QuizId;
        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Question deleted.";
        return RedirectToAction(nameof(Edit), new { id = quizId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _db.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.Id == id);
        if (quiz?.Course == null) return NotFound();
        if (!CanManage(quiz.Course)) return Forbid();

        var courseId = quiz.CourseId;
        _db.Quizzes.Remove(quiz);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Quiz deleted.";
        return RedirectToAction("Details", "Courses", new { id = courseId });
    }
}