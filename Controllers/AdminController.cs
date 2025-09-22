using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreAppPostgres.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Show all users with their roles
    public async Task<IActionResult> ManageUsers()
    {
        var users = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(int userId, string roleName)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return Json(new { success = false, message = "User not found" });

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            role = new AppRole { Name = roleName };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
        }

        if (!user.UserRoles.Any(ur => ur.RoleId == role.Id))
        {
            _db.UserRoles.Add(new AppUserRole { UserId = user.Id, RoleId = role.Id });
            await _db.SaveChangesAsync();
        }

        // Reload roles to return updated list
        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        return Json(new { success = true, roles });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRole(int userId, string roleName)
    {
        var userRole = await _db.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);

        if (userRole == null)
            return Json(new { success = false, message = "Role not found for this user" });

        // Prevent removing last Admin
        if (roleName == "Admin")
        {
            var totalAdmins = await _db.UserRoles
                .Include(ur => ur.Role)
                .CountAsync(ur => ur.Role.Name == "Admin");

            if (totalAdmins <= 1)
                return Json(new { success = false, message = "You cannot remove the last Admin user." });
        }

        _db.UserRoles.Remove(userRole);
        await _db.SaveChangesAsync();

        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        return Json(new { success = true, roles });
    }
}