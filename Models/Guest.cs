using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlannerApp.Models;

public class Guest
{
    [Key]
    public int GuestId { get; set; }
    
    public int EventId { get; set; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; set; }   
    
    public int? TableId { get; set; }
    [ForeignKey(nameof(TableId))]
    public WeddingTable? WeddingTable { get; set; }
    
    public int RsvpStatusId { get; set; }
    [Required, ForeignKey(nameof(RsvpStatusId))]
    public required RsvpStatus RsvpStatus { get; set; }
    
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Category { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? DietaryRequirements { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}