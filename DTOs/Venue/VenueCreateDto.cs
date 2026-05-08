using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Venue;

public sealed class VenueCreateDto
{
    [Required]
    [MaxLength(64)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Address { get; set; }

    [Range(0, int.MaxValue)]
    public int MinCapacity { get; set; }

    [Range(0, int.MaxValue)]
    public int MaxCapacity { get; set; }

    [Range(typeof(decimal), "0", "999999.99")]
    public decimal EstimatedPrice { get; set; }
}