using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.Models;
using WeddingPlannerApp.DTOs.Event;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
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
                MenuId = e.MenuId,
                Name = e.Name,
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
    
    // Get: api/Events/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var eventItem = await context.Events
            .Where(e => e.EventId == id)
            .Select(e => new EventDto
            {
                EventId = e.EventId,
                VenueId = e.VenueId,
                MenuId = e.MenuId,
                Name = e.Name,
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
        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == model.VenueId);
        if (!venueExists)
            return BadRequest($"Venue with id {model.VenueId} doesn't exist.");
        
        var menuExists = await context.Menus.AnyAsync(m => m.MenuId == model.MenuId);
        if (!menuExists)
            return BadRequest($"Menu with id {model.MenuId} was not found.");

        var entity = new Event
        {
            VenueId = model.VenueId,
            MenuId = model.MenuId,
            Name = model.Name,
            EventDate = model.EventDate,
            EstimatedGuests = model.EstimatedGuests,
            TotalBudget = model.TotalBudget,
            Notes = model.Notes
        };
        
        context.Events.Add(entity);
        await context.SaveChangesAsync();

        var result = new EventDto
        {
            EventId = entity.EventId,
            VenueId = entity.VenueId,
            MenuId = entity.MenuId,
            Name = entity.Name,
            EventDate = entity.EventDate,
            EstimatedGuests = entity.EstimatedGuests,
            TotalBudget = entity.TotalBudget,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
        
        return CreatedAtAction(nameof(GetById), new {id = result.EventId}, result);
    }
    
    // POST: api/Events/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto model)
    {
        var eventItem = await context.Events.FindAsync(id);
        if (eventItem == null)
            return NotFound($"Event with id: {id} was not found.");
        
        var venueExists = await context.Venues.AnyAsync(v => v.VenueId == model.VenueId);
        if (!venueExists)
            return BadRequest($"Venue with id {model.VenueId} was not found.");

        var menuExists = await context.Menus.AnyAsync(m => m.MenuId == model.MenuId);
        if (!menuExists)
            return BadRequest($"Menu with id {model.MenuId} was not found.");
        
        eventItem.VenueId = model.VenueId;
        eventItem.MenuId = model.MenuId;
        eventItem.Name = model.Name;
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
}