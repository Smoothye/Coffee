using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Guest;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<GuestDto>> GetGuestsAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);
        return await _http.GetFromJsonAsync<List<GuestDto>>($"api/Events/{eventId}/Guests") ?? [];
    }

    public async Task<GuestDto?> CreateGuestAsync(int eventId, GuestCreateDto guest)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PostAsJsonAsync($"api/Events/{eventId}/Guests", guest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GuestDto>();
    }

    public async Task<GuestBulkCreateResultDto?> CreateGuestsBulkAsync(int eventId, IEnumerable<GuestCreateDto> guests)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PostAsJsonAsync($"api/Events/{eventId}/Guests/bulk", guests);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GuestBulkCreateResultDto>();
    }

    public async Task UpdateGuestAsync(int eventId, int guestId, GuestUpdateDto guest)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PutAsJsonAsync($"api/Events/{eventId}/Guests/{guestId}", guest);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteGuestAsync(int eventId, int guestId)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.DeleteAsync($"api/Events/{eventId}/Guests/{guestId}");
        response.EnsureSuccessStatusCode();
    }

}
