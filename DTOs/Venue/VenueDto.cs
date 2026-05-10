using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Venue;

public class VenueDto
{
    public int VenueId { get; set; }

    public required string Name { get; set; }
    public required string Address { get; set; }

    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }

    public decimal EstimatedPrice { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public string? Description { get; set; }

    public int? Rating { get; set; }

    public VenueTag? Tags { get; set; }

    public string? ImagePath { get; set; }
}