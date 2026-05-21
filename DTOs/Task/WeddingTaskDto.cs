namespace WeddingPlannerApp.DTOs.Task;

public sealed class WeddingTaskDto
{
    public int TaskId { get; set; }
    public int EventId { get; set; }
    public required string Title { get; set; }
    public DateTime DueDate { get; set; }
    public required string Status { get; set; }
    public required string Priority { get; set; }
    public required string Category { get; set; }
    public bool Done { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}
