namespace WeddingPlannerApp.DTOs.Event;

public sealed class EventMenuSelectionItemDto
{
    public int MenuItemId { get; set; }

    public required string CourseName { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}