using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[Authorize(Roles = "Admin,Instructor")]
public class LessonsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public LessonsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private bool CanManage(Course course)
    {
        if (User.IsInRole("Admin")) return true;
        return course.InstructorId == _userManager.GetUserId(User);
    }

    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        var lastOrder = await _db.Lessons
            .Where(l => l.CourseId == courseId)
            .MaxAsync(l => (int?)l.Order) ?? 0;

        return View(new LessonFormViewModel { CourseId = courseId, Order = lastOrder + 1 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonFormViewModel model)
    {
        var course = await _db.Courses.FindAsync(model.CourseId);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        if (!ModelState.IsValid) return View(model);

        _db.Lessons.Add(new Lesson
        {
            CourseId = model.CourseId,
            Title = model.Title,
            Content = model.Content,
            VideoUrl = model.VideoUrl,
            Order = model.Order
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Lesson added.";
        return RedirectToAction("Details", "Courses", new { id = model.CourseId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var lesson = await _db.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.Id == id);
        if (lesson == null) return NotFound();
        if (!CanManage(lesson.Course!)) return Forbid();

        return View(new LessonFormViewModel
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            Order = lesson.Order
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonFormViewModel model)
    {
        var lesson = await _db.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.Id == model.Id);
        if (lesson == null) return NotFound();
        if (!CanManage(lesson.Course!)) return Forbid();

        if (!ModelState.IsValid) return View(model);

        lesson.Title = model.Title;
        lesson.Content = model.Content;
        lesson.VideoUrl = model.VideoUrl;
        lesson.Order = model.Order;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Lesson updated.";
        return RedirectToAction("Details", "Courses", new { id = lesson.CourseId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await _db.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.Id == id);
        if (lesson == null) return NotFound();
        if (!CanManage(lesson.Course!)) return Forbid();

        var courseId = lesson.CourseId;
        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Lesson deleted.";
        return RedirectToAction("Details", "Courses", new { id = courseId });
    }
}