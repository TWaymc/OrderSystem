namespace Contacts.Domain.Entities;

/// <summary>
/// Stores the last issued number per logical sequence (e.g. Type = "Property" for product codes).
/// </summary>
public class SequenceEntry
{
    /// <summary>Sequence key, e.g. "Property" for catalogue items, another value for future entities.</summary>
    public string SequenceType { get; set; } = string.Empty;

    public long SeqNumber { get; set; }
}
