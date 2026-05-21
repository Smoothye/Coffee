using System.Net;
using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Venue;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<VenueChangeRequestDto>> GetVenueChangeRequestsAsync()
    {
        var response = await _http.GetAsync("api/VenueChangeRequests");
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return [];

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<VenueChangeRequestDto>>() ?? [];
    }

    public async Task CreateVenueChangeRequestAsync(int eventId, int requestedVenueId)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/Events/{eventId}/VenueChangeRequest",
            new VenueChangeRequestCreateDto { RequestedVenueId = requestedVenueId });
        response.EnsureSuccessStatusCode();
    }

    public async Task ApproveVenueChangeRequestAsync(int eventId)
    {
        var response = await _http.PutAsync($"api/VenueChangeRequests/{eventId}/Approve", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RejectVenueChangeRequestAsync(int eventId)
    {
        var response = await _http.DeleteAsync($"api/VenueChangeRequests/{eventId}");
        response.EnsureSuccessStatusCode();
    }
}
