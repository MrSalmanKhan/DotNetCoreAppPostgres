using System.ComponentModel.DataAnnotations;

public class AppUser
{
    [Key]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Url]
    [MaxLength(300)]
    public string? PictureUrl { get; set; }

    // Navigation properties
    public List<AppUserRole> UserRoles { get; set; } = new();
    public List<ChatMessage> ChatMessages { get; set; } = new();
}