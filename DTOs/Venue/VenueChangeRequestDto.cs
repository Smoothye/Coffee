namespace WeddingPlannerApp.DTOs.Venue;

public sealed class VenueChangeRequestDto
{
    public int EventId { get; set; }
    public required string EventName { get; set; }
    public DateTime EventDate { get; set; }
    public required string Status { get; set; }
    public int EstimatedGuests { get; set; }
    public int ActualGuests { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerEmail { get; set; }
    public int CurrentVenueId { get; set; }
    public required string CurrentVenueName { get; set; }
    public int RequestedVenueId { get; set; }
    public required string RequestedVenueName { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal RequestedVenuePrice { get; set; }
    public bool FitsCapacity { get; set; }
    public bool FitsBudget { get; set; }
}
