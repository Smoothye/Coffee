using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Table;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/Events/{eventId:int}/[controller]")]
[Authorize]
public class TablesController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/Events/{eventId}/Tables
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeddingTableDto>>> GetAll(int eventId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var tables = await context.WeddingTables
            .Where(t => t.EventId == eventId)
            .OrderByDescending(t => t.IsHeadTable)
            .ThenBy(t => t.TableNumber)
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(tables);
    }

    // GET: api/Events/{eventId}/Tables/{tableId}
    [HttpGet("{tableId:int}")]
    public async Task<ActionResult<WeddingTableDto>> GetById(int eventId, int tableId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var table = await context.WeddingTables
            .Where(t => t.EventId == eventId && t.TableId == tableId)
            .Select(t => ToDto(t))
            .SingleOrDefaultAsync();

        if (table == null)
            return NotFound($"Table with id: {tableId} was not found.");

        return Ok(table);
    }

    // POST: api/Events/{eventId}/Tables
    [HttpPost]
    public async Task<ActionResult<WeddingTableDto>> Create(int eventId, [FromBody] WeddingTableCreateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        if (model.IsHeadTable)
        {
            var headTableExists = await context.WeddingTables
                .AnyAsync(t => t.EventId == eventId && t.IsHeadTable);

            if (headTableExists)
                return BadRequest("This event already has a head table.");
        }

        var table = new WeddingTable
        {
            EventId = eventId,
            TableNumber = model.TableNumber,
            Capacity = model.Capacity,
            IsHeadTable = model.IsHeadTable,
            Notes = model.Notes
        };

        context.WeddingTables.Add(table);
        await context.SaveChangesAsync();

        var result = ToDto(table);
        return CreatedAtAction(nameof(GetById), new { eventId, tableId = result.TableId }, result);
    }

    // PUT: api/Events/{eventId}/Tables/{tableId}
    [HttpPut("{tableId:int}")]
    public async Task<IActionResult> Update(int eventId, int tableId, [FromBody] WeddingTableUpdateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var table = await context.WeddingTables
            .SingleOrDefaultAsync(t => t.EventId == eventId && t.TableId == tableId);

        if (table == null)
            return NotFound($"Table with id: {tableId} was not found.");

        if (model.IsHeadTable && !table.IsHeadTable)
        {
            var headTableExists = await context.WeddingTables
                .AnyAsync(t => t.EventId == eventId && t.IsHeadTable && t.TableId != tableId);

            if (headTableExists)
                return BadRequest("This event already has a head table.");
        }

        table.TableNumber = model.TableNumber;
        table.Capacity = model.Capacity;
        table.IsHeadTable = model.IsHeadTable;
        table.Notes = model.Notes;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Events/{eventId}/Tables/{tableId}
    [HttpDelete("{tableId:int}")]
    public async Task<IActionResult> Delete(int eventId, int tableId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var table = await context.WeddingTables
            .Include(t => t.Guests)
            .SingleOrDefaultAsync(t => t.EventId == eventId && t.TableId == tableId);

        if (table == null)
            return NotFound($"Table with id: {tableId} was not found.");

        if (table.Guests.Count != 0)
            return BadRequest("Cannot delete a table that has seated guests.");

        context.WeddingTables.Remove(table);
        await context.SaveChangesAsync();

        return NoContent();
    }

    static WeddingTableDto ToDto(WeddingTable table) => new()
    {
        TableId = table.TableId,
        EventId = table.EventId,
        TableNumber = table.TableNumber,
        Capacity = table.Capacity,
        IsHeadTable = table.IsHeadTable,
        Notes = table.Notes
    };

    bool TryGetCurrentUserId(out int userId)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out userId);
    }

    async Task<bool> CanAccessEventAsync(int eventId)
    {
        if (User.IsInRole("Admin"))
            return await context.Events.AnyAsync(e => e.EventId == eventId);

        return TryGetCurrentUserId(out var userId) &&
               await context.UserEvents.AnyAsync(ue => ue.EventId == eventId && ue.UserId == userId);
    }
}
