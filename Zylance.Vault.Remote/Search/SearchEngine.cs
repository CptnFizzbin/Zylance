
namespace Zylance.Vault.Remote.Search;

/// <summary>
/// A zero-knowledge search engine designed for client-side operation with encrypted remote storage.
/// Tokenizes text, maintains an inverted index using buckets, and performs client-side fuzzy search.
/// </summary>
/// <typeparam name="TItemId">
/// The type of item identifier. In zero-knowledge scenarios, this is typically an encrypted
/// reference (e.g., HMAC, encrypted UUID) rather than a database key.
/// </typeparam>
/// <remarks>
/// <para>
/// This search engine is designed for zero-knowledge architecture where:
/// - Client performs all tokenization and matching locally
/// - Server stores encrypted keywords and buckets without knowing their content
/// - Search results are decrypted client-side
/// </para>
/// <para>
/// The bucketing strategy prevents unbounded growth of item lists per keyword by partitioning
/// items into fixed-size buckets. Buckets can be fetched and decrypted incrementally.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is not thread-safe. Concurrent indexing operations may result
/// in race conditions during bucket creation. For multithreaded scenarios, implement external
/// synchronization or use optimistic concurrency control in storage implementations.
/// </para>
/// </remarks>
public class SearchEngine<TItemId>(
    ISearchGlossary glossary,
    ISearchBucketManager<TItemId> bucketManager
)
{
    /// <summary>
    /// Indexes text content for a specific item by tokenizing the text and adding the item ID
    /// to keyword buckets for each token.
    /// </summary>
    /// <param name="itemId">
    /// Unique identifier for the item being indexed. In zero-knowledge scenarios, this should be
    /// an encrypted identifier that reveals nothing about the actual item to the server.
    /// </param>
    /// <param name="text">
    /// Text content to tokenize and index. This is tokenized client-side before any data is sent
    /// to remote storage, ensuring the server never sees plaintext content.
    /// </param>
    /// <remarks>
    /// <para>
    /// The indexing process:
    /// 1. Tokenizes text into lowercase terms (splits on non-word characters)
    /// 2. For each token, retrieves or creates a keyword entry
    /// 3. Adds the item ID to the appropriate bucket for that keyword
    /// 4. Creates new buckets when existing ones reach capacity
    /// </para>
    /// <para>
    /// <b>Note:</b> Calling this method multiple times with the same itemId and text will result in
    /// duplicate entries in the buckets. Consider implementing deduplication if re-indexing is needed.
    /// </para>
    /// <para>
    /// <b>Concurrency:</b> Not thread-safe. Concurrent calls for the same keyword may result in
    /// lost bucket assignments or incorrect bucket counts.
    /// </para>
    /// </remarks>
    /// <returns>A task representing the asynchronous indexing operation.</returns>
    public async Task AddIndex(TItemId itemId, string text)
    {
        var tokens = Tokenize(text);
        foreach (var token in tokens)
        {
            await AddIndexToken(itemId, token);
        }
    }

    /// <summary>
    /// Re-indexes an item by intelligently updating only the keywords that have changed between
    /// old and new text.
    /// </summary>
    /// <param name="itemId">The item identifier to reindex.</param>
    /// <param name="oldText">The previous text content that was indexed.</param>
    /// <param name="newText">The new text content to index.</param>
    /// <returns>A task representing the asynchronous reindexing operation.</returns>
    /// <remarks>
    /// <para>
    /// This method performs a differential update by calculating which keywords have been added,
    /// removed, or remained unchanged. Only modified keywords are updated in the index.
    /// </para>
    /// <para>
    /// Example: Changing "Buy coffee at Starbucks" to "Buy coffee at McDonald's" only modifies
    /// buckets for "starbucks" and "mcdonald" keywords, leaving "buy" and "coffee" untouched.
    /// </para>
    /// <para>
    /// <b>Performance:</b> Significantly more efficient than full deindex/reindex when text changes
    /// are localized. Uses set operations to identify differences.
    /// </para>
    /// <para>
    /// <b>Zero-knowledge advantage:</b> Modifying fewer buckets reduces observable server-side
    /// activity, providing better privacy than full reindexing.
    /// </para>
    /// </remarks>
    public async Task Reindex(TItemId itemId, string oldText, string newText)
    {
        var oldTokens = Tokenize(oldText);
        var newTokens = Tokenize(newText);

        // Only remove from keywords that no longer exist
        var tokensToRemove = oldTokens.Except(newTokens);
        foreach (var token in tokensToRemove)
        {
            await DeindexToken(itemId, token);
        }

        // Only add to new keywords that didn't exist before
        var tokensToAdd = newTokens.Except(oldTokens);
        foreach (var token in tokensToAdd)
        {
            await AddIndexToken(itemId, token);
        }

        // Keywords in both oldTokens and newTokens are left untouched (optimization!)
    }

    /// <summary>
    /// Removes an item from the search index by deleting it from all keyword buckets.
    /// </summary>
    /// <param name="itemId">The item identifier to remove from the index.</param>
    /// <param name="text">
    ///     The text content that was originally indexed. This is used to determine which keyword
    ///     buckets contain the item.
    /// </param>
    /// <returns>A task representing the asynchronous deindexing operation.</returns>
    /// <remarks>
    /// <para>
    /// This method tokenizes the text and removes the item ID from all corresponding keyword buckets.
    /// Buckets are only saved if the item was actually removed.
    /// </para>
    /// <para>
    /// <b>Performance:</b> Loads all buckets for each keyword, even though the item typically only
    /// appears once per keyword. For large bucket counts, this can be I/O intensive.
    /// </para>
    /// <para>
    /// <b>Cleanup:</b> Empty buckets are not removed to avoid revealing deletion patterns to the server
    /// (zero-knowledge consideration). This trades storage efficiency for privacy.
    /// </para>
    /// <para>
    /// <b>Important:</b> If the text parameter doesn't match what was originally indexed, the item
    /// will not be removed correctly. Ensure you pass the exact text that was used during indexing.
    /// </para>
    /// </remarks>
    public async Task Deindex(TItemId itemId, string text)
    {
        var tokens = Tokenize(text);
        foreach (var token in tokens)
        {
            await DeindexToken(itemId, token);
        }
    }

    /// <summary>
    /// Indexes a single token for an item by adding the item ID to the appropriate keyword bucket.
    /// </summary>
    /// <param name="itemId">The item identifier to index.</param>
    /// <param name="token">The token (keyword) to index the item under.</param>
    /// <returns>A task representing the asynchronous indexing operation.</returns>
    /// <remarks>
    /// This is a lower-level method used by <see cref="AddIndex"/> and <see cref="Reindex"/>.
    /// Creates new buckets as needed when existing buckets reach capacity.
    /// </remarks>
    private async Task AddIndexToken(TItemId itemId, string token)
    {
        var keyword = glossary.GetOrAddKeyword(token);

        SearchBucket<TItemId> bucket;
        if (keyword.NumBuckets == 0)
        {
            // No buckets exist yet, create the first one
            bucket = new SearchBucket<TItemId> { Id = new BucketId($"{keyword.Value}:0") };
            await glossary.SaveKeyword(keyword with { NumBuckets = 1 });
        }
        else
        {
            var lastBucketId = new BucketId($"{keyword.Value}:{keyword.NumBuckets - 1}");
            bucket = await bucketManager.LoadBucket(lastBucketId)
                ?? new SearchBucket<TItemId> { Id = lastBucketId };

            if (bucket.ItemIds.Count >= bucketManager.MaxItemsPerBucket)
            {
                bucket = new SearchBucket<TItemId> { Id = new BucketId($"{keyword.Value}:{keyword.NumBuckets}") };
                await glossary.SaveKeyword(keyword with { NumBuckets = keyword.NumBuckets + 1 });
            }
        }

        bucket.ItemIds.Add(itemId);
        await bucketManager.SaveBucket(bucket);
    }

    /// <summary>
    /// Removes a single token's association with an item by deleting the item ID from the
    /// keyword's buckets.
    /// </summary>
    /// <param name="itemId">The item identifier to remove.</param>
    /// <param name="token">The token (keyword) to deindex the item from.</param>
    /// <returns>A task representing the asynchronous deindexing operation.</returns>
    /// <remarks>
    /// This is a lower-level method used by <see cref="Deindex"/> and <see cref="Reindex"/>.
    /// Searches all buckets for the keyword and removes the item ID if found.
    /// </remarks>
    private async Task DeindexToken(TItemId itemId, string token)
    {
        var keyword = glossary.GetOrAddKeyword(token);
        if (keyword.NumBuckets == 0) return;

        var bucketIds = ListBucketIds([keyword]);
        foreach (var bucketId in bucketIds)
        {
            var bucket = await bucketManager.LoadBucket(bucketId);
            if (bucket is null) continue;

            if (bucket.ItemIds.Remove(itemId))
            {
                await bucketManager.SaveBucket(bucket);
            }
        }
    }

    /// <summary>
    /// Generates a list of bucket IDs for the given keywords in the specified traversal direction.
    /// </summary>
    /// <param name="keywords">The keywords to generate bucket IDs for.</param>
    /// <param name="direction">
    /// The direction to traverse buckets. Defaults to <see cref="SearchDirection.OldestFirst"/>.
    /// </param>
    /// <returns>A list of bucket IDs covering all buckets for all keywords.</returns>
    private static List<BucketId> ListBucketIds(
        List<SearchKeyword> keywords,
        SearchDirection direction = SearchDirection.LatestFirst
    )
    {
        var bucketIds = new List<BucketId>();
        foreach (var keyword in keywords)
        {
            if (keyword.NumBuckets == 0) continue;

            var currentIndex = direction == SearchDirection.LatestFirst
                ? (int)(keyword.NumBuckets - 1)
                : 0;

            while (currentIndex >= 0 && currentIndex < keyword.NumBuckets)
            {
                bucketIds.Add(new BucketId($"{keyword.Value}:{currentIndex}"));
                currentIndex += direction == SearchDirection.LatestFirst
                    ? -1
                    : 1;
            }
        }

        return bucketIds;
    }

    /// <summary>
    /// Searches the index for items matching the specified search terms.
    /// </summary>
    /// <param name="terms">
    /// Search terms to find. The terms are tokenized using the same logic as indexing, then
    /// matched against the glossary keywords client-side after decryption.
    /// </param>
    /// <param name="direction">
    /// Direction to traverse buckets. <see cref="SearchDirection.LatestFirst"/> returns most
    /// recently indexed items first; <see cref="SearchDirection.OldestFirst"/> returns oldest first.
    /// </param>
    /// <param name="fuzzy">
    /// When <c>true</c>, performs substring matching (keyword contains search token).
    /// When <c>false</c>, requires exact token matches. Fuzzy matching allows finding partial
    /// words like "mcdon" matching "McDonald's".
    /// </param>
    /// <returns>
    /// A list of unique item IDs that match any of the search terms. Results are deduplicated
    /// (items matching multiple keywords appear only once).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Search operates entirely client-side after fetching encrypted data:
    /// 1. Tokenizes search terms
    /// 2. Loads and decrypts the full glossary to find matching keywords
    /// 3. Fetches and decrypts matching keyword buckets
    /// 4. Aggregates and deduplicates item IDs
    /// </para>
    /// <para>
    /// <b>Performance:</b> For common keywords with many buckets, this may fetch significant data.
    /// Consider implementing pagination or result limits for production use.
    /// </para>
    /// <para>
    /// <b>OR semantics:</b> Returns items matching ANY of the search terms (not AND). Items that
    /// contain multiple terms are not ranked higher in the current implementation.
    /// </para>
    /// </remarks>
    public Task<List<TItemId>> Search(
        string terms,
        SearchDirection direction = SearchDirection.LatestFirst,
        bool fuzzy = true
    )
    {
        var keywords = GetKeywords(terms, fuzzy);
        return Search(keywords, direction);
    }

    /// <summary>
    /// Internal search implementation that fetches and aggregates buckets for the given keywords.
    /// </summary>
    private async Task<List<TItemId>> Search(List<SearchKeyword> keywords, SearchDirection direction)
    {
        var resultItemIds = new HashSet<TItemId>();

        foreach (var keyword in keywords)
        {
            if (keyword.NumBuckets == 0) continue;

            var currentIndex = direction == SearchDirection.LatestFirst
                ? (int)(keyword.NumBuckets - 1)
                : 0;

            while (currentIndex >= 0 && currentIndex < keyword.NumBuckets)
            {
                var bucketId = new BucketId($"{keyword.Value}:{currentIndex}");
                var bucket = await bucketManager.LoadBucket(bucketId);
                bucket?.ItemIds.ForEach(x => resultItemIds.Add(x));

                currentIndex += direction == SearchDirection.LatestFirst
                    ? -1
                    : 1;
            }
        }

        return resultItemIds.ToList();
    }

    /// <summary>
    /// Finds keywords in the glossary that match the tokenized search terms.
    /// This performs client-side matching against decrypted keywords.
    /// </summary>
    private List<SearchKeyword> GetKeywords(string terms, bool fuzzy)
    {
        var tokens = Tokenize(terms);
        var keywords = glossary.GetKeywords();
        return keywords
            .Where(k => tokens.Any(t => fuzzy
                ? k.Value.Contains(t)
                : k.Value == t))
            .ToList();
    }

    /// <summary>
    /// Tokenizes text into normalized search terms by splitting on whitespace and special characters,
    /// while preserving apostrophes, hyphens, and underscores within tokens, and converting to lowercase.
    /// </summary>
    /// <param name="text">Text to tokenize.</param>
    /// <returns>
    /// Set of lowercase tokens with whitespace and special characters removed. Apostrophes, hyphens,
    /// and underscores are preserved within tokens.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This tokenization strategy:
    /// 1. Splits on whitespace and special characters (!?@#$%^&amp;*()+=[]{}|;:,./&lt;&gt;)
    /// 2. Preserves apostrophes ('), hyphens (-), and underscores (_) within tokens
    /// 3. Converts to lowercase
    /// 4. Removes empty tokens
    /// </para>
    /// <para>
    /// Examples:
    /// - "McDonald's" → ["mcdonald's"]
    /// - "foo-bar" → ["foo-bar"]
    /// - "don't" → ["don't"]
    /// - "Test@123" → ["test", "123"]
    /// - "Hello, World!" → ["hello", "world"]
    /// - "user@example.com" → ["user", "example", "com"]
    /// - "COVID-19" → ["covid-19"]
    /// - "foo_bar" → ["foo_bar"]
    /// </para>
    /// <para>
    /// Returns a HashSet for efficient set operations during reindexing.
    /// </para>
    /// </remarks>
    private static HashSet<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        var currentToken = new System.Text.StringBuilder();

        foreach (var c in text)
        {
            // Check if character should be kept in tokens
            if (char.IsLetterOrDigit(c) || c == '\'' || c == '-' || c == '_')
            {
                currentToken.Append(c);
            }
            else if (currentToken.Length > 0)
            {
                // We hit a separator, save current token
                tokens.Add(currentToken.ToString());
                currentToken.Clear();
            }
            // If currentToken is empty and we hit a separator, just skip it
        }

        // Don't forget the last token if text doesn't end with a separator
        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens
            .Select(t => t.ToLowerInvariant())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToHashSet();
    }
}
