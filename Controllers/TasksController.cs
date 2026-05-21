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
    const string PaymentTaskPrefix = "[[payment-task:";
    const string PaymentTaskSuffix = "]]";
    const string SupplierPrefix = "Supplier: ";

    // GET: api/Events/{eventId}/Tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeddingTaskDto>>> GetAll(int eventId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        await RemoveDuplicatePaymentTasksAsync(eventId);
        await EnsureSupplierPaymentTasksAsync(eventId);
        await RemoveDuplicatePaymentTasksAsync(eventId);

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

    async Task RemoveDuplicatePaymentTasksAsync(int eventId)
    {
        var paymentTasks = await context.CheckListTasks
            .Where(t => t.EventId == eventId && t.Notes != null && t.Notes.StartsWith(PaymentTaskPrefix))
            .OrderByDescending(t => t.TaskId)
            .ToListAsync();

        var duplicates = paymentTasks
            .GroupBy(t => t.Notes)
            .SelectMany(g => g.Skip(1))
            .Concat(paymentTasks.GroupBy(t => t.Title).SelectMany(g => g.Skip(1)))
            .DistinctBy(t => t.TaskId)
            .ToList();

        if (duplicates.Count == 0)
            return;

        context.CheckListTasks.RemoveRange(duplicates);
        await context.SaveChangesAsync();
    }

    async Task EnsureSupplierPaymentTasksAsync(int eventId)
    {
        var eventDate = await context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => e.EventDate)
            .SingleAsync();

        var expenses = await context.Expenses
            .Include(e => e.Supplier)
            .Where(e => e.EventId == eventId && e.EstimatedAmount > 0)
            .ToListAsync();

        foreach (var expense in expenses)
        {
            var key = PaymentTaskKey(expense.ExpenseId);
            var marker = PaymentMarker(key);
            var existing = await context.CheckListTasks
                .Where(t => t.EventId == eventId && t.Notes == marker)
                .OrderBy(t => t.TaskId)
                .FirstOrDefaultAsync();
            var title = $"Pay: {CleanName(expense.Description, expense.Supplier?.Name)}";
            var isPaid = expense.PaymentStatus == PaymentStatus.Paid;

            if (existing is null)
            {
                context.CheckListTasks.Add(new CheckListTask
                {
                    EventId = eventId,
                    Title = title,
                    DueDate = PaymentDueDate(eventDate),
                    Priority = CheckListTaskPriority.High,
                    Category = CheckListTaskCategory.Vendors,
                    Status = isPaid ? CheckListTaskStatus.Completed : CheckListTaskStatus.Pending,
                    CompletedAt = isPaid ? DateTime.UtcNow : null,
                    Notes = marker
                });
            }
            else
            {
                existing.Title = title;
                existing.DueDate = PaymentDueDate(eventDate);
                existing.Priority = CheckListTaskPriority.High;
                existing.Category = CheckListTaskCategory.Vendors;
                existing.Status = isPaid ? CheckListTaskStatus.Completed : CheckListTaskStatus.Pending;
                existing.CompletedAt = isPaid ? existing.CompletedAt ?? DateTime.UtcNow : null;
            }
        }

        await context.SaveChangesAsync();
    }

    static string PaymentTaskKey(int expenseId) => $"expense:{expenseId}";
    static string PaymentMarker(string key) => $"{PaymentTaskPrefix}{key}{PaymentTaskSuffix}";

    static DateTime PaymentDueDate(DateTime eventDate)
    {
        var dueDate = eventDate.AddDays(-14).Date;
        return dueDate < DateTime.Today ? DateTime.Today : dueDate;
    }

    static string CleanName(string? description, string? supplierName)
    {
        var name = string.IsNullOrWhiteSpace(description) ? supplierName : description;
        if (string.IsNullOrWhiteSpace(name))
            return "Supplier payment";

        var clean = name.Trim();
        return clean.StartsWith(SupplierPrefix, StringComparison.OrdinalIgnoreCase)
            ? clean[SupplierPrefix.Length..].Trim()
            : clean;
    }

    static bool TryParseCategory(string value, out CheckListTaskCategory category)
    {
        if (value.Equals("Suppliers", StringComparison.OrdinalIgnoreCase))
        {
            category = CheckListTaskCategory.Vendors;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out category);
    }

    static string DisplayCategory(CheckListTaskCategory category) =>
        category == CheckListTaskCategory.Vendors ? "Suppliers" : category.ToString();

    static WeddingTaskDto ToDto(CheckListTask task) => new()
    {
        TaskId = task.TaskId,
        EventId = task.EventId,
        Title = task.Title,
        DueDate = task.DueDate,
        Status = task.Status.ToString().ToLowerInvariant(),
        Priority = task.Priority.ToString().ToLowerInvariant(),
        Category = DisplayCategory(task.Category),
        Done = task.Status == CheckListTaskStatus.Completed,
        CompletedAt = task.CompletedAt,
        Notes = task.Notes
    };
}
