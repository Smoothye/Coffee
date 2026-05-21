using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Supplier;

public sealed class EventSupplierStatusUpdateDto
{
    [MaxLength(32)]
    public string? CollaborationStatus { get; set; }

    public bool Paid { get; set; }

    [MaxLength(256)]
    public string? Notes { get; set; }
}
