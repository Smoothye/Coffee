namespace WeddingPlannerApp.DTOs.Menu;

public sealed class MenuItemDto
{
    public int MenuItemId { get; set; }
    public int MenuId { get; set; }
    public required string CourseName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
