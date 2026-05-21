using System.Net;
using System.Net.Http.Json;
using WeddingPlannerApp.DTOs.Task;

namespace WeddingPlannerApp.Services;

public sealed partial class WeddingApiClient
{
    public async Task<List<WeddingTaskDto>> GetTasksAsync(int eventId)
    {
        await EnsureCanAccessEventAsync(eventId);
        return await _http.GetFromJsonAsync<List<WeddingTaskDto>>($"api/Events/{eventId}/Tasks") ?? [];
    }

    public async Task<WeddingTaskDto?> CreateTaskAsync(int eventId, WeddingTaskCreateDto task)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PostAsJsonAsync($"api/Events/{eventId}/Tasks", task);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WeddingTaskDto>();
    }

    public async Task UpdateTaskAsync(int eventId, int taskId, WeddingTaskUpdateDto task)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.PutAsJsonAsync($"api/Events/{eventId}/Tasks/{taskId}", task);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTaskAsync(int eventId, int taskId)
    {
        await EnsureCanAccessEventAsync(eventId);
        var response = await _http.DeleteAsync($"api/Events/{eventId}/Tasks/{taskId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        response.EnsureSuccessStatusCode();
    }
}
