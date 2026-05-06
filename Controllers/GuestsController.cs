using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.Models;
using WeddingPlannerApp.DTOs.Guest;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/Events/{eventId:int}/[controller]")]
public class GuestsController(ApplicationDbContext context) : ControllerBase
{
    
    // GET: api/Events/{eventId}/Guests
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestDto>>> GetAll(int eventId)
    {
        var eventExists = await context.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
            return BadRequest($"Event with id {eventId} was not found.");       
        
        var guestItems = await context.Guests
            .Where(g => g.EventId == eventId)
            .Select(g => new GuestDto
            {
                GuestId = g.GuestId,
                Age = g.Age,
                TableId = g.TableId,
                FirstName = g.FirstName,
                LastName = g.LastName,
                Email = g.Email,
                Phone = g.Phone,
                Gender = g.Gender,
                RsvpStatus = g.RsvpStatus,
                Group = g.Group,
                DietaryRequirements = g.DietaryRequirements,
                Notes = g.Notes,
            })
            .ToListAsync();
        
        return Ok(guestItems);
    }
    
    // GET: api/Events/{eventId}/Guests/{guestId}
    [HttpGet("{guestId:int}")]
    public async Task<ActionResult<GuestDto>> GetById(int eventId, int guestId)
    {
        var eventExists = await context.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
            return BadRequest($"Event with id {eventId} was not found.");
        
        var guestItem = await context.Guests
            .Where(g => g.EventId == eventId && g.GuestId == guestId)
            .Select(g => new GuestDto
            {
                GuestId = g.GuestId,
                Age = g.Age,
                TableId = g.TableId,
                FirstName = g.FirstName,
                LastName = g.LastName,
                Email = g.Email,
                Phone = g.Phone,
                Gender = g.Gender,
                RsvpStatus = g.RsvpStatus,
                Group = g.Group,
                DietaryRequirements = g.DietaryRequirements,
                Notes = g.Notes,
            })
            .SingleOrDefaultAsync();
        
        return Ok(guestItem);
    }
    
    // POST: api/Events/{eventId}/Guests
    [HttpPost]
    public async Task<ActionResult<GuestDto>> Create([FromBody] GuestDto model, int eventId)
    {
        var eventExists = await context.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
            return BadRequest($"Event with id {model.EventId} doesn't exist.");

        var entity = new Guest
        {
            EventId = eventId,
            TableId = model.TableId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Age = model.Age,
            Email = model.Email,
            Phone = model.Phone,
            Gender = model.Gender,
            RsvpStatus = model.RsvpStatus,
            Group = model.Group,
            DietaryRequirements = model.DietaryRequirements,
            Notes = model.Notes,
        };
        
        context.Guests.Add(entity);
        await context.SaveChangesAsync();

        var result = new GuestDto
        {
            GuestId = entity.GuestId,
            EventId = entity.EventId,
            TableId = entity.TableId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Age = entity.Age,
            Email = entity.Email,
            Phone = entity.Phone,
            Gender = entity.Gender,
            RsvpStatus = entity.RsvpStatus,
            Group = entity.Group,
            DietaryRequirements = entity.DietaryRequirements,
            Notes = entity.Notes
        };
        
        return CreatedAtAction(nameof(GetById), new {id = result.EventId}, result);
    }
    
    // POST api/Events/{eventId}/Guests/bulk
    [HttpPost("bulk")]
    public async Task<ActionResult<GuestBulkCreateResultDto>> BulkCreate(
        int eventId,
        [FromBody] IEnumerable<GuestCreateDto> models
    )
    {
        var eventExists = await context.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
            return NotFound($"Event with id {eventId} was not found.");

        var guestModels = models.ToList();

        if (guestModels.Count == 0)
            return BadRequest("At least one guest is required.");

        var guests = guestModels.Select(model => new Guest
        {
            EventId = eventId,
            TableId = model.TableId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Age = model.Age,
            Email = model.Email,
            Phone = model.Phone,
            Gender = model.Gender,
            RsvpStatus = model.RsvpStatus,
            Group = model.Group,
            DietaryRequirements = model.DietaryRequirements,
            Notes = model.Notes
        }).ToList();

        await context.Guests.AddRangeAsync(guests);
        await context.SaveChangesAsync();

        var result = new GuestBulkCreateResultDto
        {
            CreatedCount = guests.Count,
            Guests = guests.Select(g => new GuestDto
            {
                GuestId = g.GuestId,
                EventId = g.EventId,
                TableId = g.TableId,
                FirstName = g.FirstName,
                LastName = g.LastName,
                Age = g.Age,
                Email = g.Email,
                Phone = g.Phone,
                Gender = g.Gender,
                RsvpStatus = g.RsvpStatus,
                Group = g.Group,
                DietaryRequirements = g.DietaryRequirements,
                Notes = g.Notes
            })
        };

        return CreatedAtAction(nameof(GetAll), new { eventId }, result);
    }
    
    // POST: api/Events/{eventId}/Guests/{guestId}
    [HttpPut("{guestId:int}")]
    public async Task<IActionResult> Update(int eventId, int guestId, [FromBody] GuestUpdateDto model)
    {
        var eventExists = await context.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
            return BadRequest($"Event with id {eventId} was not found.");
        
        var guestItem = await context.Guests.FindAsync(guestId);
        if (guestItem == null)
            return NotFound($"Guest with id: {guestId} does not exist.");

        if (guestItem.EventId != eventId)
            return BadRequest($"Guest with id: {guestId} does not belong to event with id: {eventId}.");

        guestItem.TableId = model.TableId;
        guestItem.FirstName = model.FirstName;
        guestItem.LastName = model.LastName;
        guestItem.Age = model.Age;
        guestItem.Email = model.Email;
        guestItem.Phone = model.Phone;
        guestItem.Gender = model.Gender;
        guestItem.RsvpStatus = model.RsvpStatus;
        guestItem.Group = model.Group;
        guestItem.DietaryRequirements = model.DietaryRequirements;
        guestItem.Notes = model.Notes;

        await context.SaveChangesAsync();
        return NoContent();
    }
    
    // DELETE: api/Events/{eventId}/Guests/{guestId}
    [HttpDelete("{guestId:int}")]
    public async Task<IActionResult> Delete(int eventId, int guestId)
    {
        var eventExists = await context.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
            return BadRequest($"Event with id {eventId} was not found.");
        
        var guestItem = await context.Guests.FindAsync(guestId);
        if (guestItem == null)
            return NotFound($"Guest with id: {guestId} does not exist.");
        
        if (guestItem.EventId != eventId)
            return BadRequest($"Guest with id: {guestId} does not belong to event with id: {eventId}.");
        
        context.Guests.Remove(guestItem);
        await context.SaveChangesAsync();
        
        return NoContent();       
    }
}