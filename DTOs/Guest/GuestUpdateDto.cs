using System.ComponentModel.DataAnnotations;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Guest;

public sealed class GuestUpdateDto
{
    
    public int? TableId { get; set; }
    
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    
    public int? Age { get; set; }
    
    public required string Email { get; set; }
    
    public string? Phone { get; set; }
    public Gender? Gender { get; set; }
    
    public RsvpStatus RsvpStatus { get; set; }
    public Group Group { get; set; }
    public DietaryRequirements DietaryRequirements { get; set; }
    
    public string? Notes { get; set; }
}