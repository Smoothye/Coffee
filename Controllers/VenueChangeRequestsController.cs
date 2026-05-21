using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Venue;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class VenueChangeRequestsController(ApplicationDbContext context) : ControllerBase
{
    const string RequestPrefix = "[[venue-change-request:";
    const string ApprovedPrefix = "[[venue-change-approved:";
    const string RequestSuffix = "]]";

    // GET: api/VenueChangeRequests
    [HttpGet("VenueChangeRequests")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<VenueChangeRequestDto>>> GetAll()
    {
        var requests = await context.Events
            .Include(e => e.Venue)
            .Include(e => e.UserEvents).ThenInclude(ue => ue.User)
            .Include(e => e.Guests)
            .Where(e => e.Notes != null &&
                        (e.Notes.Contains(RequestPrefix) || e.Notes.Contains(ApprovedPrefix)))
            .ToListAsync();

        var venueIds = requests
            .SelectMany(e => ReadVenueIds(e))
            .Distinct()
            .ToList();

        var venues = await context.Venues
            .Where(v => venueIds.Contains(v.VenueId))
            .ToDictionaryAsync(v => v.VenueId);

        var result = requests
            .Select(e =>
            {
                var pendingVenueId = ReadRequestedVenueId(e.Notes);
                var approvedVenueIds = ReadApprovedVenueIds(e.Notes);
                var isPending = pendingVenueId.HasValue;
                var currentVenueId = isPending ? e.VenueId : approvedVenueIds?.CurrentVenueId;
                var requestedVenueId = isPending ? pendingVenueId : approvedVenueIds?.RequestedVenueId;

                if (!currentVenueId.HasValue ||
                    !requestedVenueId.HasValue ||
                    !venues.TryGetValue(requestedVenueId.Value, out var requestedVenue))
                    return null;

                var owner = e.UserEvents.Select(ue => ue.User).FirstOrDefault();
                var ownerName = owner == null ? "" : $"{owner.FirstName} {owner.LastName}".Trim();
                var actualGuests = e.Guests.Count(g => g.RsvpStatus != RsvpStatus.Declined);

                return new VenueChangeRequestDto
                {
                    EventId = e.EventId,
                    EventName = e.Name,
                    EventDate = e.EventDate,
                    Status = isPending ? "Pending" : "Accepted",
                    EstimatedGuests = e.EstimatedGuests,
                    ActualGuests = actualGuests,
                    OwnerName = ownerName,
                    OwnerEmail = owner?.Email,
                    CurrentVenueId = currentVenueId.Value,
                    CurrentVenueName = venues.TryGetValue(currentVenueId.Value, out var currentVenue)
                        ? currentVenue.Name
                        : e.Venue?.Name ?? $"Venue #{currentVenueId.Value}",
                    RequestedVenueId = requestedVenue.VenueId,
                    RequestedVenueName = requestedVenue.Name,
                    TotalBudget = e.TotalBudget,
                    RequestedVenuePrice = requestedVenue.EstimatedPrice,
                    FitsCapacity = FitsCapacity(requestedVenue, Math.Max(e.EstimatedGuests, actualGuests)),
                    FitsBudget = FitsBudget(e, requestedVenue)
                };
            })
            .Where(request => request != null)
            .ToList();

        return Ok(result);
    }

    // POST: api/Events/{eventId}/VenueChangeRequest
    [HttpPost("Events/{eventId:int}/VenueChangeRequest")]
    public async Task<IActionResult> Create(int eventId, [FromBody] VenueChangeRequestCreateDto model)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdText, out var userId))
            return Unauthorized();

        var eventItem = await context.Events
            .Include(e => e.UserEvents)
            .Include(e => e.Guests)
            .SingleOrDefaultAsync(e => e.EventId == eventId);

        if (eventItem == null)
            return NotFound($"Event with id: {eventId} was not found.");

        if (!User.IsInRole("Admin") && eventItem.UserEvents.All(ue => ue.UserId != userId))
            return Forbid();

        if (eventItem.VenueId == model.RequestedVenueId)
            return BadRequest("This venue is already selected for the event.");

        var venue = await context.Venues.FindAsync(model.RequestedVenueId);
        if (venue == null)
            return BadRequest($"Venue with id {model.RequestedVenueId} was not found.");

        var actualGuests = eventItem.Guests.Count(g => g.RsvpStatus != RsvpStatus.Declined);
        var guestsToFit = Math.Max(eventItem.EstimatedGuests, actualGuests);
        if (!FitsCapacity(venue, guestsToFit))
            return BadRequest($"This venue does not fit {guestsToFit} guests.");
        if (!FitsBudget(eventItem, venue))
            return BadRequest("This venue is above the event budget.");

        eventItem.Notes = WriteRequestedVenueId(eventItem.Notes, model.RequestedVenueId);
        eventItem.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // PUT: api/VenueChangeRequests/{eventId}/Approve
    [HttpPut("VenueChangeRequests/{eventId:int}/Approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int eventId)
    {
        var eventItem = await context.Events
            .Include(e => e.Guests)
            .SingleOrDefaultAsync(e => e.EventId == eventId);
        if (eventItem == null)
            return NotFound($"Event with id: {eventId} was not found.");

        var requestedVenueId = ReadRequestedVenueId(eventItem.Notes);
        if (!requestedVenueId.HasValue)
            return NotFound("No pending venue change request was found.");

        var venue = await context.Venues.SingleOrDefaultAsync(v => v.VenueId == requestedVenueId.Value);
        if (venue == null)
            return BadRequest($"Venue with id {requestedVenueId.Value} was not found.");

        var actualGuests = eventItem.Guests.Count(g => g.RsvpStatus != RsvpStatus.Declined);
        var guestsToFit = Math.Max(eventItem.EstimatedGuests, actualGuests);
        if (!FitsCapacity(venue, guestsToFit))
            return BadRequest($"This venue does not fit {guestsToFit} guests.");
        if (!FitsBudget(eventItem, venue))
            return BadRequest("This venue is above the event budget.");

        var currentVenueId = eventItem.VenueId;
        eventItem.VenueId = requestedVenueId.Value;
        eventItem.Notes = WriteApprovedVenueChange(eventItem.Notes, currentVenueId, requestedVenueId.Value);
        eventItem.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/VenueChangeRequests/{eventId}
    [HttpDelete("VenueChangeRequests/{eventId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int eventId)
    {
        var eventItem = await context.Events.SingleOrDefaultAsync(e => e.EventId == eventId);
        if (eventItem == null)
            return NotFound($"Event with id: {eventId} was not found.");

        eventItem.Notes = RemoveVenueRequest(eventItem.Notes);
        eventItem.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return NoContent();
    }

    static bool FitsCapacity(Venue venue, int guests) =>
        guests <= venue.MaxCapacity;

    static bool FitsBudget(Event eventItem, Venue venue) =>
        eventItem.TotalBudget <= 0 || venue.EstimatedPrice <= eventItem.TotalBudget;

    static int? ReadRequestedVenueId(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        var start = notes.IndexOf(RequestPrefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return null;

        start += RequestPrefix.Length;
        var end = notes.IndexOf(RequestSuffix, start, StringComparison.OrdinalIgnoreCase);
        if (end < 0)
            return null;

        return int.TryParse(notes[start..end], out var venueId) ? venueId : null;
    }

    static string WriteRequestedVenueId(string? notes, int venueId)
    {
        var cleaned = RemoveVenueRequest(notes);
        return $"{cleaned} {RequestPrefix}{venueId}{RequestSuffix}".Trim();
    }

    static string WriteApprovedVenueChange(string? notes, int currentVenueId, int requestedVenueId)
    {
        var cleaned = RemoveApprovedVenueChange(RemoveVenueRequest(notes));
        return $"{cleaned} {ApprovedPrefix}{currentVenueId}:{requestedVenueId}{RequestSuffix}".Trim();
    }

    static IEnumerable<int> ReadVenueIds(Event eventItem)
    {
        var pendingVenueId = ReadRequestedVenueId(eventItem.Notes);
        if (pendingVenueId.HasValue)
        {
            yield return eventItem.VenueId;
            yield return pendingVenueId.Value;
            yield break;
        }

        var approvedVenueIds = ReadApprovedVenueIds(eventItem.Notes);
        if (approvedVenueIds is null)
            yield break;

        yield return approvedVenueIds.Value.CurrentVenueId;
        yield return approvedVenueIds.Value.RequestedVenueId;
    }

    static (int CurrentVenueId, int RequestedVenueId)? ReadApprovedVenueIds(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        var start = notes.IndexOf(ApprovedPrefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return null;

        start += ApprovedPrefix.Length;
        var end = notes.IndexOf(RequestSuffix, start, StringComparison.OrdinalIgnoreCase);
        if (end < 0)
            return null;

        var parts = notes[start..end].Split(':', 2);
        if (parts.Length != 2)
            return null;

        return int.TryParse(parts[0], out var currentVenueId) &&
               int.TryParse(parts[1], out var requestedVenueId)
            ? (currentVenueId, requestedVenueId)
            : null;
    }

    public static string RemoveVenueRequest(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return "";

        var start = notes.IndexOf(RequestPrefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return notes.Trim();

        var end = notes.IndexOf(RequestSuffix, start, StringComparison.OrdinalIgnoreCase);
        if (end < 0)
            return notes[..start].Trim();

        return (notes[..start] + notes[(end + RequestSuffix.Length)..]).Trim();
    }

    static string RemoveApprovedVenueChange(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return "";

        var start = notes.IndexOf(ApprovedPrefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return notes.Trim();

        var end = notes.IndexOf(RequestSuffix, start, StringComparison.OrdinalIgnoreCase);
        if (end < 0)
            return notes[..start].Trim();

        return (notes[..start] + notes[(end + RequestSuffix.Length)..]).Trim();
    }
}
