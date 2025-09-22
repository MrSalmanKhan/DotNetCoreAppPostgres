using System.ComponentModel.DataAnnotations;

public class AppUserRole
{
    [Required]
    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    [Required]
    public int RoleId { get; set; }

    public AppRole Role { get; set; } = null!;
}