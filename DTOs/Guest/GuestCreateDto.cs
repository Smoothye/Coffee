using System.ComponentModel.DataAnnotations;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Guest;

public sealed class GuestCreateDto
{
    [Range(0, int.MaxValue, ErrorMessage = "TableId must be a positive number.")]
    public int? TableId { get; set; }

    [RegularExpression(
        @"^[\p{L}\p{M}][\p{L}\p{M}' -]*$",
        ErrorMessage = "FirstName can only contain letters, spaces, apostrophes, and hyphens."
    )]
    public required string FirstName { get; set; }
    
    [RegularExpression(
        @"^[\p{L}\p{M}][\p{L}\p{M}' -]*$",
        ErrorMessage = "LastName can only contain letters, spaces, apostrophes, and hyphens."
    )]
    public required string LastName { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Age must be a positive number.")]
    public int? Age { get; set; }

    [RegularExpression(@"^$|^[^\s@]+@[^\s@]+$", ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    public string? Phone { get; set; }
    public Gender? Gender { get; set; }

    public RsvpStatus RsvpStatus { get; set; } = RsvpStatus.Pending;
    public Group Group { get; set; } = Group.Other;
    public DietaryRequirements DietaryRequirements { get; set; } = DietaryRequirements.Standard;

    public bool HasPlusOne { get; set; } = false;
    public int? SeatNumber { get; set; }

    public string? Notes { get; set; }
}
