using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Event;

public sealed class EventDto
{
    public int EventId { get; set; }

    public int VenueId { get; set; }

    public int MenuId { get; set; }

    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }

    public DateTime EventDate { get; set; }

    public int EstimatedGuests { get; set; }
    
    public decimal TotalBudget { get; set; }
    
    [MaxLength(256)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}