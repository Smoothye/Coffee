using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Venue;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<VenueDto>> GetVenuesAsync() =>
        await _http.GetFromJsonAsync<List<VenueDto>>("api/Venues") ?? [];

    public async Task<VenueDto?> GetVenueAsync(int venueId) =>
        await _http.GetFromJsonAsync<VenueDto>($"api/Venues/{venueId}");

    public async Task<VenueDto?> CreateVenueAsync(VenueCreateDto venue)
    {
        var response = await _http.PostAsJsonAsync("api/Venues", venue);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VenueDto>();
    }

    public async Task UpdateVenueAsync(int venueId, VenueUpdateDto venue)
    {
        var response = await _http.PutAsJsonAsync($"api/Venues/{venueId}", venue);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteVenueAsync(int venueId)
    {
        var response = await _http.DeleteAsync($"api/Venues/{venueId}");
        response.EnsureSuccessStatusCode();
    }
}
