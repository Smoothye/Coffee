using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Event;

public sealed class EventMenuSelectionDto
{
    public int MenuId { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    public MenuDietaryType DietaryType { get; set; }

    public string? Description { get; set; }

    public List<EventMenuSelectionItemDto> Items { get; set; } = [];
}