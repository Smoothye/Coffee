using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.Models;
using WeddingPlannerApp.DTOs.Guest;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/Events/{eventId:int}/[controller]")]
[Authorize]
public class GuestsController(ApplicationDbContext context) : ControllerBase
{
    
    // GET: api/Events/{eventId}/Guests
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestDto>>> GetAll(int eventId)
    {
        if (!await EventExistsAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");

        if (!await CanAccessEventAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");
        
        var guestItems = await context.Guests
            .Where(g => g.EventId == eventId)
            .Select(g => new GuestDto
            {
                GuestId = g.GuestId,
                EventId = g.EventId,
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
                HasPlusOne = g.HasPlusOne,
                SeatNumber = g.SeatNumber,
                Notes = g.Notes,
            })
            .ToListAsync();
        
        return Ok(guestItems);
    }
    
    // GET: api/Events/{eventId}/Guests/{guestId}
    [HttpGet("{guestId:int}")]
    public async Task<ActionResult<GuestDto>> GetById(int eventId, int guestId)
    {
        if (!await EventExistsAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");

        if (!await CanAccessEventAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");
        
        var guestItem = await context.Guests
            .Where(g => g.EventId == eventId && g.GuestId == guestId)
            .Select(g => new GuestDto
            {
                GuestId = g.GuestId,
                EventId = g.EventId,
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
                HasPlusOne = g.HasPlusOne,
                SeatNumber = g.SeatNumber,
                Notes = g.Notes,
            })
            .SingleOrDefaultAsync();
        
        return Ok(guestItem);
    }
    
    // POST: api/Events/{eventId}/Guests
    [HttpPost]
    public async Task<ActionResult<GuestDto>> Create([FromBody] GuestCreateDto model, int eventId)
    {
        var validationError = ValidateDto(model);
        if (validationError is not null)
            return validationError;

        if (!await EventExistsAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");

        if (!await CanAccessEventAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");

        var entity = new Guest
        {
            EventId = eventId,
            TableId = model.TableId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Age = model.Age,
            Email = CleanEmail(model.Email),
            Phone = model.Phone,
            Gender = model.Gender,
            RsvpStatus = model.RsvpStatus,
            Group = model.Group,
            DietaryRequirements = model.DietaryRequirements,
            HasPlusOne = model.HasPlusOne,
            SeatNumber = model.SeatNumber,
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
            HasPlusOne = entity.HasPlusOne,
            SeatNumber = entity.SeatNumber,
            Notes = entity.Notes
        };

        return CreatedAtAction(
            nameof(GetById),
            new { eventId = result.EventId, guestId = result.GuestId },
            result
        );
    }

    [NonAction]
    public Task<ActionResult<GuestDto>> Create(GuestDto model, int eventId) =>
        Create(ToCreateDto(model), eventId);
    
    // POST api/Events/{eventId}/Guests/bulk
    [HttpPost("bulk")]
    public async Task<ActionResult<GuestBulkCreateResultDto>> BulkCreate(
        int eventId,
        [FromBody] IEnumerable<GuestCreateDto> models
    )
    {
        if (!await EventExistsAsync(eventId))
            return NotFound($"Event with id {eventId} was not found.");

        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id {eventId} was not found.");

        var guestModels = models.ToList();

        if (guestModels.Count == 0)
            return BadRequest("At least one guest is required.");

        foreach (var guestModel in guestModels)
        {
            var validationError = ValidateDto(guestModel);
            if (validationError is not null)
                return validationError;
        }

        var guests = guestModels.Select(model => new Guest
        {
            EventId = eventId,
            TableId = model.TableId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Age = model.Age,
            Email = CleanEmail(model.Email),
            Phone = model.Phone,
            Gender = model.Gender,
            RsvpStatus = model.RsvpStatus,
            Group = model.Group,
            DietaryRequirements = model.DietaryRequirements,
            HasPlusOne = model.HasPlusOne,
            SeatNumber = model.SeatNumber,
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
                HasPlusOne = g.HasPlusOne,
                SeatNumber = g.SeatNumber,
                Notes = g.Notes
            })
        };

        return CreatedAtAction(nameof(GetAll), new { eventId }, result);
    }
    
    // POST: api/Events/{eventId}/Guests/{guestId}
    [HttpPut("{guestId:int}")]
    public async Task<IActionResult> Update(int eventId, int guestId, [FromBody] GuestUpdateDto model)
    {
        var validationError = ValidateDto(model);
        if (validationError is not null)
            return validationError;

        if (!await EventExistsAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");

        if (!await CanAccessEventAsync(eventId))
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
        guestItem.Email = CleanEmail(model.Email);
        guestItem.Phone = model.Phone;
        guestItem.Gender = model.Gender;
        guestItem.RsvpStatus = model.RsvpStatus;
        guestItem.Group = model.Group;
        guestItem.DietaryRequirements = model.DietaryRequirements;
        guestItem.HasPlusOne = model.HasPlusOne;
        guestItem.SeatNumber = model.SeatNumber;
        guestItem.Notes = model.Notes;

        await context.SaveChangesAsync();
        return NoContent();
    }
    
    // DELETE: api/Events/{eventId}/Guests/{guestId}
    [HttpDelete("{guestId:int}")]
    public async Task<IActionResult> Delete(int eventId, int guestId)
    {
        if (!await EventExistsAsync(eventId))
            return BadRequest($"Event with id {eventId} was not found.");

        if (!await CanAccessEventAsync(eventId))
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

    static string CleanEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? "" : email.Trim();

    static BadRequestObjectResult? ValidateDto<T>(T model)
    {
        var context = new ValidationContext(model!);
        var results = new List<ValidationResult>();

        return Validator.TryValidateObject(model!, context, results, validateAllProperties: true)
            ? null
            : new BadRequestObjectResult(results);
    }

    static GuestCreateDto ToCreateDto(GuestDto guest) => new()
    {
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
        Notes = guest.Notes
    };

    async Task<bool> EventExistsAsync(int eventId) =>
        await context.Events.AnyAsync(e => e.EventId == eventId);

    bool TryGetCurrentUserId(out int userId)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out userId);
    }

    async Task<bool> CanAccessEventAsync(int eventId)
    {
        if (User.Identity?.IsAuthenticated != true)
            return await EventExistsAsync(eventId);

        if (User.IsInRole("Admin"))
            return await context.Events.AnyAsync(e => e.EventId == eventId);

        return TryGetCurrentUserId(out var userId) &&
               await context.UserEvents.AnyAsync(ue => ue.EventId == eventId && ue.UserId == userId);
    }
}
