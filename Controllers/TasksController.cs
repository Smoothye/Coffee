using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Task;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/Events/{eventId:int}/[controller]")]
[Authorize]
public class TasksController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/Events/{eventId}/Tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeddingTaskDto>>> GetAll(int eventId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var tasks = await context.CheckListTasks
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.Status == CheckListTaskStatus.Completed)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(tasks);
    }

    // GET: api/Events/{eventId}/Tasks/{taskId}
    [HttpGet("{taskId:int}")]
    public async Task<ActionResult<WeddingTaskDto>> GetById(int eventId, int taskId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var task = await context.CheckListTasks
            .Where(t => t.EventId == eventId && t.TaskId == taskId)
            .Select(t => ToDto(t))
            .SingleOrDefaultAsync();

        if (task is null)
            return NotFound($"Task with id: {taskId} was not found.");

        return Ok(task);
    }

    // POST: api/Events/{eventId}/Tasks
    [HttpPost]
    public async Task<ActionResult<WeddingTaskDto>> Create(int eventId, [FromBody] WeddingTaskCreateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        if (!TryParsePriority(model.Priority, out var priority))
            return BadRequest("Choose a valid task priority.");

        if (!TryParseCategory(model.Category, out var category))
            return BadRequest("Choose a valid task category.");

        var title = model.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Task title is required.");

        var dueDate = model.DueDate.Date;
        var nextDueDate = dueDate.AddDays(1);

        var duplicateExists = await context.CheckListTasks.AnyAsync(t =>
            t.EventId == eventId &&
            t.Title == title &&
            t.Category == category &&
            t.DueDate >= dueDate &&
            t.DueDate < nextDueDate);

        if (duplicateExists)
            return BadRequest("This task already exists for the selected date and category.");

        var task = new CheckListTask
        {
            EventId = eventId,
            Title = title,
            DueDate = dueDate,
            Priority = priority,
            Category = category,
            Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim()
        };

        context.CheckListTasks.Add(task);
        await context.SaveChangesAsync();

        var result = ToDto(task);
        return CreatedAtAction(nameof(GetById), new { eventId, taskId = result.TaskId }, result);
    }

    // PUT: api/Events/{eventId}/Tasks/{taskId}
    [HttpPut("{taskId:int}")]
    public async Task<IActionResult> Update(int eventId, int taskId, [FromBody] WeddingTaskUpdateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var task = await context.CheckListTasks
            .SingleOrDefaultAsync(t => t.EventId == eventId && t.TaskId == taskId);

        if (task is null)
            return NotFound($"Task with id: {taskId} was not found.");

        if (!TryParsePriority(model.Priority, out var priority))
            return BadRequest("Choose a valid task priority.");

        if (!TryParseCategory(model.Category, out var category))
            return BadRequest("Choose a valid task category.");

        var title = model.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Task title is required.");

        var dueDate = model.DueDate.Date;
        var nextDueDate = dueDate.AddDays(1);

        var duplicateExists = await context.CheckListTasks.AnyAsync(t =>
            t.EventId == eventId &&
            t.TaskId != taskId &&
            t.Title == title &&
            t.Category == category &&
            t.DueDate >= dueDate &&
            t.DueDate < nextDueDate);

        if (duplicateExists)
            return BadRequest("This task already exists for the selected date and category.");

        task.Title = title;
        task.DueDate = dueDate;
        task.Priority = priority;
        task.Category = category;
        task.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
        task.Status = model.Done ? CheckListTaskStatus.Completed : CheckListTaskStatus.Pending;
        task.CompletedAt = model.Done ? task.CompletedAt ?? DateTime.UtcNow : null;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Events/{eventId}/Tasks/{taskId}
    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> Delete(int eventId, int taskId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var task = await context.CheckListTasks
            .SingleOrDefaultAsync(t => t.EventId == eventId && t.TaskId == taskId);

        if (task is null)
            return NotFound($"Task with id: {taskId} was not found.");

        context.CheckListTasks.Remove(task);
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
        if (User.IsInRole("Admin"))
            return await context.Events.AnyAsync(e => e.EventId == eventId);

        return TryGetCurrentUserId(out var userId) &&
               await context.UserEvents.AnyAsync(ue => ue.EventId == eventId && ue.UserId == userId);
    }

    static bool TryParsePriority(string value, out CheckListTaskPriority priority) =>
        Enum.TryParse(value, ignoreCase: true, out priority);

    static bool TryParseCategory(string value, out CheckListTaskCategory category) =>
        Enum.TryParse(value, ignoreCase: true, out category);

    static WeddingTaskDto ToDto(CheckListTask task) => new()
    {
        TaskId = task.TaskId,
        EventId = task.EventId,
        Title = task.Title,
        DueDate = task.DueDate,
        Status = task.Status.ToString().ToLowerInvariant(),
        Priority = task.Priority.ToString().ToLowerInvariant(),
        Category = task.Category.ToString(),
        Done = task.Status == CheckListTaskStatus.Completed,
        CompletedAt = task.CompletedAt,
        Notes = task.Notes
    };
}
