using System.Net;
using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Supplier;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<SupplierDto>> GetSuppliersAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);
        return await _http.GetFromJsonAsync<List<SupplierDto>>($"api/Events/{eventId}/Suppliers") ?? [];
    }

    public async Task<SupplierDto?> CreateSupplierAsync(int eventId, SupplierCreateDto supplier)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PostAsJsonAsync($"api/Events/{eventId}/Suppliers", supplier);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SupplierDto>();
    }

    public async Task UpdateSupplierAsync(int eventId, int supplierId, SupplierUpdateDto supplier)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PutAsJsonAsync($"api/Events/{eventId}/Suppliers/{supplierId}", supplier);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateSupplierStatusAsync(int eventId, int supplierId, EventSupplierStatusUpdateDto status)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PutAsJsonAsync($"api/Events/{eventId}/Suppliers/{supplierId}/Status", status);
        response.EnsureSuccessStatusCode();
    }
}
