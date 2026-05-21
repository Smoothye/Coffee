using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Budget;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<BudgetItemDto>> GetBudgetItemsAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);
        return await _http.GetFromJsonAsync<List<BudgetItemDto>>($"api/Events/{eventId}/BudgetItems") ?? [];
    }

    public async Task UpdateBudgetItemPaymentAsync(int eventId, int budgetItemId, bool paid)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PutAsJsonAsync(
            $"api/Events/{eventId}/BudgetItems/{budgetItemId}/Payment",
            new BudgetItemPaymentUpdateDto { Paid = paid });
        response.EnsureSuccessStatusCode();
    }
}
