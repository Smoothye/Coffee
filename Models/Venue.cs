using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

public enum VenueTag
{
    Garden,
    OpenSpace,
    Indoor,
    Outdoor,
    Ballroom,
    BanquetHall,
    Restaurant,
    Hotel,
    Barn,
    Beach,
    Vineyard,
    Rooftop,
    Historic,
    Luxury,
    Rustic,
    Modern,
    Waterfront,
    MountainView,
    Forest,
    Chapel,
    Castle,
    Resort,
    CountryClub,
    SmallCapacity,
    LargeCapacity,
    BudgetFriendly,
    Premium
}

public class Venue
{
    [Key]
    public int VenueId { get; init; }

    [Required, MaxLength(64)]
    public required string Name { get; set; }

    [Required, MaxLength(64)]
    public required string Address { get; set; }

    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }

    [Precision(8, 2)]
    public decimal EstimatedPrice { get; set; }

    [Precision(9, 6)]
    public decimal? Latitude { get; set; }

    [Precision(9, 6)]
    public decimal? Longitude { get; set; }

    [MaxLength(1024)]
    public string? Description { get; set; }

    [Range(0, 5)]
    public int? Rating { get; set; }

    public VenueTag? Tags { get; set; }

    [MaxLength(512)]
    public string? ImagePath { get; set; }

    public ICollection<Event> Events { get; set; } = [];
}