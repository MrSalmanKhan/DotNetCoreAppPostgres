using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000)]
    [Precision(9, 3)]
    public decimal Price { get; set; }

    [MaxLength(4098)]
    public string? Description { get; set; }

    public string GetDescriptionForDisplay()
    {
        if (!string.IsNullOrWhiteSpace(this.Description))
        {
            if (this.Description.Length > 64)
            {
                return this.Description.Substring(0, 64) + " ...";
            }
            else
            {
                return this.Description;
            }
        }
        else
        {
            return "";
        }
    }
}
