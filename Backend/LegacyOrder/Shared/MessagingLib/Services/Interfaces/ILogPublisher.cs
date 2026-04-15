using LoggingLib.Models;

namespace LoggingLib.Services.Interfaces;

public interface ILogPublisher
{
    Task PublishAsync(LogMessage log);
    Task ErrorAsync(string message, string? exception = null);
    Task WarningAsync(string message, string? exception = null);
    
    Task InfoAsync(string message, string? exception = null);
    
}