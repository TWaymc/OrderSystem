using Orders.Worker.Messaging;

namespace Orders.Worker;

public class Worker : BackgroundService
{
    private readonly CacheInvalidationConsumer _consumer;

    public Worker(CacheInvalidationConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Start();
        return Task.CompletedTask;
    }
}
