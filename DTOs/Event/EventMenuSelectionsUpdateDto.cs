using System.ComponentModel.DataAnnotations;

namespace WeddingPlannerApp.DTOs.Event;

public sealed class EventMenuSelectionsUpdateDto
{
    [Required]
    public List<int> MenuIds { get; set; } = [];
}