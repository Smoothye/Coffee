using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Budget;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/Events/{eventId:int}/BudgetItems")]
[Authorize]
public class BudgetItemsController(ApplicationDbContext context) : ControllerBase
{
    const string PaymentTaskPrefix = "[[payment-task:";
    const string PaymentTaskSuffix = "]]";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetItemDto>>> GetAll(int eventId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        await RemoveDuplicateSupplierBudgetItemsAsync(eventId);

        var items = await context.Expenses
            .Include(e => e.Supplier)
            .Where(e => e.EventId == eventId)
            .OrderBy(e => e.ExpenseCategory)
            .ThenBy(e => e.Description)
            .Select(e => ToDto(e))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{budgetItemId:int}")]
    public async Task<ActionResult<BudgetItemDto>> GetById(int eventId, int budgetItemId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var item = await context.Expenses
            .Include(e => e.Supplier)
            .Where(e => e.EventId == eventId && e.ExpenseId == budgetItemId)
            .Select(e => ToDto(e))
            .SingleOrDefaultAsync();

        if (item is null)
            return NotFound($"Budget item with id: {budgetItemId} was not found.");

        return Ok(item);
    }

    [HttpPut("{budgetItemId:int}/Payment")]
    public async Task<IActionResult> UpdatePayment(int eventId, int budgetItemId, [FromBody] BudgetItemPaymentUpdateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var item = await context.Expenses
            .Include(e => e.Supplier)
            .SingleOrDefaultAsync(e => e.EventId == eventId && e.ExpenseId == budgetItemId);

        if (item is null)
            return NotFound($"Budget item with id: {budgetItemId} was not found.");

        item.PaymentStatus = model.Paid ? PaymentStatus.Paid : PaymentStatus.Unpaid;
        await SyncPaymentTaskAsync(item);
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

    static string DisplayCategory(ExpenseCategory? category) => category switch
    {
        ExpenseCategory.DressSuit => "Dress/Suit",
        null => "Other",
        _ => category.Value.ToString()
    };

    async Task RemoveDuplicateSupplierBudgetItemsAsync(int eventId)
    {
        var items = await context.Expenses
            .Include(e => e.Supplier)
            .Where(e => e.EventId == eventId)
            .ToListAsync();

        var removedItems = new List<Expense>();
        foreach (var group in items.GroupBy(e => e.SupplierId).Where(g => g.Count() > 1))
        {
            var supplierPrice = group.First().Supplier?.BasePrice;
            var keep = group
                .OrderBy(e => supplierPrice.HasValue && e.ActualAmount == supplierPrice.Value ? 0 : 1)
                .ThenByDescending(e => e.ExpenseId)
                .First();

            removedItems.AddRange(group.Where(e => e.ExpenseId != keep.ExpenseId));
        }

        if (removedItems.Count == 0)
            return;

        var markers = removedItems
            .Select(item => PaymentMarker(PaymentTaskKey(item.ExpenseId)))
            .ToList();
        var paymentTasks = await context.CheckListTasks
            .Where(t => t.EventId == eventId && t.Notes != null && markers.Contains(t.Notes))
            .ToListAsync();

        context.CheckListTasks.RemoveRange(paymentTasks);
        context.Expenses.RemoveRange(removedItems);
        await context.SaveChangesAsync();
    }

    static string PaymentTaskKey(int expenseId) => $"expense:{expenseId}";
    static string PaymentMarker(string key) => $"{PaymentTaskPrefix}{key}{PaymentTaskSuffix}";

    async Task SyncPaymentTaskAsync(Expense item)
    {
        var marker = PaymentMarker(PaymentTaskKey(item.ExpenseId));
        var tasks = await context.CheckListTasks
            .Where(t => t.EventId == item.EventId && t.Notes == marker)
            .OrderBy(t => t.TaskId)
            .ToListAsync();

        var task = tasks.FirstOrDefault();
        if (tasks.Count > 1)
            context.CheckListTasks.RemoveRange(tasks.Skip(1));

        var eventDate = await context.Events
            .Where(e => e.EventId == item.EventId)
            .Select(e => e.EventDate)
            .SingleAsync();
        var isPaid = item.PaymentStatus == PaymentStatus.Paid;

        if (task is null)
        {
            context.CheckListTasks.Add(new CheckListTask
            {
                EventId = item.EventId,
                Title = $"Pay: {CleanName(item)}",
                DueDate = PaymentDueDate(eventDate),
                Priority = CheckListTaskPriority.High,
                Category = CheckListTaskCategory.Vendors,
                Status = isPaid ? CheckListTaskStatus.Completed : CheckListTaskStatus.Pending,
                CompletedAt = isPaid ? DateTime.UtcNow : null,
                Notes = marker
            });
            return;
        }

        task.Title = $"Pay: {CleanName(item)}";
        task.DueDate = PaymentDueDate(eventDate);
        task.Priority = CheckListTaskPriority.High;
        task.Category = CheckListTaskCategory.Vendors;
        task.Status = isPaid ? CheckListTaskStatus.Completed : CheckListTaskStatus.Pending;
        task.CompletedAt = isPaid ? task.CompletedAt ?? DateTime.UtcNow : null;
    }

    static DateTime PaymentDueDate(DateTime eventDate)
    {
        var dueDate = eventDate.AddDays(-14).Date;
        return dueDate < DateTime.Today ? DateTime.Today : dueDate;
    }

    static string CleanName(Expense item)
    {
        var name = item.Description;
        if (string.IsNullOrWhiteSpace(name))
            name = item.Supplier?.Name;

        if (!string.IsNullOrWhiteSpace(name) &&
            name.StartsWith("Supplier: ", StringComparison.OrdinalIgnoreCase))
        {
            name = name["Supplier: ".Length..];
        }

        return string.IsNullOrWhiteSpace(name) ? "Budget item" : name.Trim();
    }

    static BudgetItemDto ToDto(Expense item) => new()
    {
        BudgetItemId = item.ExpenseId,
        EventId = item.EventId,
        SupplierId = item.SupplierId,
        IsSupplierLinked = true,
        Name = CleanName(item),
        Category = DisplayCategory(item.ExpenseCategory),
        Estimated = (int)Math.Round(item.EstimatedAmount, MidpointRounding.AwayFromZero),
        Actual = (int)Math.Round(item.ActualAmount, MidpointRounding.AwayFromZero),
        Paid = item.PaymentStatus == PaymentStatus.Paid,
        ExpenseDate = item.ExpenseDate
    };
}
