using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Supplier;

public sealed class SupplierUpdateDto
{
    [MaxLength(64)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(120)]
    public string? Email { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal Price { get; set; }

    public bool Paid { get; set; }
}
