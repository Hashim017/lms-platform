using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[Authorize]
public class AssignmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignmentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private bool CanManage(Course course)
    {
        if (User.IsInRole("Admin")) return true;
        return User.IsInRole("Instructor") && course.InstructorId == _userManager.GetUserId(User);
    }

    public async Task<IActionResult> Details(int id)
    {
        var a = await _db.Assignments.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == id);
        if (a?.Course == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var canManage = CanManage(a.Course);
        var enrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == a.CourseId && e.StudentId == userId);

        if (!canManage && (!enrolled || !a.Course.IsPublished))
        {
            TempData["Error"] = "Enroll in this course to open its assignments.";
            return RedirectToAction("Details", "Courses", new { id = a.CourseId });
        }

        var mine = await _db.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == userId);

        var all = new List<Submission>();
        if (canManage)
        {
            all = await _db.Submissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == id)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        return View(new AssignmentPageViewModel
        {
            Assignment = a,
            Course = a.Course,
            Mine = mine,
            All = all,
            CanManage = canManage,
            IsEnrolled = enrolled
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, string? content)
    {
        var a = await _db.Assignments.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == id);
        if (a?.Course == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var enrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == a.CourseId && e.StudentId == userId);
        if (!enrolled) return Forbid();

        if (string.IsNullOrWhiteSpace(content) || content.Length > 5000)
        {
            TempData["Error"] = "Write your answer. It can have up to 5000 characters.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var s = await _db.Submissions.FirstOrDefaultAsync(x => x.AssignmentId == id && x.StudentId == userId);

        if (s != null && s.Score != null)
        {
            TempData["Error"] = "This work is already graded.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (s == null)
        {
            _db.Submissions.Add(new Submission
            {
                AssignmentId = id,
                StudentId = userId,
                Content = content.Trim()
            });
        }
        else
        {
            s.Content = content.Trim();
            s.SubmittedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "Your work was submitted.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Grade(int id, int score, string? feedback)
    {
        var s = await _db.Submissions
            .Include(x => x.Assignment).ThenInclude(a => a!.Course)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (s?.Assignment?.Course == null) return NotFound();
        if (!CanManage(s.Assignment.Course)) return Forbid();

        var assignmentId = s.AssignmentId;

        if (score < 0 || score > s.Assignment.MaxScore)
        {
            TempData["Error"] = "Score must be between 0 and " + s.Assignment.MaxScore + ".";
            return RedirectToAction(nameof(Details), new { id = assignmentId });
        }

        s.Score = score;
        s.Feedback = string.IsNullOrWhiteSpace(feedback) ? null : feedback.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = "Grade saved.";
        return RedirectToAction(nameof(Details), new { id = assignmentId });
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        return View(new AssignmentFormViewModel { CourseId = courseId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(AssignmentFormViewModel model)
    {
        var course = await _db.Courses.FindAsync(model.CourseId);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        if (!ModelState.IsValid) return View(model);

        var assignment = new Assignment
        {
            CourseId = model.CourseId,
            Title = model.Title,
            Instructions = model.Instructions,
            DueDate = DateTime.SpecifyKind(model.DueDate, DateTimeKind.Utc),
            MaxScore = model.MaxScore
        };
        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Assignment created.";
        return RedirectToAction(nameof(Details), new { id = assignment.Id });
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id)
    {
        var a = await _db.Assignments.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == id);
        if (a?.Course == null) return NotFound();
        if (!CanManage(a.Course)) return Forbid();

        return View(new AssignmentFormViewModel
        {
            Id = a.Id,
            CourseId = a.CourseId,
            Title = a.Title,
            Instructions = a.Instructions,
            DueDate = a.DueDate,
            MaxScore = a.MaxScore
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(AssignmentFormViewModel model)
    {
        var a = await _db.Assignments.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == model.Id);
        if (a?.Course == null) return NotFound();
        if (!CanManage(a.Course)) return Forbid();

        if (!ModelState.IsValid) return View(model);

        a.Title = model.Title;
        a.Instructions = model.Instructions;
        a.DueDate = DateTime.SpecifyKind(model.DueDate, DateTimeKind.Utc);
        a.MaxScore = model.MaxScore;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Assignment updated.";
        return RedirectToAction(nameof(Details), new { id = a.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Assignments.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == id);
        if (a?.Course == null) return NotFound();
        if (!CanManage(a.Course)) return Forbid();

        var courseId = a.CourseId;
        _db.Assignments.Remove(a);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Assignment deleted.";
        return RedirectToAction("Details", "Courses", new { id = courseId });
    }
}