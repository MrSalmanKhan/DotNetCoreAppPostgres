using System.ComponentModel.DataAnnotations;

public class ChatMessage
{
    public int Id { get; set; }

    public int AppUserId { get; set; }

    [MaxLength(1024)]
    public string? PictureUrl { get; set; }

    [Required]
    [MaxLength(4096)]
    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public AppUser AppUser { get; set; } = null!;
}