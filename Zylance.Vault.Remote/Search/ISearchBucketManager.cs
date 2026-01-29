namespace Zylance.Vault.Remote.Search;

/// <summary>
/// Manages storage and retrieval of search buckets containing item ID lists for keywords.
/// </summary>
/// <typeparam name="TItemId">
/// The type of item identifier stored in buckets. Should be encrypted in zero-knowledge scenarios.
/// </typeparam>
/// <remarks>
/// <para>
/// Buckets partition the item IDs for each keyword to prevent unbounded list growth. Each keyword
/// can have multiple buckets, numbered sequentially (e.g., "coffee:0", "coffee:1", "coffee:2").
/// </para>
/// <para>
/// In zero-knowledge architecture:
/// - Bucket contents should be encrypted before storage
/// - Bucket IDs may be encrypted/hashed keyword identifiers
/// - Server can observe bucket access patterns and sizes (metadata leakage)
/// </para>
/// <para>
/// <b>Security consideration:</b> Variable bucket sizes reveal keyword frequency to the server.
/// Consider implementing fixed-size buckets with padding for stronger privacy guarantees.
/// </para>
/// </remarks>
public interface ISearchBucketManager<TItemId>
{
    /// <summary>
    /// Gets the maximum number of item IDs that can be stored in a single bucket before
    /// a new bucket must be created.
    /// </summary>
    /// <remarks>
    /// Larger values reduce storage overhead (fewer buckets) but increase the amount of data
    /// that must be fetched and decrypted during search operations. Typical values might be
    /// 10-100 depending on dataset characteristics and performance requirements.
    /// </remarks>
    public uint MaxItemsPerBucket { get; }

    /// <summary>
    /// Loads a bucket by its identifier.
    /// </summary>
    /// <param name="bucketId">
    /// The bucket identifier, typically formatted as "{keyword}:{index}" (e.g., "coffee:0").
    /// </param>
    /// <returns>
    /// The decrypted bucket containing item IDs, or <c>null</c> if the bucket doesn't exist.
    /// </returns>
    /// <remarks>
    /// Implementations should decrypt bucket contents client-side. The server only stores and
    /// retrieves encrypted blobs identified by the bucket ID.
    /// </remarks>
    public Task<SearchBucket<TItemId>?> LoadBucket(BucketId bucketId);

    /// <summary>
    /// Saves a bucket to storage.
    /// </summary>
    /// <param name="bucket">The bucket to save, containing the bucket ID and item ID list.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <remarks>
    /// Implementations should encrypt bucket contents before sending to remote storage.
    /// If the bucket already exists, this operation should update it (upsert semantics).
    /// </remarks>
    public Task SaveBucket(SearchBucket<TItemId> bucket);
}
