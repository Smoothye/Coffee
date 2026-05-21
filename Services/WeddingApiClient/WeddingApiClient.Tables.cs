using System.Net;
using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Table;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<WeddingTableDto>> GetTablesAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);
        return await _http.GetFromJsonAsync<List<WeddingTableDto>>($"api/Events/{eventId}/Tables") ?? [];
    }

    public async Task<WeddingTableDto?> CreateTableAsync(int eventId, WeddingTableCreateDto table)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PostAsJsonAsync($"api/Events/{eventId}/Tables", table);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WeddingTableDto>();
    }

    public async Task UpdateTableAsync(int eventId, int tableId, WeddingTableUpdateDto table)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PutAsJsonAsync($"api/Events/{eventId}/Tables/{tableId}", table);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTableAsync(int eventId, int tableId)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.DeleteAsync($"api/Events/{eventId}/Tables/{tableId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        response.EnsureSuccessStatusCode();
    }
}
