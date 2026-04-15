namespace Contacts.Application.Interfaces;

public interface ISequenceService
{
    Task<string> GetNextContactCodeAsync(CancellationToken cancellationToken = default);
}
