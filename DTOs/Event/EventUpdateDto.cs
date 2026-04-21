using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Event;

public sealed class EventUpdateDto
{
    [Required]
    public int VenueId { get; set; }

    [Required]
    public int MenuId { get; set; }

    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Range(0, int.MaxValue)]
    public int EstimatedGuests { get; set; }

    [Range(typeof(decimal), "0", "99_999_999.99")]
    public decimal TotalBudget { get; set; }

    [MaxLength(256)]
    public string? Notes { get; set; }
}