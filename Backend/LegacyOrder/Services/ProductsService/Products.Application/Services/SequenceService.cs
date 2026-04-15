using Products.Application.Interfaces;
using Products.Domain.Interfaces;

namespace Products.Application.Services;

public class SequenceService : ISequenceService
{
    private const string ProductSequenceType = "Property";
    private const string ProductCodePrefix = "G-";  // so far G for Generic, Supposing to add a Product Type we could use a letter according to the type
    private const int ProductCodeWidth = 5;

    private readonly ISequenceRepository _repository;

    public SequenceService(ISequenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> GetNextProductCodeAsync(CancellationToken cancellationToken = default)
    {
        var nextNumber = await GetNextNumberAsync(ProductSequenceType, cancellationToken);
        return $"{ProductCodePrefix}{nextNumber.ToString($"D{ProductCodeWidth}")}";
    }

    private Task<long> GetNextNumberAsync(string sequenceType, CancellationToken cancellationToken)
        => _repository.GetNextNumberAsync(sequenceType, cancellationToken);
}
