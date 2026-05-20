using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Menu;

public sealed class MenuItemCreateDto
{
    [Required]
    [MaxLength(64)]
    public required string CourseName { get; set; }

    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}
