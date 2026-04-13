using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

public class Venue
{
    [Key]
    public int VenueId { get; init; }
    
    [Required, MaxLength(64)]
    public required string Name { get; init; }
    
    [Required, MaxLength(64)]
    public required string Address { get; init; }
    
    public int MinCapacity { get; init; }
    public int MaxCapacity { get; init; }
    
    [Precision(8, 2)]
    public decimal EstimatedPrice { get; init; }
    
    public ICollection<Event> Events { get; set; } = [];
}