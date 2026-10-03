using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoursesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private bool CanManage(Course course)
    {
        if (User.IsInRole("Admin")) return true;
        return User.IsInRole("Instructor") && course.InstructorId == _userManager.GetUserId(User);
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Where(c => c.IsPublished)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        ViewBag.Categories = courses.Select(c => c.Category).Distinct().OrderBy(c => c).ToList();

        var userId = _userManager.GetUserId(User);
        var enrolledIds = new HashSet<int>();
        if (userId != null)
        {
            var ids = await _db.Enrollments
                .Where(e => e.StudentId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();
            enrolledIds = ids.ToHashSet();
        }
        ViewBag.EnrolledIds = enrolledIds;

        return View(courses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Quizzes).ThenInclude(q => q.Questions)
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        var canManage = CanManage(course);
        if (!course.IsPublished && !canManage) return NotFound();

        var userId = _userManager.GetUserId(User);
        var isEnrolled = false;
        var done = new HashSet<int>();

        if (userId != null)
        {
            isEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == id && e.StudentId == userId);
            var lessonIds = course.Lessons.Select(l => l.Id).ToList();
            var doneList = await _db.LessonProgresses
                .Where(p => p.StudentId == userId && lessonIds.Contains(p.LessonId))
                .Select(p => p.LessonId)
                .ToListAsync();
            done = doneList.ToHashSet();
        }

        ViewBag.CanManage = canManage;
        ViewBag.IsEnrolled = isEnrolled;
        ViewBag.DoneIds = done;
        ViewBag.EnrollCount = await _db.Enrollments.CountAsync(e => e.CourseId == id);
        return View(course);
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Manage()
    {
        var userId = _userManager.GetUserId(User);

        var query = _db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
            query = query.Where(c => c.InstructorId == userId);

        var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(courses);
    }

    [Authorize(Roles = "Admin,Instructor")]
    public IActionResult Create()
    {
        return View(new CourseFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var course = new Course
        {
            Title = model.Title,
            Description = model.Description,
            Category = model.Category,
            Level = model.Level,
            IsPublished = model.IsPublished,
            InstructorId = _userManager.GetUserId(User)!
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Course created.";
        return RedirectToAction(nameof(Details), new { id = course.Id });
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        return View(new CourseFormViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            Level = course.Level,
            IsPublished = course.IsPublished
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(CourseFormViewModel model)
    {
        var course = await _db.Courses.FindAsync(model.Id);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        if (!ModelState.IsValid) return View(model);

        course.Title = model.Title;
        course.Description = model.Description;
        course.Category = model.Category;
        course.Level = model.Level;
        course.IsPublished = model.IsPublished;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Course updated.";
        return RedirectToAction(nameof(Details), new { id = course.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        if (!CanManage(course)) return Forbid();

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Course deleted.";
        return RedirectToAction(nameof(Manage));
    }
}