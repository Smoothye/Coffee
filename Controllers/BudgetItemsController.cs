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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetItemDto>>> GetAll(int eventId)
    {
        if (!await CanAccessEventAsync(eventId))
            return NotFound($"Event with id: {eventId} was not found.");

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

    static string CleanName(Expense item)
    {
        var name = item.Description;
        if (string.IsNullOrWhiteSpace(name))
            name = item.Supplier?.Name;

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
