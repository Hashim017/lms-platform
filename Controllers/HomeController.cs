using System.Diagnostics;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            CourseCount = await _db.Courses.CountAsync(c => c.IsPublished),
            LessonCount = await _db.Lessons.CountAsync(l => l.Course!.IsPublished),
            StudentCount = await _db.Users.CountAsync(),
            InstructorCount = await _db.Courses.Select(c => c.InstructorId).Distinct().CountAsync(),
            Featured = await _db.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.Enrollments.Count)
                .ThenByDescending(c => c.CreatedAt)
                .Take(3)
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}