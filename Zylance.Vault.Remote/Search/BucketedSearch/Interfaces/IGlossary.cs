#nullable disable

using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;

/// <summary>
///     Manages the search glossary, which tracks all indexed keywords and their bucket metadata.
/// </summary>
/// <remarks>
///     <para>
///         The glossary is the central registry of all keywords that have been indexed.
///         Each keyword entry tracks how many buckets have been created to store item IDs for that keyword.
///     </para>
///     <para>
///         In zero-knowledge implementations, keyword values should be encrypted or hashed before
///         persistence to prevent the storage layer from knowing what terms are being searched.
///     </para>
/// </remarks>
public interface IGlossary
{
    /// <summary>
    ///     Retrieves keyword metadata for a specific keyword value.
    /// </summary>
    /// <param name="keyword">The keyword to look up.</param>
    /// <returns>
    ///     A <see cref="Keyword" /> object with metadata. If the keyword doesn't exist,
    ///     returns a new Keyword with NumBuckets = 0.
    /// </returns>
    /// <remarks>
    ///     This method should never return null. For non-existent keywords, it returns
    ///     a default Keyword object to simplify indexing logic.
    /// </remarks>
    public Keyword Get(string keyword);

    /// <summary>
    ///     Retrieves all keywords in the glossary.
    /// </summary>
    /// <returns>A list of all keyword metadata entries.</returns>
    /// <remarks>
    ///     <para>
    ///         This method is used during search operations to perform fuzzy matching client-side.
    ///         The entire glossary is loaded into memory and filtered based on search terms.
    ///     </para>
    ///     <para>
    ///         <b>Performance consideration:</b> For large glossaries (10,000+ keywords),
    ///         consider implementing client-side caching or incremental updates to reduce
    ///         the cost of repeatedly loading all keywords.
    ///     </para>
    /// </remarks>
    public List<Keyword> GetAll();

    /// <summary>
    ///     Saves or updates keyword metadata in the glossary.
    /// </summary>
    /// <param name="keyword">The keyword metadata to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <remarks>
    ///     This method is called when:
    ///     <list type="bullet">
    ///         <item>A new keyword is first indexed (NumBuckets = 1)</item>
    ///         <item>A new bucket is created because the current bucket is full (NumBuckets incremented)</item>
    ///     </list>
    ///     Implementations should use upsert semantics (insert or update based on keyword value).
    /// </remarks>
    public Task Save(Keyword keyword);
}
