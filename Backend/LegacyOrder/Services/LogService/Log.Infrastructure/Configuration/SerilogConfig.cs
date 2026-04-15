using Serilog;
using SLog = Serilog.Log;

namespace Log.Infrastructure.Configuration;

public static class SerilogConfig
{
    public static void Configure()
    {
        SLog.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();
    }
}