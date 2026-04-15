namespace Orders.Application.Interfaces;

public interface ISequenceService
{
    Task<string> GetNextOrderCodeAsync(CancellationToken cancellationToken = default);
}
