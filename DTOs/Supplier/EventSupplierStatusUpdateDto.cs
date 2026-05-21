using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Supplier;

public sealed class EventSupplierStatusUpdateDto
{
    [MaxLength(32)]
    public string? CollaborationStatus { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Advance { get; set; }

    public bool RemainingPaid { get; set; }

    [MaxLength(256)]
    public string? Notes { get; set; }
}
