using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;

namespace WeddingPlannerApp.Models;

[PrimaryKey(nameof(UserId), nameof(EventId))]
public class UserEvent
{
    public int UserId { get; init; }
    [Required, ForeignKey(nameof(UserId))]
    public required ApplicationUser User { get; init; }
    
    public int EventId { get; init; }
    [Required, ForeignKey(nameof(EventId))]
    public required Event Event { get; init; }
}