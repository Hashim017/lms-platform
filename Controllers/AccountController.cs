using System.ComponentModel.DataAnnotations;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AccountController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser> users,
        ApplicationDbContext db,
        IConfiguration config)
    {
        _signIn = signIn;
        _users = users;
        _db = db;
        _config = config;
    }

    private string SafeUrl(string? returnUrl, string fallback)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : fallback;
    }

    private IActionResult Back(string tab)
    {
        return RedirectToAction(nameof(Settings), new { tab });
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewBag.ShowDemo = _config["SeedDemoData"] == "true";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewBag.ShowDemo = _config["SeedDemoData"] == "true";

        if (!ModelState.IsValid) return View(model);

        var user = await _users.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var result = await _signIn.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return LocalRedirect(SafeUrl(model.ReturnUrl, "/"));
        }

        ModelState.AddModelError(string.Empty, "Wrong email or password.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName.Trim(),
            EmailConfirmed = true
        };

        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _users.AddToRoleAsync(user, DbSeeder.StudentRole);
        await _signIn.SignInAsync(user, isPersistent: false);

        var first = user.FullName.Split(' ')[0];
        TempData["Success"] = "Welcome to Lumen, " + first + "!";
        return LocalRedirect(SafeUrl(model.ReturnUrl, Url.Action("Index", "Courses") ?? "/"));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Settings(string tab = "profile")
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await _users.GetRolesAsync(user);

        return View(new AccountSettingsViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "Student",
            CreatedAt = user.CreatedAt,
            Tab = tab
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> UpdateProfile(string? fullName)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 80)
        {
            TempData["Error"] = "Enter your name. It can have up to 80 characters.";
            return Back("profile");
        }

        user.FullName = fullName.Trim();
        await _users.UpdateAsync(user);

        TempData["Success"] = "Profile saved.";
        return Back("profile");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> ChangeEmail(string? newEmail, string? password)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrWhiteSpace(newEmail) || !new EmailAddressAttribute().IsValid(newEmail))
        {
            TempData["Error"] = "Enter a valid email address.";
            return Back("email");
        }

        if (!await _users.CheckPasswordAsync(user, password ?? ""))
        {
            TempData["Error"] = "Your password is wrong.";
            return Back("email");
        }

        var existing = await _users.FindByEmailAsync(newEmail);
        if (existing != null && existing.Id != user.Id)
        {
            TempData["Error"] = "That email is already in use.";
            return Back("email");
        }

        var r1 = await _users.SetEmailAsync(user, newEmail);
        var r2 = await _users.SetUserNameAsync(user, newEmail);
        if (!r1.Succeeded || !r2.Succeeded)
        {
            TempData["Error"] = "Could not change the email. Try again.";
            return Back("email");
        }

        await _signIn.RefreshSignInAsync(user);
        TempData["Success"] = "Email changed. Use the new email next time you log in.";
        return Back("email");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> ChangePassword(string? currentPassword, string? newPassword, string? confirmPassword)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
        {
            TempData["Error"] = "The new passwords do not match.";
            return Back("password");
        }

        var result = await _users.ChangePasswordAsync(user, currentPassword ?? "", newPassword);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return Back("password");
        }

        await _signIn.RefreshSignInAsync(user);
        TempData["Success"] = "Password changed.";
        return Back("password");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> DeleteAccount(string? password)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        if (await _users.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "Admin accounts cannot be deleted here.";
            return Back("delete");
        }

        if (await _db.Courses.AnyAsync(c => c.InstructorId == user.Id))
        {
            TempData["Error"] = "You still own courses. Delete them first.";
            return Back("delete");
        }

        if (!await _users.CheckPasswordAsync(user, password ?? ""))
        {
            TempData["Error"] = "Your password is wrong.";
            return Back("delete");
        }

        await _users.DeleteAsync(user);
        await _signIn.SignOutAsync();

        TempData["Success"] = "Your account was deleted.";
        return RedirectToAction("Index", "Home");
    }
}