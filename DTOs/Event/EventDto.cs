using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Event;

public sealed class EventDto
{
    public int EventId { get; set; }

    public int VenueId { get; set; }

    public List<int> MenuIds { get; set; } = [];

    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(128)]
    public string? BrideName { get; set; }

    [MaxLength(128)]
    public string? GroomName { get; set; }

    public DateTime EventDate { get; set; }

    public int EstimatedGuests { get; set; }
    
    public decimal TotalBudget { get; set; }

    public string? OwnerName { get; set; }

    public string? OwnerEmail { get; set; }
    
    [MaxLength(256)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}
