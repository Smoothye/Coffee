using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<int>
{
    // [Required]
    public string? FirstName { get; set; }
    
    // [Required]
    public string? LastName { get; set; }    
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<UserEvent> UserEvents { get; set; } = [];
}