using Contacts.Application.Interfaces;
using Contacts.Domain.Interfaces;

namespace Contacts.Application.Services;

public class SequenceService : ISequenceService
{
    private const string ContactSequenceType = "Contact";
    private const string ContactCodePrefix = "C-";  // C as customer so far, like product later we could add ContactTypes if required 
    private const int ContactCodeWidth = 5;

    private readonly ISequenceRepository _repository;

    public SequenceService(ISequenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> GetNextContactCodeAsync(CancellationToken cancellationToken = default)
    {
        var nextNumber = await GetNextNumberAsync(ContactSequenceType, cancellationToken);
        return $"{ContactCodePrefix}{nextNumber.ToString($"D{ContactCodeWidth}")}";
    }

    private Task<long> GetNextNumberAsync(string sequenceType, CancellationToken cancellationToken)
        => _repository.GetNextNumberAsync(sequenceType, cancellationToken);
}
