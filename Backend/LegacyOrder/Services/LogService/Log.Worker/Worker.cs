using Log.Infrastructure.Messaging;

namespace Log.Worker;

public class Worker : BackgroundService
{
    private readonly LogConsumer _consumer;

    public Worker(LogConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Start();
        return Task.CompletedTask;
    }
}