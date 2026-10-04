using System.Security.Claims;
using LMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LMS.Data;

public class AppClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly UserManager<ApplicationUser> _users;

    public AppClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
        _users = userManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var roles = await _users.GetRolesAsync(user);
        if (roles.Count == 0)
            await _users.AddToRoleAsync(user, DbSeeder.StudentRole);

        return await base.GenerateClaimsAsync(user);
    }
}