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
            await SyncSupplierBudgetAsync(eventId, supplierId, model.Paid);

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
        await SyncSupplierBudgetAsync(eventId, supplierId, model.Paid);

        return NoContent();
    }

    async Task SyncSupplierBudgetAsync(int eventId, int supplierId, bool paid)
    {
        await RemoveSupplierBudgetAsync(eventId, supplierId);

        var supplier = await context.Suppliers.FindAsync(supplierId);
        var eventSupplier = await context.EventSuppliers
            .SingleOrDefaultAsync(es => es.EventId == eventId && es.SupplierId == supplierId);

        if (supplier is null || eventSupplier?.CollaborationStatus is null || supplier.BasePrice <= 0)
            return;

        var category = ExpenseCategoryFor(supplier.SupplierType);
        context.Expenses.Add(new Expense
        {
            EventId = eventId,
            SupplierId = supplierId,
            Description = $"{SupplierPrefix}{supplier.Name}",
            ExpenseCategory = category,
            EstimatedAmount = supplier.BasePrice,
            ActualAmount = supplier.BasePrice,
            PaymentStatus = paid ? PaymentStatus.Paid : PaymentStatus.Unpaid
        });

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
            if (expense.EstimatedAmount <= 0)
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
                    Notes = PaymentMarker(key)
                });
            }
            else
            {
                existing.Title = title;
                existing.DueDate = PaymentDueDate(eventDate);
                existing.Status = isPaid ? CheckListTaskStatus.Completed : CheckListTaskStatus.Pending;
                existing.CompletedAt = isPaid ? existing.CompletedAt ?? DateTime.UtcNow : null;
            }

            await context.SaveChangesAsync();
        }
    }

    async Task<CheckListTask?> FindPaymentTaskAsync(int eventId, string key)
    {
        var tasks = await context.CheckListTasks
            .Where(t => t.EventId == eventId && t.Notes == PaymentMarker(key))
            .OrderBy(t => t.TaskId)
            .ToListAsync();

        if (tasks.Count > 1)
        {
            context.CheckListTasks.RemoveRange(tasks.Skip(1));
            await context.SaveChangesAsync();
        }

        return tasks.FirstOrDefault();
    }

    async Task DeletePaymentTaskAsync(int eventId, string key)
    {
        var tasks = await context.CheckListTasks
            .Where(t => t.EventId == eventId && t.Notes == PaymentMarker(key))
            .ToListAsync();

        if (tasks.Count == 0)
            return;

        context.CheckListTasks.RemoveRange(tasks);
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

    static string CleanName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Supplier payment";

        var clean = value.Trim();
        return clean.StartsWith(SupplierPrefix, StringComparison.OrdinalIgnoreCase)
            ? clean[SupplierPrefix.Length..].Trim()
            : clean;
    }

    static SupplierDto ToDto(Supplier supplier, EventSupplier? eventSupplier, IEnumerable<Expense> expenses)
    {
        var expenseList = expenses.ToList();
        var supplierExpense = expenseList.FirstOrDefault(e =>
            e.Description != null && e.Description.StartsWith(SupplierPrefix, StringComparison.OrdinalIgnoreCase));
        var paid = supplierExpense?.PaymentStatus == PaymentStatus.Paid;

        if (supplierExpense is null && expenseList.Count > 0)
            paid = expenseList.All(e => e.PaymentStatus == PaymentStatus.Paid);

        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            Category = supplier.SupplierType,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Description = supplier.Notes,
            Price = supplier.BasePrice,
            Paid = paid,
            CollaborationStatus = eventSupplier?.CollaborationStatus,
            EventNotes = eventSupplier?.Notes
        };
    }
}
