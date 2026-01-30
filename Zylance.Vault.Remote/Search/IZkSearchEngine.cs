namespace Zylance.Vault.Remote.Search;

/// <summary>
///     Defines a zero-knowledge search engine that can index and search encrypted content
///     without the storage layer having access to plaintext data.
/// </summary>
/// <typeparam name="TItemId">
///     The type of item identifier. In zero-knowledge scenarios, this should be an encrypted
///     or hashed identifier to prevent the storage layer from correlating items.
/// </typeparam>
/// <remarks>
///     <para>
///         This interface supports zero-knowledge architecture by requiring content to be provided
///         for update and remove operations. This allows the search engine to determine which index
///         entries to modify without querying the storage layer for the item's indexed terms.
///     </para>
///     <para>
///         All indexing operations tokenize content client-side and only store encrypted/hashed
///         tokens and bucket references in remote storage.
///     </para>
/// </remarks>
public interface IZkSearchEngine<TItemId> where TItemId : notnull
{
    /// <summary>
    ///     Adds an item to the search index.
    /// </summary>
    /// <param name="itemId">The unique identifier for the item.</param>
    /// <param name="content">The plaintext content to index. Will be tokenized client-side.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     If the item is already indexed, this operation is a no-op. The item's IsIndexed flag
    ///     is checked to prevent duplicate indexing.
    /// </remarks>
    public Task AddItemAsync(TItemId itemId, string content);

    /// <summary>
    ///     Adds multiple items to the search index in a batch operation.
    /// </summary>
    /// <param name="items">A list of tuples containing item IDs and their content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This is more efficient than calling <see cref="AddItemAsync" /> multiple times as it
    ///     batches flag lookups and updates. Items that are already indexed will be skipped.
    /// </remarks>
    public Task AddItemsAsync(List<(TItemId itemId, string content)> items);

    /// <summary>
    ///     Updates an item's indexed content by removing old terms and adding new ones.
    /// </summary>
    /// <param name="itemId">The unique identifier for the item.</param>
    /// <param name="oldContent">
    ///     The previous content of the item. Required to determine which index entries to remove.
    /// </param>
    /// <param name="newContent">The new content to index.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     <para>
    ///         The <paramref name="oldContent" /> parameter is required because in a zero-knowledge
    ///         architecture, the storage layer cannot reveal which terms were previously indexed.
    ///         The client must provide both old and new content to perform efficient differential indexing.
    ///     </para>
    ///     <para>
    ///         Only tokens that changed between old and new content are updated, making this operation
    ///         more efficient than removing and re-adding the entire item.
    ///     </para>
    ///     <para>
    ///         If the item is not currently indexed, this operation is a no-op.
    ///     </para>
    /// </remarks>
    public Task UpdateItemAsync(TItemId itemId, string oldContent, string newContent);

    /// <summary>
    ///     Updates multiple items' indexed content in a batch operation.
    /// </summary>
    /// <param name="items">
    ///     A list of tuples containing item IDs, old content, and new content.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     More efficient than calling <see cref="UpdateItemAsync" /> multiple times.
    ///     Only items that are currently indexed will be updated.
    /// </remarks>
    public Task UpdateItemsAsync(List<(TItemId itemId, string oldContent, string newContent)> items);

    /// <summary>
    ///     Removes an item from the search index.
    /// </summary>
    /// <param name="itemId">The unique identifier for the item to remove.</param>
    /// <param name="content">
    ///     The content that was indexed. Required to determine which index entries to remove.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     <para>
    ///         The <paramref name="content" /> parameter is required because in a zero-knowledge
    ///         architecture, the storage layer cannot reveal which terms were indexed for this item.
    ///         The client must provide the content to determine which index entries to remove.
    ///     </para>
    ///     <para>
    ///         After removal, the item's IsIndexed flag is set to false, but the flag entry itself
    ///         is retained to track that this item was previously indexed.
    ///     </para>
    ///     <para>
    ///         If the item is not currently indexed, this operation is a no-op.
    ///     </para>
    /// </remarks>
    public Task RemoveItemAsync(TItemId itemId, string content);

    /// <summary>
    ///     Removes multiple items from the search index in a batch operation.
    /// </summary>
    /// <param name="items">A list of tuples containing item IDs and their content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     More efficient than calling <see cref="RemoveItemAsync" /> multiple times.
    ///     Only items that are currently indexed will be removed.
    /// </remarks>
    public Task RemoveItemsAsync(List<(TItemId itemId, string content)> items);

    /// <summary>
    ///     Searches for items matching all space-separated search terms.
    /// </summary>
    /// <param name="searchTerms">
    ///     Space-separated search terms. Items must match ALL terms to be included in results (AND semantics).
    /// </param>
    /// <param name="direction">
    ///     The direction to traverse buckets. <see cref="SearchDirection.LatestFirst" /> returns
    ///     more recently indexed items first.
    /// </param>
    /// <param name="fuzzy">
    ///     If true, performs substring matching (e.g., "hel" matches "hello").
    ///     If false, requires exact token matches.
    /// </param>
    /// <returns>A list of item IDs matching all search terms.</returns>
    /// <remarks>
    ///     <para>
    ///         <b>Search Semantics (AND):</b> For search query "coffee payment", items must contain
    ///         keywords matching BOTH "coffee" AND "payment" to be returned.
    ///     </para>
    ///     <para>
    ///         <b>Fuzzy Matching:</b> When enabled, each search term performs substring matching against
    ///         indexed keywords. For example, "pay" would match keywords "payment", "paycheck", "payday".
    ///     </para>
    ///     <para>
    ///         <b>Zero-Knowledge:</b> All keyword matching happens client-side after retrieving encrypted
    ///         keywords from storage. The storage layer never sees plaintext search terms.
    ///     </para>
    /// </remarks>
    public Task<List<TItemId>> SearchAsync(
        string searchTerms,
        SearchDirection direction = SearchDirection.LatestFirst,
        bool fuzzy = true
    );
}
