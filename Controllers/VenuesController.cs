using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Venue;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/Venues
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAll()
    {
        var venues = await context.Venues
            .Select(v => new VenueDto
            {
                VenueId = v.VenueId,
                Name = v.Name,
                Address = v.Address,
                MinCapacity = v.MinCapacity,
                MaxCapacity = v.MaxCapacity,
                EstimatedPrice = v.EstimatedPrice
            })
            .ToListAsync();

        return Ok(venues);
    }

    // GET: api/Venues/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VenueDto>> GetById(int id)
    {
        var venue = await context.Venues
            .Where(v => v.VenueId == id)
            .Select(v => new VenueDto
            {
                VenueId = v.VenueId,
                Name = v.Name,
                Address = v.Address,
                MinCapacity = v.MinCapacity,
                MaxCapacity = v.MaxCapacity,
                EstimatedPrice = v.EstimatedPrice
            })
            .SingleOrDefaultAsync();

        if (venue == null)
            return NotFound($"Venue with id: {id} does not exist.");

        return Ok(venue);
    }

    // POST: api/Venues
    [HttpPost]
    public async Task<ActionResult<VenueDto>> Create([FromBody] VenueCreateDto model)
    {
        if (model.MinCapacity > model.MaxCapacity)
            return BadRequest("MinCapacity cannot be greater than MaxCapacity.");

        var entity = new Venue
        {
            Name = model.Name,
            Address = model.Address,
            MinCapacity = model.MinCapacity,
            MaxCapacity = model.MaxCapacity,
            EstimatedPrice = model.EstimatedPrice
        };

        context.Venues.Add(entity);
        await context.SaveChangesAsync();

        var result = new VenueDto
        {
            VenueId = entity.VenueId,
            Name = entity.Name,
            Address = entity.Address,
            MinCapacity = entity.MinCapacity,
            MaxCapacity = entity.MaxCapacity,
            EstimatedPrice = entity.EstimatedPrice
        };

        return CreatedAtAction(nameof(GetById), new { id = result.VenueId }, result);
    }

    // PUT: api/Venues/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VenueUpdateDto model)
    {
        if (model.MinCapacity > model.MaxCapacity)
            return BadRequest("MinCapacity cannot be greater than MaxCapacity.");

        var venue = await context.Venues.FindAsync(id);
        if (venue == null)
            return NotFound($"Venue with id {id} was not found.");

        venue.Name = model.Name;
        venue.Address = model.Address;
        venue.MinCapacity = model.MinCapacity;
        venue.MaxCapacity = model.MaxCapacity;
        venue.EstimatedPrice = model.EstimatedPrice;

        await context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Venues/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var venue = await context.Venues
            .Include(v => v.Events)
            .SingleOrDefaultAsync(v => v.VenueId == id);

        if (venue == null)
            return NotFound($"Venue with id {id} was not found.");

        if (venue.Events.Count != 0)
            return BadRequest("Cannot delete a venue that is assigned to one or more events.");

        context.Venues.Remove(venue);
        await context.SaveChangesAsync();

        return NoContent();
    }
}