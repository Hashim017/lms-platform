using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[Authorize]
public class EnrollmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> MyLearning()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        ViewBag.Name = string.IsNullOrWhiteSpace(user.FullName) ? "there" : user.FullName;

        var enrollments = await _db.Enrollments
            .Include(e => e.Course).ThenInclude(c => c!.Lessons)
            .Include(e => e.Course).ThenInclude(c => c!.Instructor)
            .Where(e => e.StudentId == user.Id)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var doneList = await _db.LessonProgresses
            .Where(p => p.StudentId == user.Id)
            .Select(p => p.LessonId)
            .ToListAsync();
        var doneIds = doneList.ToHashSet();

        var items = new List<MyCourseItem>();
        foreach (var e in enrollments)
        {
            if (e.Course == null) continue;

            var lessons = e.Course.Lessons.OrderBy(l => l.Order).ToList();
            var done = lessons.Count(l => doneIds.Contains(l.Id));
            var resume = lessons.FirstOrDefault(l => !doneIds.Contains(l.Id)) ?? lessons.FirstOrDefault();

            items.Add(new MyCourseItem
            {
                Course = e.Course,
                TotalLessons = lessons.Count,
                DoneLessons = done,
                IsCompleted = lessons.Count > 0 && done == lessons.Count,
                ResumeLessonId = resume?.Id
            });
        }

        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null || !course.IsPublished) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var exists = await _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == userId);

        if (!exists)
        {
            _db.Enrollments.Add(new Enrollment { CourseId = courseId, StudentId = userId });
            await _db.SaveChangesAsync();
            TempData["Success"] = "You are enrolled. Let's start learning!";
        }

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var userId = _userManager.GetUserId(User)!;

        var lessonIds = await _db.Lessons
            .Where(l => l.CourseId == courseId)
            .Select(l => l.Id)
            .ToListAsync();

        var progress = await _db.LessonProgresses
            .Where(p => p.StudentId == userId && lessonIds.Contains(p.LessonId))
            .ToListAsync();
        _db.LessonProgresses.RemoveRange(progress);

        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);
        if (enrollment != null) _db.Enrollments.Remove(enrollment);

        await _db.SaveChangesAsync();

        TempData["Success"] = "You left the course.";
        return RedirectToAction("Details", "Courses", new { id = courseId });
    }
}