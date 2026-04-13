using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

public class Event
{
    [Key]
    public int EventId { get; init; }
    
    public int VenueId { get; init; }
    [Required, ForeignKey(nameof(VenueId))]
    public required Venue Venue { get; init; }
    
    public int MenuId { get; set; }
    [Required, ForeignKey(nameof(MenuId))]
    public required Menu Menu { get; set; }
    
    [Required, MaxLength(128)]
    public required string Name { get; set; }
    
    public DateTime EventDate { get; set; }
    
    public int EstimatedGuests { get; set; }
    
    [Precision(8, 2)]
    public decimal TotalBudget { get; set; }
    
    [MaxLength(256)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserEvent> UserEvents { get; set; } = [];
    public ICollection<Guest> Guests { get; set; } = [];
    public ICollection<WeddingTable> WeddingTables { get; set; } = [];
    public ICollection<CheckListTask> Tasks { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
    public ICollection<EventSupplier> EventSuppliers { get; set; } = [];
    public ICollection<ScheduleItem> ScheduleItems { get; set; } = [];
}