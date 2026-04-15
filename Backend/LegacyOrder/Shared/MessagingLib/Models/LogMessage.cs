namespace LoggingLib.Models;

public class LogMessage
{
    public string ServiceName { get; set; }
    public string Level { get; set; } // Info, Error, Warning
    public string Message { get; set; }
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
}