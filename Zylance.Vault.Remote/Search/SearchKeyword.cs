namespace Zylance.Vault.Remote.Search;

/// <summary>
/// Represents a keyword in the search glossary with metadata about its buckets.
/// </summary>
/// <remarks>
/// Keywords are created during indexing when new terms are encountered. Each keyword tracks
/// how many buckets have been created to store item IDs for that keyword. This allows the
/// search engine to know which buckets to fetch during search operations.
/// </remarks>
public record SearchKeyword
{
    /// <summary>
    /// Gets the keyword value (the actual search term).
    /// </summary>
    /// <remarks>
    /// In zero-knowledge scenarios, this should be the decrypted keyword value on the client side.
    /// The storage implementation should encrypt this value before persisting to remote storage.
    /// </remarks>
    public required string Value { get; init; }

    /// <summary>
    /// Gets the number of buckets that have been created for this keyword.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Buckets are numbered from 0 to NumBuckets-1. For example, if NumBuckets is 3,
    /// the bucket IDs would be "{keyword}:0", "{keyword}:1", and "{keyword}:2".
    /// </para>
    /// <para>
    /// A value of 0 indicates no items have been indexed for this keyword yet.
    /// </para>
    /// </remarks>
    public uint NumBuckets { get; init; }
}
