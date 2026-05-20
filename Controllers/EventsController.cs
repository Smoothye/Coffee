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
        var events = await context.Events
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

        return Ok(events);
    }

    // GET: api/Events/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var eventItem = await context.Events
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
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .SingleOrDefaultAsync();

        if (eventItem == null)
            return NotFound();

        return Ok(eventItem);
    }

    // POST: api/Events
    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] EventCreateDto model)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdText, out var userId))
            return Unauthorized();

        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == model.VenueId);
        if (!venueExists)
            return BadRequest($"Venue with id {model.VenueId} doesn't exist.");

        var userAlreadyHasEvent = await context.UserEvents.AnyAsync(ue => ue.UserId == userId);
        if (userAlreadyHasEvent)
            return BadRequest("This user already has an event.");

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

        context.UserEvents.Add(new UserEvent
        {
            UserId = userId,
            EventId = entity.EventId
        });
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        var result = ToDto(entity);
        return CreatedAtAction(nameof(GetById), new { id = result.EventId }, result);
    }

    // PUT: api/Events/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto model)
    {
        var eventItem = await context.Events
            .Include(e => e.Menus)
            .SingleOrDefaultAsync(e => e.EventId == id);

        if (eventItem == null)
            return NotFound($"Event with id: {id} was not found.");

        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == model.VenueId);
        if (!venueExists)
            return BadRequest($"Venue with id {model.VenueId} was not found.");

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

    // DELETE: api/Events/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventItem = await context.Events.FindAsync(id);
        if (eventItem == null)
            return NotFound();

        context.Events.Remove(eventItem);
        await context.SaveChangesAsync();

        return NoContent();
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
        Notes = entity.Notes,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
