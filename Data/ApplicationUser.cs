using Microsoft.AspNetCore.Identity;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Data;

public class ApplicationUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserEvent> UserEvents { get; set; } = [];
}
