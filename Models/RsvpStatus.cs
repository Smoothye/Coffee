using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.Models;

public class RsvpStatus
{
    [Key]
    public int RsvpStatusId { get; set; }
    
    [Required, MaxLength(32)]
    public required string StatusName { get; set; }
    
    public ICollection<Guest> Guests { get; set; } = [];
}