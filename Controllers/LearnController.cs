using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[Authorize]
public class LearnController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public LearnController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Lesson(int id)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Course).ThenInclude(c => c!.Instructor)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson?.Course == null) return NotFound();

        var course = lesson.Course;
        var userId = _userManager.GetUserId(User)!;

        var isEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == course.Id && e.StudentId == userId);
        var canManage = User.IsInRole("Admin") || course.InstructorId == userId;

        if (!course.IsPublished && !canManage) return NotFound();

        if (!isEnrolled && !canManage)
        {
            TempData["Error"] = "Enroll in this course to open its lessons.";
            return RedirectToAction("Details", "Courses", new { id = course.Id });
        }

        var lessons = await _db.Lessons
            .Where(l => l.CourseId == course.Id)
            .OrderBy(l => l.Order)
            .ToListAsync();

        var lessonIds = lessons.Select(l => l.Id).ToList();
        var doneList = await _db.LessonProgresses
            .Where(p => p.StudentId == userId && lessonIds.Contains(p.LessonId))
            .Select(p => p.LessonId)
            .ToListAsync();

        var index = lessons.FindIndex(l => l.Id == id);

        var vm = new LessonPageViewModel
        {
            Course = course,
            Lesson = lesson,
            Lessons = lessons,
            DoneIds = doneList.ToHashSet(),
            IsEnrolled = isEnrolled,
            Prev = index > 0 ? lessons[index - 1] : null,
            Next = index >= 0 && index < lessons.Count - 1 ? lessons[index + 1] : null
        };

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var lesson = await _db.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;

        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == lesson.CourseId && e.StudentId == userId);
        if (enrollment == null) return Forbid();

        var existing = await _db.LessonProgresses
            .FirstOrDefaultAsync(p => p.LessonId == id && p.StudentId == userId);

        var markedDone = existing == null;
        if (existing == null)
            _db.LessonProgresses.Add(new LessonProgress { LessonId = id, StudentId = userId });
        else
            _db.LessonProgresses.Remove(existing);

        await _db.SaveChangesAsync();

        var lessonIds = await _db.Lessons
            .Where(l => l.CourseId == lesson.CourseId)
            .OrderBy(l => l.Order)
            .Select(l => l.Id)
            .ToListAsync();

        var doneCount = await _db.LessonProgresses
            .CountAsync(p => p.StudentId == userId && lessonIds.Contains(p.LessonId));

        var allDone = lessonIds.Count > 0 && doneCount == lessonIds.Count;
        enrollment.CompletedAt = allDone ? (enrollment.CompletedAt ?? DateTime.UtcNow) : null;
        await _db.SaveChangesAsync();

        if (markedDone)
        {
            if (allDone)
            {
                TempData["Celebrate"] = "1";
                return RedirectToAction("Details", "Courses", new { id = lesson.CourseId });
            }

            var position = lessonIds.IndexOf(id);
            if (position >= 0 && position < lessonIds.Count - 1)
            {
                TempData["Success"] = "Lesson complete. Keep going!";
                return RedirectToAction(nameof(Lesson), new { id = lessonIds[position + 1] });
            }
        }

        return RedirectToAction(nameof(Lesson), new { id });
    }
}