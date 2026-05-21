using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Supplier;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/Events/{eventId:int}/Suppliers")]
[Authorize]
public class SuppliersController(ApplicationDbContext context) : ControllerBase
{
    const string PaymentTaskPrefix = "[[payment-task:";
    const string PaymentTaskSuffix = "]]";
    const string SupplierPrefix = "Supplier: ";
    const string SupplierDepositPrefix = "Supplier advance: ";
    const string SupplierRemainingPrefix = "Supplier remaining: ";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll(int eventId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var eventSuppliers = await context.EventSuppliers
            .Where(es => es.EventId == eventId)
            .ToDictionaryAsync(es => es.SupplierId);

        var expenses = await context.Expenses
            .Where(e => e.EventId == eventId)
            .ToListAsync();

        var suppliers = await context.Suppliers
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(suppliers.Select(supplier =>
            ToDto(
                supplier,
                eventSuppliers.GetValueOrDefault(supplier.SupplierId),
                expenses.Where(e => e.SupplierId == supplier.SupplierId))));
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(int eventId, [FromBody] SupplierCreateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        if (model.Advance > model.Price)
            return BadRequest("Advance cannot be greater than price.");

        var supplier = new Supplier
        {
            Name = model.Name.Trim(),
            SupplierType = model.Category.Trim(),
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Notes = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            BasePrice = model.Price
        };

        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        var result = ToDto(supplier, null, []);
        return CreatedAtAction(nameof(GetById), new { eventId, supplierId = result.SupplierId }, result);
    }

    [HttpGet("{supplierId:int}")]
    public async Task<ActionResult<SupplierDto>> GetById(int eventId, int supplierId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var supplier = await context.Suppliers.FindAsync(supplierId);
        if (supplier is null)
            return NotFound($"Supplier with id: {supplierId} was not found.");

        var eventSupplier = await context.EventSuppliers
            .SingleOrDefaultAsync(es => es.EventId == eventId && es.SupplierId == supplierId);

        var expenses = await context.Expenses
            .Where(e => e.EventId == eventId && e.SupplierId == supplierId)
            .ToListAsync();

        return Ok(ToDto(supplier, eventSupplier, expenses));
    }

    [HttpPut("{supplierId:int}")]
    public async Task<IActionResult> Update(int eventId, int supplierId, [FromBody] SupplierUpdateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        if (model.Advance > model.Price)
            return BadRequest("Advance cannot be greater than price.");

        var supplier = await context.Suppliers.FindAsync(supplierId);
        if (supplier is null)
            return NotFound($"Supplier with id: {supplierId} was not found.");

        supplier.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        supplier.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        supplier.Notes = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        supplier.BasePrice = model.Price;

        await context.SaveChangesAsync();

        var eventSupplier = await context.EventSuppliers
            .SingleOrDefaultAsync(es => es.EventId == eventId && es.SupplierId == supplierId);
        if (eventSupplier?.CollaborationStatus is not null)
            await SyncSupplierBudgetAsync(eventId, supplierId, model.Advance, model.RemainingPaid);

        return NoContent();
    }

    [HttpPut("{supplierId:int}/Status")]
    public async Task<IActionResult> UpdateStatus(int eventId, int supplierId, [FromBody] EventSupplierStatusUpdateDto model)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

        var supplier = await context.Suppliers.FindAsync(supplierId);
        if (supplier is null)
            return NotFound($"Supplier with id: {supplierId} was not found.");

        if (model.Advance > supplier.BasePrice)
            return BadRequest("Advance cannot be greater than price.");

        var status = NormalizeStatus(model.CollaborationStatus);
        var eventSupplier = await context.EventSuppliers
            .SingleOrDefaultAsync(es => es.EventId == eventId && es.SupplierId == supplierId);

        if (status is null)
        {
            if (eventSupplier is not null)
                context.EventSuppliers.Remove(eventSupplier);

            await RemoveSupplierBudgetAsync(eventId, supplierId);
            await context.SaveChangesAsync();
            return NoContent();
        }

        if (eventSupplier is null)
        {
            context.EventSuppliers.Add(new EventSupplier
            {
                EventId = eventId,
                SupplierId = supplierId,
                CollaborationStatus = status,
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim()
            });
        }
        else
        {
            eventSupplier.CollaborationStatus = status;
            eventSupplier.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
        }

        await context.SaveChangesAsync();
        await SyncSupplierBudgetAsync(eventId, supplierId, model.Advance, model.RemainingPaid);

        return NoContent();
    }

    async Task SyncSupplierBudgetAsync(int eventId, int supplierId, decimal advanceAmount, bool remainingPaid)
    {
        await RemoveSupplierBudgetAsync(eventId, supplierId);

        var supplier = await context.Suppliers.FindAsync(supplierId);
        var eventSupplier = await context.EventSuppliers
            .SingleOrDefaultAsync(es => es.EventId == eventId && es.SupplierId == supplierId);

        if (supplier is null || eventSupplier?.CollaborationStatus is null || supplier.BasePrice <= 0)
            return;

        var category = ExpenseCategoryFor(supplier.SupplierType);
        if (eventSupplier.CollaborationStatus == "pending")
        {
            context.Expenses.Add(new Expense
            {
                EventId = eventId,
                SupplierId = supplierId,
                Description = $"{SupplierPrefix}{supplier.Name}",
                ExpenseCategory = category,
                EstimatedAmount = supplier.BasePrice,
                ActualAmount = supplier.BasePrice,
                PaymentStatus = PaymentStatus.Unpaid
            });
            await context.SaveChangesAsync();
            await SyncSupplierPaymentTasksAsync(eventId, supplierId);
            return;
        }

        var advance = Math.Min(advanceAmount, supplier.BasePrice);
        var remaining = Math.Max(0, supplier.BasePrice - advance);

        if (advance > 0)
        {
            context.Expenses.Add(new Expense
            {
                EventId = eventId,
                SupplierId = supplierId,
                Description = $"{SupplierDepositPrefix}{supplier.Name}",
                ExpenseCategory = category,
                EstimatedAmount = advance,
                ActualAmount = advance,
                PaymentStatus = PaymentStatus.Paid
            });
        }

        if (remaining > 0)
        {
            context.Expenses.Add(new Expense
            {
                EventId = eventId,
                SupplierId = supplierId,
                Description = $"{SupplierRemainingPrefix}{supplier.Name}",
                ExpenseCategory = category,
                EstimatedAmount = remaining,
                ActualAmount = remaining,
                PaymentStatus = remainingPaid ? PaymentStatus.Paid : PaymentStatus.Unpaid
            });
        }

        await context.SaveChangesAsync();
        await SyncSupplierPaymentTasksAsync(eventId, supplierId);
    }

    async Task RemoveSupplierBudgetAsync(int eventId, int supplierId)
    {
        var expenses = await context.Expenses
            .Where(e => e.EventId == eventId && e.SupplierId == supplierId)
            .ToListAsync();

        foreach (var expense in expenses)
        {
            await DeletePaymentTaskAsync(eventId, PaymentTaskKey(expense.ExpenseId));
        }

        context.Expenses.RemoveRange(expenses);
    }

    async Task SyncSupplierPaymentTasksAsync(int eventId, int supplierId)
    {
        var expenses = await context.Expenses
            .Where(e => e.EventId == eventId && e.SupplierId == supplierId)
            .ToListAsync();

        foreach (var expense in expenses)
        {
            var key = PaymentTaskKey(expense.ExpenseId);
            if (expense.PaymentStatus == PaymentStatus.Paid || expense.EstimatedAmount <= 0)
            {
                await DeletePaymentTaskAsync(eventId, key);
                continue;
            }

            var existing = await FindPaymentTaskAsync(eventId, key);
            var eventDate = await context.Events
                .Where(e => e.EventId == eventId)
                .Select(e => e.EventDate)
                .SingleAsync();
            var title = $"Pay: {CleanName(expense.Description)}";

            if (existing is null)
            {
                context.CheckListTasks.Add(new CheckListTask
                {
                    EventId = eventId,
                    Title = title,
                    DueDate = PaymentDueDate(eventDate),
                    Priority = CheckListTaskPriority.High,
                    Category = CheckListTaskCategory.Vendors,
                    Notes = PaymentMarker(key)
                });
            }
            else
            {
                existing.Title = title;
                existing.DueDate = PaymentDueDate(eventDate);
                existing.Status = CheckListTaskStatus.Pending;
                existing.CompletedAt = null;
            }

            await context.SaveChangesAsync();
        }
    }

    Task<CheckListTask?> FindPaymentTaskAsync(int eventId, string key) =>
        context.CheckListTasks
            .SingleOrDefaultAsync(t => t.EventId == eventId && t.Notes == PaymentMarker(key));

    async Task DeletePaymentTaskAsync(int eventId, string key)
    {
        var task = await FindPaymentTaskAsync(eventId, key);
        if (task is null)
            return;

        context.CheckListTasks.Remove(task);
        await context.SaveChangesAsync();
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

    static string? NormalizeStatus(string? status)
    {
        var value = status?.Trim().ToLowerInvariant();
        return value is "pending" or "confirmed" ? value : null;
    }

    static ExpenseCategory ExpenseCategoryFor(string category) => category switch
    {
        "Cake" or "Candy Bar" or "Catering" => ExpenseCategory.Catering,
        "Flowers" => ExpenseCategory.Florals,
        "Music/DJ" => ExpenseCategory.Music,
        "Decorations" or "Rentals" => ExpenseCategory.Decor,
        "Photography" => ExpenseCategory.Photography,
        _ => ExpenseCategory.Other
    };

    static string PaymentTaskKey(int expenseId) => $"expense:{expenseId}";
    static string PaymentMarker(string key) => $"{PaymentTaskPrefix}{key}{PaymentTaskSuffix}";

    static DateTime PaymentDueDate(DateTime eventDate)
    {
        var dueDate = eventDate.AddDays(-14).Date;
        return dueDate < DateTime.Today ? DateTime.Today : dueDate;
    }

    static string CleanName(string? value) => string.IsNullOrWhiteSpace(value) ? "Supplier payment" : value.Trim();

    static SupplierDto ToDto(Supplier supplier, EventSupplier? eventSupplier, IEnumerable<Expense> expenses)
    {
        var expenseList = expenses.ToList();
        var remainingExpense = expenseList.FirstOrDefault(e =>
            e.Description != null && e.Description.StartsWith(SupplierRemainingPrefix, StringComparison.OrdinalIgnoreCase));
        var advance = expenseList
            .Where(e => e.Description != null && e.Description.StartsWith(SupplierDepositPrefix, StringComparison.OrdinalIgnoreCase))
            .Sum(e => e.ActualAmount > 0 ? e.ActualAmount : e.EstimatedAmount);
        advance = Math.Min(advance, supplier.BasePrice);
        var remaining = Math.Max(0, supplier.BasePrice - advance);

        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            Category = supplier.SupplierType,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Description = supplier.Notes,
            Price = supplier.BasePrice,
            Advance = advance,
            Remaining = remaining,
            RemainingPaid = remainingExpense?.PaymentStatus == PaymentStatus.Paid || remaining <= 0,
            CollaborationStatus = eventSupplier?.CollaborationStatus,
            EventNotes = eventSupplier?.Notes
        };
    }
}
