using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Task;

public sealed class WeddingTaskCreateDto
{
    [Required]
    [MaxLength(120)]
    public required string Title { get; set; }

    public DateTime DueDate { get; set; }

    [Required]
    [MaxLength(32)]
    public string Priority { get; set; } = "medium";

    [Required]
    [MaxLength(32)]
    public string Category { get; set; } = "General";

    [MaxLength(256)]
    public string? Notes { get; set; }
}
