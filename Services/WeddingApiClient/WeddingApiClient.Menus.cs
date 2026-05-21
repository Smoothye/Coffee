using WeddingPlannerApp.DTOs.Event;
using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Menu;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<MenuDto>> GetMenusAsync() =>
        await _http.GetFromJsonAsync<List<MenuDto>>("api/Menus") ?? [];

    public async Task<MenuDto> CreateMenuAsync(MenuCreateDto menu)
    {
        var response = await _http.PostAsJsonAsync("api/Menus", menu);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MenuDto>()
            ?? throw new InvalidOperationException("Menu API returned an empty response.");
    }

    public async Task UpdateMenuAsync(int menuId, MenuUpdateDto menu)
    {
        var response = await _http.PutAsJsonAsync($"api/Menus/{menuId}", menu);
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
}
