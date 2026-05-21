using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Venue;

public sealed class VenueChangeRequestCreateDto
{
    [Range(1, int.MaxValue)]
    public int RequestedVenueId { get; set; }
}
