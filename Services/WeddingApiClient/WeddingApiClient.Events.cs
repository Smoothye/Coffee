using System.Net;
using System.Globalization;
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

    public async Task<HashSet<int>> GetBookedVenueIdsAsync(DateTime eventDate, int? exceptEventId = null)
    {
        var date = Uri.EscapeDataString(eventDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var url = $"api/Events/BookedVenueIds?eventDate={date}";
        if (exceptEventId.HasValue)
            url += $"&exceptEventId={exceptEventId.Value}";

        var response = await _http.GetAsync(url);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return [];

        response.EnsureSuccessStatusCode();
        var ids = await response.Content.ReadFromJsonAsync<List<int>>() ?? [];
        return ids.ToHashSet();
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

    public async Task UpdateEventBudgetAsync(int eventId, decimal totalBudget)
    {
        var response = await _http.PutAsJsonAsync($"api/Events/{eventId}/Budget", new EventBudgetUpdateDto
        {
            TotalBudget = totalBudget
        });
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
