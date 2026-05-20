using WeddingPlannerApp.DTOs.Event;
using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Menu;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<Menu>> GetMenusAsync() =>
        await _http.GetFromJsonAsync<List<Menu>>("api/Menus") ?? [];

    public async Task<Menu> CreateMenuAsync(Menu menu)
    {
        var response = await _http.PostAsJsonAsync("api/Menus", ToMenuCreateDto(menu));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Menu>()
            ?? throw new InvalidOperationException("Menu API returned an empty response.");
    }

    public async Task UpdateMenuAsync(int menuId, Menu menu)
    {
        var response = await _http.PutAsJsonAsync($"api/Menus/{menuId}", ToMenuUpdateDto(menu));
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMenuAsync(int menuId)
    {
        var response = await _http.DeleteAsync($"api/Menus/{menuId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<EventMenuSelectionDto>> GetMenuSelectionsAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);
        return await _http.GetFromJsonAsync<List<EventMenuSelectionDto>>($"api/Events/{eventId}/MenuSelections") ?? [];
    }

    public async Task UpdateMenuSelectionsAsync(int eventId, IEnumerable<int> menuIds)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PutAsJsonAsync(
            $"api/Events/{eventId}/MenuSelections",
            new EventMenuSelectionsUpdateDto { MenuIds = menuIds.Distinct().ToList() });
        response.EnsureSuccessStatusCode();
    }

    static MenuCreateDto ToMenuCreateDto(Menu menu) => new()
    {
        Name = menu.Name,
        Price = menu.Price,
        DietaryType = menu.DietaryType,
        Description = menu.Description,
        MenuItems = menu.MenuItems
            .OrderBy(item => item.DisplayOrder)
            .Select((item, index) => new MenuItemCreateDto
            {
                CourseName = item.CourseName,
                Name = item.Name,
                Description = item.Description,
                DisplayOrder = index
            })
            .ToList()
    };

    static MenuUpdateDto ToMenuUpdateDto(Menu menu) => new()
    {
        Name = menu.Name,
        Price = menu.Price,
        DietaryType = menu.DietaryType,
        Description = menu.Description,
        MenuItems = menu.MenuItems
            .OrderBy(item => item.DisplayOrder)
            .Select((item, index) => new MenuItemCreateDto
            {
                CourseName = item.CourseName,
                Name = item.Name,
                Description = item.Description,
                DisplayOrder = index
            })
            .ToList()
    };
}
