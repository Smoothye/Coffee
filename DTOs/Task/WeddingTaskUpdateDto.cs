using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Task;

public sealed class WeddingTaskUpdateDto
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

    public bool Done { get; set; }

    [MaxLength(256)]
    public string? Notes { get; set; }
}
