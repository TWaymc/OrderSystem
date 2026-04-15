namespace Log.Domain.Entities;

public class LogEntry
{
    public string ServiceName { get; set; } = default!;
    public string Level { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
}