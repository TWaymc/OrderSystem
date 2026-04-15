namespace Contacts.Domain.Interfaces;

public interface ISequenceRepository
{
    Task<long> GetNextNumberAsync(string sequenceType, CancellationToken cancellationToken = default);
}
