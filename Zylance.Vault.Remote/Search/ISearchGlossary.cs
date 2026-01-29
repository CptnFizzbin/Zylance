namespace Zylance.Vault.Remote.Search;

/// <summary>
/// Manages the search glossary, which tracks all unique keywords and their bucket metadata.
/// </summary>
/// <remarks>
/// <para>
/// In a zero-knowledge architecture, implementations should:
/// - Use deterministic encryption for keyword values (allowing client to look up encrypted keywords)
/// - Store encrypted keyword-to-bucket mappings on the remote server
/// - Decrypt the full glossary client-side for search operations
/// </para>
/// <para>
/// The glossary is typically much smaller than the full search index (only unique terms),
/// making it feasible to load entirely into client memory during search operations.
/// </para>
/// </remarks>
public interface ISearchGlossary
{
    /// <summary>
    /// Retrieves all keywords from the glossary.
    /// </summary>
    /// <returns>
    /// List of all keywords with their metadata. In zero-knowledge scenarios, this should
    /// return decrypted keywords for client-side matching.
    /// </returns>
    /// <remarks>
    /// This method loads the entire glossary, which is acceptable because:
    /// - The glossary only contains unique terms (typically much smaller than the full dataset)
    /// - Client needs to decrypt all keywords anyway to perform fuzzy matching
    /// - Server cannot perform filtering (keywords are encrypted)
    /// </remarks>
    public List<SearchKeyword> GetKeywords();

    /// <summary>
    /// Retrieves an existing keyword or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="keyword">The keyword value to look up or create.</param>
    /// <returns>
    /// The existing keyword with its metadata, or a newly created keyword with zero buckets.
    /// </returns>
    /// <remarks>
    /// For zero-knowledge implementations, the keyword should be encrypted before storage.
    /// The returned keyword should be decrypted for client-side use.
    /// </remarks>
    public SearchKeyword GetOrAddKeyword(string keyword);

    /// <summary>
    /// Saves or updates a keyword and its metadata in the glossary.
    /// </summary>
    /// <param name="keyword">The keyword to save, including updated bucket count.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <remarks>
    /// Typically called when a new bucket is created for a keyword, incrementing the
    /// <see cref="SearchKeyword.NumBuckets"/> count.
    /// </remarks>
    public Task SaveKeyword(SearchKeyword keyword);
}
