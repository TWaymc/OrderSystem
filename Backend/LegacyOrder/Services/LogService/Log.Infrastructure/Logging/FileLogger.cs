using Log.Application.Interfaces;
using Log.Domain.Entities;
using Serilog;
using SLog = Serilog.Log;


namespace Log.Infrastructure.Logging;

public class FileLogger : ILogWriter
{
    public void Write(LogEntry log)
    {
        switch (log.Level)
        {
            case "Error":
                SLog.Error("{Service} {Message} {Exception} {CorrelationId}",
                    log.ServiceName,
                    log.Message,
                    log.Exception,
                    log.CorrelationId);
                break;

            case "Warning":
                SLog.Warning("{Service} {Message} {CorrelationId}",
                    log.ServiceName,
                    log.Message,
                    log.CorrelationId);
                break;

            default:
                SLog.Information("{Service} {Message} {CorrelationId}",
                    log.ServiceName,
                    log.Message,
                    log.CorrelationId);
                break;
        }
    }
}