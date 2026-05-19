using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Event;
using WeddingPlannerApp.DTOs.Guest;
using WeddingPlannerApp.DTOs.Venue;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Services;

public sealed class WeddingApiClient(HttpClient http)
{
    public async Task<List<EventDto>> GetEventsAsync() =>
        await http.GetFromJsonAsync<List<EventDto>>("api/Events") ?? [];

    public async Task<EventDto?> GetEventAsync(int eventId) =>
        await http.GetFromJsonAsync<EventDto>($"api/Events/{eventId}");

    public async Task<EventDto?> CreateEventAsync(EventCreateDto eventItem)
    {
        var response = await http.PostAsJsonAsync("api/Events", eventItem);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventDto>();
    }

    public async Task UpdateEventAsync(int eventId, EventUpdateDto eventItem)
    {
        var response = await http.PutAsJsonAsync($"api/Events/{eventId}", eventItem);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(int eventId)
    {
        var response = await http.DeleteAsync($"api/Events/{eventId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Menu>> GetMenusAsync() =>
        await http.GetFromJsonAsync<List<Menu>>("api/Menus") ?? [];

    public async Task<List<VenueDto>> GetVenuesAsync() =>
        await http.GetFromJsonAsync<List<VenueDto>>("api/Venues") ?? [];

    public async Task<VenueDto?> GetVenueAsync(int venueId) =>
        await http.GetFromJsonAsync<VenueDto>($"api/Venues/{venueId}");

    public async Task<VenueDto?> CreateVenueAsync(VenueCreateDto venue)
    {
        var response = await http.PostAsJsonAsync("api/Venues", venue);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VenueDto>();
    }

    public async Task UpdateVenueAsync(int venueId, VenueUpdateDto venue)
    {
        var response = await http.PutAsJsonAsync($"api/Venues/{venueId}", venue);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteVenueAsync(int venueId)
    {
        var response = await http.DeleteAsync($"api/Venues/{venueId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<GuestDto>> GetGuestsAsync(int eventId) =>
        await http.GetFromJsonAsync<List<GuestDto>>($"api/Events/{eventId}/Guests") ?? [];

    public async Task<GuestDto?> CreateGuestAsync(int eventId, GuestCreateDto guest)
    {
        var response = await http.PostAsJsonAsync($"api/Events/{eventId}/Guests", ToGuestDto(eventId, guest));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GuestDto>();
    }

    public async Task<GuestBulkCreateResultDto?> CreateGuestsBulkAsync(int eventId, IEnumerable<GuestCreateDto> guests)
    {
        var response = await http.PostAsJsonAsync($"api/Events/{eventId}/Guests/bulk", guests);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GuestBulkCreateResultDto>();
    }

    public async Task UpdateGuestAsync(int eventId, int guestId, GuestUpdateDto guest)
    {
        var response = await http.PutAsJsonAsync($"api/Events/{eventId}/Guests/{guestId}", guest);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteGuestAsync(int eventId, int guestId)
    {
        var response = await http.DeleteAsync($"api/Events/{eventId}/Guests/{guestId}");
        response.EnsureSuccessStatusCode();
    }

    static GuestDto ToGuestDto(int eventId, GuestCreateDto guest) => new()
    {
        EventId = eventId,
        TableId = guest.TableId,
        FirstName = guest.FirstName,
        LastName = guest.LastName,
        Age = guest.Age,
        Email = guest.Email,
        Phone = guest.Phone,
        Gender = guest.Gender,
        RsvpStatus = guest.RsvpStatus,
        Group = guest.Group,
        DietaryRequirements = guest.DietaryRequirements,
        HasPlusOne = guest.HasPlusOne,
        SeatNumber = guest.SeatNumber,
        Notes = guest.Notes,
    };
}
