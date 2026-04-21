using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public enum RsvpStatus
{
    Pending,
    Declined,
    Confirmed
}

public enum Gender
{
    Male,
    Female,
    Other
}

public enum Group
{
    Friends,
    Family,
    Colleagues,
    Other
}

public enum DietaryRequirements
{
    None,
    Vegan,
    Vegetarian,
    GlutenFree,
    Halal,
    Kosher,
    Other
}

public class Guest
{
    [Key]
    public int GuestId { get; init; }
    
    public int EventId { get; init; }
    [Required, ForeignKey(nameof(EventId))]
    public Event? Event { get; init; }   
    
    public int? TableId { get; set; }
    [ForeignKey(nameof(TableId))]
    public WeddingTable? WeddingTable { get; set; }
    
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    public int? Age { get; set; }
    
    [Required]
    public required string Email { get; init; }
    
    public string? Phone  { get; set; }
    public Gender? Gender { get; set; }
    
    public RsvpStatus           RsvpStatus          { get; set; } = RsvpStatus.Pending;
    public Group                Group               { get; set; } = Group.Other;
    public DietaryRequirements  DietaryRequirements { get; set; } = DietaryRequirements.None;
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}