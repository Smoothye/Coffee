namespace WeddingPlannerApp.DTOs.Venue;

public class VenueDto
{
    public int VenueId { get; set; }
    
    public required string Name { get; set; }
    public required string Address { get; set; }
    
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }

    public decimal EstimatedPrice { get; set; }
}