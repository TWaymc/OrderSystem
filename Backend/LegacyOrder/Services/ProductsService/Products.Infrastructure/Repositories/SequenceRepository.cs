using System.Data;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Infrastructure.Data;

namespace Products.Infrastructure.Repositories;

public class SequenceRepository : ISequenceRepository
{
    private readonly AppDbContext _context;

    public SequenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<long> GetNextNumberAsync(string sequenceType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sequenceType))
            throw new ArgumentException("Sequence type is required.", nameof(sequenceType));

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var entry = await _context.Sequences
                .FirstOrDefaultAsync(e => e.SequenceType == sequenceType, cancellationToken);

            if (entry == null)
            {
                entry = new SequenceEntry { SequenceType = sequenceType, SeqNumber = 1 };
                _context.Sequences.Add(entry);
            }
            else
            {
                entry.SeqNumber++;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return entry.SeqNumber;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
