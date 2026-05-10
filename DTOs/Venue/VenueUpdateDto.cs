using System.ComponentModel.DataAnnotations;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Venue;

public sealed class VenueUpdateDto
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

    [Range(typeof(decimal), "-90.000000", "90.000000")]
    public decimal? Latitude { get; set; }

    [Range(typeof(decimal), "-180.000000", "180.000000")]
    public decimal? Longitude { get; set; }

    [MaxLength(1024)]
    public string? Description { get; set; }

    [Range(0, 5)]
    public int? Rating { get; set; }

    public VenueTag? Tags { get; set; }

    [MaxLength(512)]
    public string? ImagePath { get; set; }
}