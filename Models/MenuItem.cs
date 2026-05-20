using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public class MenuItem
{
    [Key]
    public int MenuItemId { get; init; }

    public int MenuId { get; set; }

    [Required, ForeignKey(nameof(MenuId))]
    public Menu? Menu { get; set; }

    [Required, MaxLength(64)]
    public required string CourseName { get; set; }

    [Required, MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}
