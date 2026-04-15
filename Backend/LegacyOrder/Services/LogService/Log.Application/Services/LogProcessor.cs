using Log.Application.Interfaces;
using Log.Domain.Entities;

namespace LogService.Application.Services;

public class LogProcessor
{
    private readonly ILogWriter _logger;

    public LogProcessor(ILogWriter logger)
    {
        _logger = logger;
    }

    public void Process(LogEntry log)
    {
        _logger.Write(log);
    }
}