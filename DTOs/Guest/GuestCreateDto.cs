using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Guest;

public sealed class GuestCreateDto
{
    public int? TableId { get; set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public int? Age { get; set; }

    public required string Email { get; set; }

    public string? Phone { get; set; }
    public Gender? Gender { get; set; }

    public RsvpStatus RsvpStatus { get; set; } = RsvpStatus.Pending;
    public Group Group { get; set; } = Group.Other;
    public DietaryRequirements DietaryRequirements { get; set; } = DietaryRequirements.None;

    public string? Notes { get; set; }
}