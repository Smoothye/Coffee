using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Event;
using WeddingPlannerApp.DTOs.Guest;
using WeddingPlannerApp.DTOs.Venue;
using WeddingPlannerApp.Models;
using EventEntity = WeddingPlannerApp.Models.Event;

namespace WeddingPlannerApp.Services;

public sealed class WeddingApiClient(
    HttpClient http,
    ApplicationDbContext context,
    AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<List<EventDto>> GetEventsAsync()
    {
        var (userId, isAdmin) = await GetCurrentUserScopeAsync();
        var query = AccessibleEventsQuery(userId, isAdmin);

        return await query
            .Select(e => new EventDto
            {
                EventId = e.EventId,
                VenueId = e.VenueId,
                MenuIds = e.Menus.Select(m => m.MenuId).ToList(),
                Name = e.Name,
                BrideName = e.BrideName,
                GroomName = e.GroomName,
                EventDate = e.EventDate,
                EstimatedGuests = e.EstimatedGuests,
                TotalBudget = e.TotalBudget,
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<EventDto?> GetEventAsync(int eventId)
    {
        var (userId, isAdmin) = await GetCurrentUserScopeAsync();

        return await AccessibleEventsQuery(userId, isAdmin)
            .Where(e => e.EventId == eventId)
            .Select(e => new EventDto
            {
                EventId = e.EventId,
                VenueId = e.VenueId,
                MenuIds = e.Menus.Select(m => m.MenuId).ToList(),
                Name = e.Name,
                BrideName = e.BrideName,
                GroomName = e.GroomName,
                EventDate = e.EventDate,
                EstimatedGuests = e.EstimatedGuests,
                TotalBudget = e.TotalBudget,
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .SingleOrDefaultAsync();
    }

    public async Task<EventDto?> CreateEventAsync(EventCreateDto eventItem)
    {
        var userId = await GetCurrentUserIdAsync();

        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == eventItem.VenueId);
        if (!venueExists)
            throw new InvalidOperationException($"Venue with id {eventItem.VenueId} doesn't exist.");

        var userAlreadyHasEvent = await context.UserEvents.AnyAsync(ue => ue.UserId == userId);
        if (userAlreadyHasEvent)
            throw new InvalidOperationException("This user already has an event.");

        var entity = new EventEntity
        {
            VenueId = eventItem.VenueId,
            Name = eventItem.Name,
            BrideName = eventItem.BrideName,
            GroomName = eventItem.GroomName,
            EventDate = eventItem.EventDate,
            EstimatedGuests = eventItem.EstimatedGuests,
            TotalBudget = eventItem.TotalBudget,
            Notes = eventItem.Notes
        };

        var menuIds = eventItem.MenuIds?.Distinct().ToList() ?? [];
        if (menuIds.Count > 0)
        {
            var menus = await context.Menus
                .Where(m => menuIds.Contains(m.MenuId))
                .ToListAsync();

            if (menus.Count != menuIds.Count)
                throw new InvalidOperationException("One or more menu ids do not exist.");

            entity.Menus = menus;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        context.Events.Add(entity);
        await context.SaveChangesAsync();

        context.UserEvents.Add(new UserEvent
        {
            UserId = userId,
            EventId = entity.EventId
        });
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        return ToEventDto(entity);
    }

    public async Task UpdateEventAsync(int eventId, EventUpdateDto eventItem)
    {
        await EnsureCanAccessEventAsync(eventId);

        var entity = await context.Events
            .Include(e => e.Menus)
            .SingleOrDefaultAsync(e => e.EventId == eventId);

        if (entity is null)
            throw new InvalidOperationException($"Event with id {eventId} was not found.");

        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == eventItem.VenueId);
        if (!venueExists)
            throw new InvalidOperationException($"Venue with id {eventItem.VenueId} was not found.");

        entity.VenueId = eventItem.VenueId;
        entity.Menus.Clear();

        var menuIds = eventItem.MenuIds?.Distinct().ToList() ?? [];
        if (menuIds.Count > 0)
        {
            var menus = await context.Menus
                .Where(m => menuIds.Contains(m.MenuId))
                .ToListAsync();

            if (menus.Count != menuIds.Count)
                throw new InvalidOperationException("One or more menu ids do not exist.");

            foreach (var menu in menus)
            {
                entity.Menus.Add(menu);
            }
        }

        entity.Name = eventItem.Name;
        entity.BrideName = eventItem.BrideName;
        entity.GroomName = eventItem.GroomName;
        entity.EventDate = eventItem.EventDate;
        entity.EstimatedGuests = eventItem.EstimatedGuests;
        entity.TotalBudget = eventItem.TotalBudget;
        entity.Notes = eventItem.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);

        var entity = await context.Events.FindAsync(eventId);
        if (entity is null)
            return;

        context.Events.Remove(entity);
        await context.SaveChangesAsync();
    }

    public async Task<List<Menu>> GetMenusAsync() =>
        await context.Menus
            .Include(menu => menu.MenuItems)
            .AsNoTracking()
            .OrderBy(menu => menu.DietaryType)
            .ThenBy(menu => menu.Name)
            .ToListAsync();

    public async Task<Menu> CreateMenuAsync(Menu menu)
    {
        context.Menus.Add(menu);
        await context.SaveChangesAsync();
        return menu;
    }

    public async Task DeleteMenuAsync(int menuId)
    {
        var menu = await context.Menus.FindAsync(menuId);
        if (menu is null)
            return;

        context.Menus.Remove(menu);
        await context.SaveChangesAsync();
    }

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

    public async Task<List<GuestDto>> GetGuestsAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);
        return await http.GetFromJsonAsync<List<GuestDto>>($"api/Events/{eventId}/Guests") ?? [];
    }

    public async Task<GuestDto?> CreateGuestAsync(int eventId, GuestCreateDto guest)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await http.PostAsJsonAsync($"api/Events/{eventId}/Guests", ToGuestDto(eventId, guest));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GuestDto>();
    }

    public async Task<GuestBulkCreateResultDto?> CreateGuestsBulkAsync(int eventId, IEnumerable<GuestCreateDto> guests)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await http.PostAsJsonAsync($"api/Events/{eventId}/Guests/bulk", guests);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GuestBulkCreateResultDto>();
    }

    public async Task UpdateGuestAsync(int eventId, int guestId, GuestUpdateDto guest)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await http.PutAsJsonAsync($"api/Events/{eventId}/Guests/{guestId}", guest);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteGuestAsync(int eventId, int guestId)
    {
        await EnsureCanAccessEventAsync(eventId);
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

    async Task<int> GetCurrentUserIdAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdText, out var userId))
            throw new InvalidOperationException("You need to be signed in to create an event.");

        return userId;
    }

    async Task<(int UserId, bool IsAdmin)> GetCurrentUserScopeAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdText, out var userId))
            throw new InvalidOperationException("You need to be signed in to view wedding information.");

        return (userId, authState.User.IsInRole("Admin"));
    }

    IQueryable<EventEntity> AccessibleEventsQuery(int userId, bool isAdmin)
    {
        var query = context.Events.AsQueryable();
        if (isAdmin)
            return query;

        return query.Where(e => context.UserEvents.Any(ue => ue.UserId == userId && ue.EventId == e.EventId));
    }

    async Task EnsureCanAccessEventAsync(int eventId)
    {
        var (userId, isAdmin) = await GetCurrentUserScopeAsync();
        var canAccess = await AccessibleEventsQuery(userId, isAdmin).AnyAsync(e => e.EventId == eventId);

        if (!canAccess)
            throw new InvalidOperationException("You can only access your own wedding information.");
    }

    static EventDto ToEventDto(EventEntity entity) => new()
    {
        EventId = entity.EventId,
        VenueId = entity.VenueId,
        MenuIds = entity.Menus.Select(m => m.MenuId).ToList(),
        Name = entity.Name,
        BrideName = entity.BrideName,
        GroomName = entity.GroomName,
        EventDate = entity.EventDate,
        EstimatedGuests = entity.EstimatedGuests,
        TotalBudget = entity.TotalBudget,
        Notes = entity.Notes,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
