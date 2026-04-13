using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

public class Venue
{
    [Key]
    public int VenueId { get; set; }
    
    [Required, MaxLength(64)]
    public required string Name { get; set; }
    
    [Required, MaxLength(64)]
    public required string Address { get; set; }
    
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    
    [Precision(8, 2)]
    public decimal EstimatedPrice { get; set; }
    
    public ICollection<Event> Events { get; set; } = [];
}