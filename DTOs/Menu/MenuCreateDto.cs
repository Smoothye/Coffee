using System.ComponentModel.DataAnnotations;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.DTOs.Menu;

public sealed class MenuCreateDto
{
    [Required]
    [MaxLength(64)]
    public required string Name { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Price { get; set; }

    public MenuDietaryType DietaryType { get; set; } = MenuDietaryType.Standard;

    [MaxLength(256)]
    public string? Description { get; set; }

    [Required]
    public List<MenuItemCreateDto> MenuItems { get; set; } = [];
}
