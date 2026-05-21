using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Menu;

public sealed class MenuDto
{
    public int MenuId { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public MenuDietaryType DietaryType { get; set; }
    public string? Description { get; set; }
    public List<MenuItemDto> MenuItems { get; set; } = [];
}
