namespace Products.Application.Interfaces;

public interface ISequenceService
{
    /// <summary>Generates the next product code (e.g. G-00001).</summary>
    Task<string> GetNextProductCodeAsync(CancellationToken cancellationToken = default);
}

