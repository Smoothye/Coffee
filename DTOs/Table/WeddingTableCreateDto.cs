using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Table;

public sealed class WeddingTableCreateDto
{
    [Range(0, int.MaxValue, ErrorMessage = "TableNumber must be a non-negative number.")]
    public int TableNumber { get; set; }

    [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50.")]
    public int Capacity { get; set; }

    public bool IsHeadTable { get; set; }

    [MaxLength(256)]
    public string? Notes { get; set; }
}
