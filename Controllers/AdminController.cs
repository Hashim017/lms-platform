using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var since = DateTime.UtcNow.Date.AddDays(-6);
        var recent = await _db.Enrollments
            .Where(e => e.EnrolledAt >= since)
            .Select(e => e.EnrolledAt)
            .ToListAsync();

        var perDay = Enumerable.Range(0, 7)
            .Select(i => since.AddDays(i))
            .Select(d => new DayCount
            {
                Label = d.ToString("ddd"),
                Count = recent.Count(x => x.Date == d)
            })
            .ToList();

        var roleCounts = await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            group r by r.Name into g
            select new { Name = g.Key, N = g.Count() }).ToListAsync();

        var top = await _db.Courses
            .OrderByDescending(c => c.Enrollments.Count)
            .Take(5)
            .Select(c => new TopCourse
            {
                Title = c.Title,
                Instructor = c.Instructor == null ? "" : c.Instructor.FullName,
                Learners = c.Enrollments.Count
            })
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            Users = await _db.Users.CountAsync(),
            Students = roleCounts.FirstOrDefault(r => r.Name == "Student")?.N ?? 0,
            Instructors = roleCounts.FirstOrDefault(r => r.Name == "Instructor")?.N ?? 0,
            Courses = await _db.Courses.CountAsync(),
            Published = await _db.Courses.CountAsync(c => c.IsPublished),
            Enrollments = await _db.Enrollments.CountAsync(),
            Completed = await _db.Enrollments.CountAsync(e => e.CompletedAt != null),
            QuizAttempts = await _db.QuizAttempts.CountAsync(),
            Submissions = await _db.Submissions.CountAsync(),
            PerDay = perDay,
            Top = top
        };

        return View(model);
    }

    public async Task<IActionResult> Users(string? q)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.ToLower();
            query = query.Where(u => (u.Email ?? "").ToLower().Contains(s) || u.FullName.ToLower().Contains(s));
        }

        var users = await query.OrderBy(u => u.Email).ToListAsync();

        var roleMap = await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            select new { ur.UserId, r.Name }).ToListAsync();

        var enrolled = await _db.Enrollments
            .GroupBy(e => e.StudentId)
            .Select(g => new { Id = g.Key, N = g.Count() })
            .ToListAsync();

        var rows = users.Select(u => new UserRow
        {
            Id = u.Id,
            Email = u.Email ?? "",
            FullName = u.FullName,
            CreatedAt = u.CreatedAt,
            Role = roleMap.FirstOrDefault(r => r.UserId == u.Id)?.Name ?? "Student",
            Enrolled = enrolled.FirstOrDefault(e => e.Id == u.Id)?.N ?? 0
        }).ToList();

        ViewBag.Q = q;
        ViewBag.Me = _userManager.GetUserId(User);
        return View(rows);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string id, string role)
    {
        var allowed = new[] { "Admin", "Instructor", "Student" };
        if (!allowed.Contains(role)) return BadRequest();

        if (id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot change your own role.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var current = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, current);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = (user.Email ?? "User") + " is now " + role + ". The change shows after the next login.";
        return RedirectToAction(nameof(Users));
    }
}