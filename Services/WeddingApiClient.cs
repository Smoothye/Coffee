using System.Net;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient(HttpClient http)
{
    readonly HttpClient _http = http;

    async Task EnsureCanAccessEventAsync(int eventId)
    {
        var response = await _http.GetAsync($"api/Events/{eventId}");

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new InvalidOperationException("You need to be signed in to view wedding information.");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException("You can only access your own wedding information.");

        response.EnsureSuccessStatusCode();
    }
}
