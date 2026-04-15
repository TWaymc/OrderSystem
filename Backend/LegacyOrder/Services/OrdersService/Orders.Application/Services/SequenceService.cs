using Orders.Application.Interfaces;
using Orders.Domain.Interfaces;

namespace Orders.Application.Services;

public class SequenceService : ISequenceService
{
    private const string OrderSequenceType = "Order";
    private const string OrderCodePrefix = "O-";
    private const int OrderCodeWidth = 5;

    private readonly ISequenceRepository _repository;

    public SequenceService(ISequenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> GetNextOrderCodeAsync(CancellationToken cancellationToken = default)
    {
        var nextNumber = await _repository.GetNextNumberAsync(OrderSequenceType, cancellationToken);
        return $"{OrderCodePrefix}{nextNumber.ToString($"D{OrderCodeWidth}")}";
    }
}
