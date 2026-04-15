using Log.Domain.Entities;

namespace Log.Application.Interfaces;

public interface ILogWriter
{
    void Write(LogEntry log);
}