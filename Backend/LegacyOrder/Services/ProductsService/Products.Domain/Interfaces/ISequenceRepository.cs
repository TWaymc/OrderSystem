namespace Products.Domain.Interfaces;

public interface ISequenceRepository
{
    /// <summary>Atomically increments and returns the next sequence value for the given type.</summary>
    Task<long> GetNextNumberAsync(string sequenceType, CancellationToken cancellationToken = default);
}
