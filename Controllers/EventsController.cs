using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Event;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/Events
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
    {
        var query = context.Events.AsQueryable();

        if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            query = query.Where(e => e.UserEvents.Any(ue => ue.UserId == userId));
        }

        var events = await query
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
                OwnerName = e.UserEvents
                    .Select(ue => ((ue.User!.FirstName ?? "") + " " + (ue.User.LastName ?? "")).Trim())
                    .FirstOrDefault(),
                OwnerEmail = e.UserEvents
                    .Select(ue => ue.User!.Email)
                    .FirstOrDefault(),
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/Events/BookedVenueIds?eventDate=2026-06-01
    [HttpGet("BookedVenueIds")]
    public async Task<ActionResult<IEnumerable<int>>> GetBookedVenueIds(
        [FromQuery] DateTime eventDate,
        [FromQuery] int? exceptEventId = null
    )
    {
        var date = eventDate.Date;
        var nextDate = date.AddDays(1);

        var bookedVenueIds = await context.Events
            .Where(e => e.EventDate >= date && e.EventDate < nextDate)
            .Where(e => !exceptEventId.HasValue || e.EventId != exceptEventId.Value)
            .Select(e => e.VenueId)
            .Distinct()
            .ToListAsync();

        return Ok(bookedVenueIds);
    }

    // GET: api/Events/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var query = context.Events.Where(e => e.EventId == id);

        if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized();

            query = query.Where(e => e.UserEvents.Any(ue => ue.UserId == userId));
        }

        var eventItem = await query
            .Where(e => e.EventId == id)
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
                OwnerName = e.UserEvents
                    .Select(ue => ((ue.User!.FirstName ?? "") + " " + (ue.User.LastName ?? "")).Trim())
                    .FirstOrDefault(),
                OwnerEmail = e.UserEvents
                    .Select(ue => ue.User!.Email)
                    .FirstOrDefault(),
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .SingleOrDefaultAsync();

        if (eventItem == null)
            return NotFound();

        return Ok(eventItem);
    }
    
    // GET: api/Events/{eventId}/MenuSelections
    // returns a list of menu selections for the event
    [HttpGet("{eventId:int}/MenuSelections")]
    public async Task<ActionResult<IEnumerable<EventMenuSelectionDto>>> GetMenuSelections(int eventId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var menuSelections = await context.Events
            .Where(e => e.EventId == eventId)
            .SelectMany(e => e.Menus)
            .Select(m => new EventMenuSelectionDto
            {
                MenuId = m.MenuId,
                Name = m.Name,
                Price = m.Price,
                DietaryType = m.DietaryType,
                Description = m.Description,
                Items = m.MenuItems
                    .OrderBy(mi => mi.DisplayOrder)
                    .Select(mi => new EventMenuSelectionItemDto
                    {
                        MenuItemId = mi.MenuItemId,
                        CourseName = mi.CourseName,
                        Name = mi.Name,
                        Description = mi.Description,
                        DisplayOrder = mi.DisplayOrder
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(menuSelections);
    }
    
    // PUT: api/Events/{eventId}/MenuSelections
    // updates the menu selections for the event
    [HttpPut("{eventId:int}/MenuSelections")]
    public async Task<IActionResult> UpdateMenuSelections(int eventId, [FromBody] EventMenuSelectionsUpdateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var eventItem = await context.Events
            .Include(e => e.Menus)
            .SingleOrDefaultAsync(e => e.EventId == eventId);

        if (eventItem == null)
            return NotFound($"Event with id: {eventId} was not found.");

        var menuIds = model.MenuIds.Distinct().ToList();

        var menus = await context.Menus
            .Where(m => menuIds.Contains(m.MenuId))
            .ToListAsync();

        if (menus.Count != menuIds.Count)
            return BadRequest("One or more menu ids do not exist.");

        eventItem.Menus.Clear();

        foreach (var menu in menus)
        {
            eventItem.Menus.Add(menu);
        }

        eventItem.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return NoContent();
    }
    

    // POST: api/Events
    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] EventCreateDto model)
    {
        var hasCurrentUser = TryGetCurrentUserId(out var userId);
        if (User.Identity?.IsAuthenticated == true && !hasCurrentUser)
            return Unauthorized();

        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == model.VenueId);
        if (!venueExists)
            return BadRequest($"Venue with id {model.VenueId} doesn't exist.");

        if (hasCurrentUser)
        {
            var userAlreadyHasEvent = await context.UserEvents.AnyAsync(ue => ue.UserId == userId);
            if (userAlreadyHasEvent)
                return BadRequest("This user already has an event.");
        }
        
        // check if an event already exists for this date and venue
        var eventDate = model.EventDate.Date;
        var nextEventDate = eventDate.AddDays(1);
        var eventExists = await context.Events.AnyAsync(e =>
            e.EventDate >= eventDate &&
            e.EventDate < nextEventDate &&
            e.VenueId == model.VenueId);
        if (eventExists)
            return BadRequest("An event already exists for this date and venue.");       
        
        var entity = new Event
        {
            VenueId = model.VenueId,
            Name = model.Name,
            BrideName = model.BrideName,
            GroomName = model.GroomName,
            EventDate = model.EventDate,
            EstimatedGuests = model.EstimatedGuests,
            TotalBudget = model.TotalBudget,
            Notes = model.Notes
        };

        var menuIds = model.MenuIds?.Distinct().ToList() ?? [];
        if (menuIds.Count > 0)
        {
            var menus = await context.Menus
                .Where(m => menuIds.Contains(m.MenuId))
                .ToListAsync();

            if (menus.Count != menuIds.Count)
                return BadRequest("One or more menu ids do not exist.");

            entity.Menus = menus;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        context.Events.Add(entity);
        await context.SaveChangesAsync();

        if (hasCurrentUser)
        {
            context.UserEvents.Add(new UserEvent
            {
                UserId = userId,
                EventId = entity.EventId
            });
            await context.SaveChangesAsync();
        }

        await transaction.CommitAsync();

        var result = ToDto(entity);
        return CreatedAtAction(nameof(GetById), new { id = result.EventId }, result);
    }

    // PUT: api/Events/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto model)
    {
        if (!await CanAccessEventAsync(id))
            return NotFound($"Event with id: {id} was not found.");

        var eventItem = await context.Events
            .Include(e => e.Menus)
            .SingleOrDefaultAsync(e => e.EventId == id);

        if (eventItem == null)
            return NotFound($"Event with id: {id} was not found.");

        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == model.VenueId);
        if (!venueExists)
            return BadRequest($"Venue with id {model.VenueId} was not found.");

        var eventDate = model.EventDate.Date;
        var nextEventDate = eventDate.AddDays(1);
        var eventExists = await context.Events.AnyAsync(e =>
            e.EventId != id &&
            e.EventDate >= eventDate &&
            e.EventDate < nextEventDate &&
            e.VenueId == model.VenueId);
        if (eventExists)
            return BadRequest("An event already exists for this date and venue.");

        eventItem.VenueId = model.VenueId;
        eventItem.Menus.Clear();

        var menuIds = model.MenuIds?.Distinct().ToList() ?? [];
        if (menuIds.Count > 0)
        {
            var menus = await context.Menus
                .Where(m => menuIds.Contains(m.MenuId))
                .ToListAsync();

            if (menus.Count != menuIds.Count)
                return BadRequest("One or more menu ids do not exist.");

            foreach (var menu in menus)
            {
                eventItem.Menus.Add(menu);
            }
        }

        eventItem.Name = model.Name;
        eventItem.BrideName = model.BrideName;
        eventItem.GroomName = model.GroomName;
        eventItem.EventDate = model.EventDate;
        eventItem.EstimatedGuests = model.EstimatedGuests;
        eventItem.TotalBudget = model.TotalBudget;
        eventItem.Notes = model.Notes;
        eventItem.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // PUT: api/Events/{id}/Budget
    [HttpPut("{id:int}/Budget")]
    public async Task<IActionResult> UpdateBudget(int id, [FromBody] EventBudgetUpdateDto model)
    {
        if (!await CanAccessEventAsync(id))
            return NotFound($"Event with id: {id} was not found.");

        var eventItem = await context.Events.SingleOrDefaultAsync(e => e.EventId == id);
        if (eventItem == null)
            return NotFound($"Event with id: {id} was not found.");

        eventItem.TotalBudget = model.TotalBudget;
        eventItem.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Events/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await CanAccessEventAsync(id))
            return NotFound();

        var eventItem = await context.Events.FindAsync(id);
        if (eventItem == null)
            return NotFound();

        context.Events.Remove(eventItem);
        await context.SaveChangesAsync();

        return NoContent();
    }

    bool TryGetCurrentUserId(out int userId)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out userId);
    }

    async Task<bool> CanAccessEventAsync(int eventId)
    {
        if (User.Identity?.IsAuthenticated != true)
            return await context.Events.AnyAsync(e => e.EventId == eventId);

        if (User.IsInRole("Admin"))
            return await context.Events.AnyAsync(e => e.EventId == eventId);

        return TryGetCurrentUserId(out var userId) &&
               await context.UserEvents.AnyAsync(ue => ue.EventId == eventId && ue.UserId == userId);
    }

    static EventDto ToDto(Event entity) => new()
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
        OwnerName = entity.UserEvents
            .Select(ue => ((ue.User?.FirstName ?? "") + " " + (ue.User?.LastName ?? "")).Trim())
            .FirstOrDefault(),
        OwnerEmail = entity.UserEvents
            .Select(ue => ue.User?.Email)
            .FirstOrDefault(),
        Notes = entity.Notes,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
