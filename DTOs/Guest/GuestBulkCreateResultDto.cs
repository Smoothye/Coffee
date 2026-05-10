namespace WeddingPlannerApp.DTOs.Guest;

public sealed class GuestBulkCreateResultDto
{
    public int CreatedCount { get; set; }

    public required IEnumerable<GuestDto> Guests { get; set; }
}