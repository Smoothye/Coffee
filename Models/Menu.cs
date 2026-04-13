using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlannerApp.Models;

public class Menu
{
    [Key]
    public int MenuId { get; set; }
    
    [Required, MaxLength(64)]
    public required string Name { get; set; }
    
    [Precision(8, 2)]
    public decimal Price { get; set; }
    
    [MaxLength(256)]
    public string? Description { get; set; }

    public ICollection<Event> Events { get; set; } = [];
}