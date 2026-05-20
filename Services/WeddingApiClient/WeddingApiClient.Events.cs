using System.Net;
using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Event;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<EventDto>> GetEventsAsync()
    {
        var response = await _http.GetAsync("api/Events");

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return [];

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? [];
    }

    public async Task<EventDto?> GetEventAsync(int eventId)
    {
        var response = await _http.GetAsync($"api/Events/{eventId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventDto>();
    }

    public async Task<EventDto?> CreateEventAsync(EventCreateDto eventItem)
    {
        var response = await _http.PostAsJsonAsync("api/Events", eventItem);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventDto>();
    }

    public async Task UpdateEventAsync(int eventId, EventUpdateDto eventItem)
    {
        var response = await _http.PutAsJsonAsync($"api/Events/{eventId}", eventItem);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(int eventId)
    {
        var response = await _http.DeleteAsync($"api/Events/{eventId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        response.EnsureSuccessStatusCode();
    }
}
