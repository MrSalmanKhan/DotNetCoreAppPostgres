using System.ComponentModel.DataAnnotations;

public class AppRole
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = ""; // e.g., "Admin", "User"

    // Navigation
    public List<AppUserRole> UserRoles { get; set; } = new();
}