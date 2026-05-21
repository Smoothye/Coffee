using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Supplier;

public sealed class SupplierCreateDto
{
    [Required]
    [MaxLength(120)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(32)]
    public string Category { get; set; } = "Other";

    [MaxLength(64)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(120)]
    public string? Email { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Advance { get; set; }
}
