using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public class DbRoleClaimsTransformer : IClaimsTransformation
{
    private readonly ApplicationDbContext _db;

    public DbRoleClaimsTransformer(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var identity = (ClaimsIdentity)principal.Identity;

        // Avoid duplicate re-adding
        if (identity.HasClaim("RolesLoaded", "true"))
            return principal;

        var email = identity.FindFirst(ClaimTypes.Email)?.Value
                    ?? identity.Name; // fallback to Name if email missing

        if (string.IsNullOrEmpty(email))
            return principal;

        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
        {
            // Remove existing role claims
            var existingRoles = identity.FindAll(ClaimTypes.Role).ToList();
            foreach (var r in existingRoles)
                identity.RemoveClaim(r);

            // Add fresh roles
            foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Mark so we don’t reload again in this request
            identity.AddClaim(new Claim("RolesLoaded", "true"));
        }

        return principal;
    }
}
