using System.Text.RegularExpressions;
using Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;
using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Search.BucketedSearch;

/// <summary>
///     A zero-knowledge search engine implementation that uses bucketed storage to partition
///     the inverted index and prevent unbounded growth.
/// </summary>
/// <typeparam name="TItemId">
///     The type of item identifier. In zero-knowledge scenarios, should be encrypted or hashed
///     to prevent correlation by the storage layer.
/// </typeparam>
/// <remarks>
///     <para>
///         <b>Design Assumptions:</b>
///         <list type="bullet">
///             <item>
///                 <b>Chronological Indexing:</b> Items are added to the index in the same order they're created.
///                 This assumption enables <see cref="SearchDirection.LatestFirst"/> to return more recently
///                 created items first by traversing buckets from newest to oldest. For finance applications,
///                 this means recent transactions naturally appear at the top of search results.
///             </item>
///             <item>
///                 <b>Rare Reindexing:</b> Content updates (reindexing) are infrequent compared to new item additions.
///                 The differential reindexing algorithm in <see cref="ReindexItem"/> is optimized for this case,
///                 only updating changed tokens rather than removing and re-adding all tokens. Frequent updates
///                 to the same items may impact performance.
///             </item>
///             <item>
///                 <b>Low Keyword Cardinality:</b> The number of unique keywords is substantially less than the
///                 number of indexed items. This allows the entire glossary to be loaded into memory for fuzzy
///                 search operations. Typical finance applications might have 1,000-10,000 unique keywords
///                 across millions of transactions, making client-side fuzzy matching feasible.
///             </item>
///         </list>
///     </para>
///     <para>
///         <b>Zero-Knowledge Guarantees:</b>
///         <list type="bullet">
///             <item>All tokenization happens client-side</item>
///             <item>Storage layer receives only encrypted keywords, bucket IDs, and item IDs</item>
///             <item>Fuzzy matching performed client-side after retrieving encrypted glossary</item>
///             <item>Server cannot determine what's being searched or what content is indexed</item>
///         </list>
///     </para>
///     <para>
///         <b>Thread Safety:</b> This class is NOT thread-safe. Concurrent write operations on the same keyword may result in
///         race conditions where bucket updates are lost (last write wins). For multi-user scenarios,
///         implement optimistic concurrency control in the storage layer.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Create storage implementation
/// var storage = new MyBucketedStorage();
/// var searchEngine = new BucketedSearchEngine&lt;string&gt;(storage);
///
/// // Index some transactions
/// await searchEngine.AddItemAsync("txn1", "Coffee at Starbucks $5.50");
/// await searchEngine.AddItemAsync("txn2", "Coffee payment pending");
/// await searchEngine.AddItemAsync("txn3", "Monthly rent payment");
///
/// // Search for transactions (AND semantics)
/// var results = await searchEngine.SearchAsync("coffee payment");
/// // Returns: ["txn2"] - only item matching BOTH terms
///
/// // Fuzzy search
/// var fuzzyResults = await searchEngine.SearchAsync("coff pay", fuzzy: true);
/// // Returns: ["txn2"] - substring matches
/// </code>
/// </example>
public partial class BucketedSearchEngine<TItemId>(IBucketedStorage<TItemId> storage) : IZkSearchEngine<TItemId>
    where TItemId : notnull
{
    public async Task AddItemAsync(TItemId itemId, string content)
    {
        var itemFlags = await storage.Flags.GetFlagAsync(itemId);
        if (itemFlags.IsIndexed)
            return;

        await IndexItem(itemId, content);

        await storage.Flags.SaveFlagAsync(itemFlags with { IsIndexed = true });
    }

    public async Task AddItemsAsync(List<(TItemId itemId, string content)> items)
    {
        var itemIds = items.Select(i => i.itemId).ToList();
        var itemFlags = await storage.Flags.GetFlagsAsync(itemIds);

        var indexedItemIds = itemIds.Where(id => itemFlags.GetValueOrDefault(id)?.IsIndexed ?? false).ToHashSet();

        var itemsToIndex = items.Where(item => !indexedItemIds.Contains(item.itemId)).ToList();

        foreach (var item in itemsToIndex)
            await IndexItem(item.itemId, item.content);

        var updatedFlags = itemIds
            .Where(id => !indexedItemIds.Contains(id))
            .Select(id =>
                itemFlags.GetValueOrDefault(id) is { } existingFlags
                    ? existingFlags with
                    {
                        IsIndexed = true,
                    }
                    : new ItemFlags<TItemId> { ItemId = id, IsIndexed = true }
            )
            .ToList();

        await storage.Flags.SaveFlagsAsync(updatedFlags);
    }

    public async Task UpdateItemAsync(TItemId itemId, string oldContent, string content)
    {
        if (!(await storage.Flags.GetFlagAsync(itemId)).IsIndexed)
            return;

        await ReindexItem(itemId, oldContent, content);
    }

    public async Task UpdateItemsAsync(List<(TItemId itemId, string oldContent, string newContent)> items)
    {
        var itemIds = items.Select(i => i.itemId).ToList();
        var itemFlags = await storage.Flags.GetFlagsAsync(itemIds);

        var indexedItemIds = itemIds.Where(id => itemFlags.GetValueOrDefault(id)?.IsIndexed ?? false).ToHashSet();

        var itemsToReindex = items.Where(item => indexedItemIds.Contains(item.itemId)).ToList();

        foreach (var item in itemsToReindex)
            await ReindexItem(item.itemId, item.oldContent, item.newContent);
    }

    public async Task RemoveItemAsync(TItemId itemId, string content)
    {
        var itemFlags = await storage.Flags.GetFlagAsync(itemId);
        if (!itemFlags.IsIndexed)
            return;

        await DeindexItem(itemId, content);

        await storage.Flags.SaveFlagAsync(itemFlags with { IsIndexed = false });
    }

    public async Task RemoveItemsAsync(List<(TItemId itemId, string content)> items)
    {
        var itemIds = items.Select(i => i.itemId).ToList();
        var itemFlags = await storage.Flags.GetFlagsAsync(itemIds);

        var indexedItemIds = itemIds.Where(id => itemFlags.GetValueOrDefault(id)?.IsIndexed ?? false).ToHashSet();

        var itemsToDeindex = items.Where(item => indexedItemIds.Contains(item.itemId)).ToList();

        foreach (var item in itemsToDeindex)
            await DeindexItem(item.itemId, item.content);

        var updatedFlags = itemIds
            .Where(id => indexedItemIds.Contains(id))
            .Select(id => itemFlags[id] with { IsIndexed = false })
            .ToList();

        await storage.Flags.SaveFlagsAsync(updatedFlags);
    }

    public async Task<List<TItemId>> SearchAsync(
        string terms,
        SearchDirection direction = SearchDirection.LatestFirst,
        bool fuzzy = true
    )
    {
        var searchTokens = Tokenize(terms);
        if (searchTokens.Count == 0)
            return [];

        HashSet<TItemId>? resultItemIds = null;

        // For each search token, find all matching keywords and union their items
        foreach (var searchToken in searchTokens)
        {
            var matchingKeywords = storage
                .Glossary.GetAll()
                .Where(k => fuzzy ? k.Value.Contains(searchToken) : k.Value == searchToken)
                .ToList();

            var itemsForThisToken = new HashSet<TItemId>();
            foreach (var keyword in matchingKeywords)
            {
                var keywordItems = await GetItemsForKeyword(keyword, direction);
                foreach (var item in keywordItems)
                    itemsForThisToken.Add(item);
            }

            resultItemIds = resultItemIds is null
                ? itemsForThisToken
                : resultItemIds.Intersect(itemsForThisToken).ToHashSet();

            // Early exit: if any token yields no results, the final result is empty
            if (resultItemIds.Count == 0)
                break;
        }

        return resultItemIds?.ToList() ?? [];
    }

    /// <summary>
    ///     Indexes all tokens from the provided text for the specified item.
    /// </summary>
    /// <param name="itemId">The item identifier to index.</param>
    /// <param name="text">The text content to tokenize and index.</param>
    /// <remarks>
    ///     Tokenizes the text and indexes each unique token in parallel. Each token
    ///     is added to the appropriate keyword bucket.
    /// </remarks>
    private async Task IndexItem(TItemId itemId, string text)
    {
        await Task.WhenAll(Tokenize(text).Select(token => IndexToken(itemId, token)));
    }

    /// <summary>
    ///     Efficiently reindexes an item by only updating changed tokens.
    /// </summary>
    /// <param name="itemId">The item identifier to reindex.</param>
    /// <param name="oldText">The previous content (to remove old tokens).</param>
    /// <param name="newText">The new content (to add new tokens).</param>
    /// <remarks>
    ///     <para>
    ///         This method performs differential indexing:
    ///         <list type="bullet">
    ///             <item>Removes tokens that appear in old but not new content</item>
    ///             <item>Adds tokens that appear in new but not old content</item>
    ///             <item>Leaves unchanged tokens untouched</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         This is more efficient than calling DeindexItem + IndexItem when only
    ///         a few tokens have changed.
    ///     </para>
    /// </remarks>
    private async Task ReindexItem(TItemId itemId, string oldText, string newText)
    {
        var oldTokens = Tokenize(oldText);
        var newTokens = Tokenize(newText);

        foreach (var token in oldTokens.Except(newTokens))
            await DeindexToken(itemId, token);

        foreach (var token in newTokens.Except(oldTokens))
            await IndexToken(itemId, token);
    }

    /// <summary>
    ///     Removes all tokens for the specified item from the index.
    /// </summary>
    /// <param name="itemId">The item identifier to deindex.</param>
    /// <param name="text">The text content to tokenize and remove from index.</param>
    /// <remarks>
    ///     Tokenizes the text and removes the item ID from all corresponding keyword buckets.
    /// </remarks>
    private async Task DeindexItem(TItemId itemId, string text)
    {
        await Task.WhenAll(Tokenize(text).Select(token => DeindexToken(itemId, token)));
    }

    /// <summary>
    ///     Indexes a single token for an item by adding it to the appropriate bucket.
    /// </summary>
    /// <param name="itemId">The item identifier to index.</param>
    /// <param name="token">The normalized token (lowercase, no punctuation).</param>
    /// <remarks>
    ///     <para>
    ///         This method:
    ///         <list type="number">
    ///             <item>Looks up or creates keyword metadata in the glossary</item>
    ///             <item>Finds the last bucket for this keyword (or creates first bucket)</item>
    ///             <item>If the bucket is full, creates a new bucket and increments NumBuckets</item>
    ///             <item>Adds the item ID to the bucket (with duplicate check)</item>
    ///             <item>Saves the bucket back to storage</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Duplicate item IDs are prevented by checking if the item is already in the bucket
    ///         before adding.
    ///     </para>
    /// </remarks>
    private async Task IndexToken(TItemId itemId, string token)
    {
        var keyword = storage.Glossary.Get(token);

        Bucket<TItemId> bucket;
        if (keyword.NumBuckets == 0)
        {
            bucket = new Bucket<TItemId> { Id = new BucketId($"{keyword.Value}:0") };
            await storage.Glossary.Save(keyword with { NumBuckets = 1 });
        }
        else
        {
            var lastBucketId = new BucketId($"{keyword.Value}:{keyword.NumBuckets - 1}");
            bucket = await storage.Buckets.LoadBucket(lastBucketId) ?? new Bucket<TItemId> { Id = lastBucketId };

            if (bucket.ItemIds.Count >= storage.Buckets.MaxItemsPerBucket)
            {
                bucket = new Bucket<TItemId> { Id = new BucketId($"{keyword.Value}:{keyword.NumBuckets}") };
                await storage.Glossary.Save(keyword with { NumBuckets = keyword.NumBuckets + 1 });
            }
        }

        if (!bucket.ItemIds.Contains(itemId))
        {
            bucket.ItemIds.Add(itemId);
            await storage.Buckets.SaveBucket(bucket);
        }
    }

    /// <summary>
    ///     Removes a single token for an item from all buckets containing it.
    /// </summary>
    /// <param name="itemId">The item identifier to remove.</param>
    /// <param name="token">The normalized token to remove.</param>
    /// <remarks>
    ///     <para>
    ///         This method searches through ALL buckets for the keyword and removes the item ID
    ///         from any bucket that contains it. This is necessary because we don't track which
    ///         specific bucket an item is in (only which keyword).
    ///     </para>
    ///     <para>
    ///         For keywords with many buckets, this can be slow. Consider tracking bucket
    ///         assignments per item for better deindexing performance.
    ///     </para>
    /// </remarks>
    private async Task DeindexToken(TItemId itemId, string token)
    {
        var keyword = storage.Glossary.Get(token);
        if (keyword.NumBuckets == 0)
            return;

        var bucketIds = ListBucketIds([keyword]);
        foreach (var bucketId in bucketIds)
        {
            var bucket = await storage.Buckets.LoadBucket(bucketId);
            if (bucket is null)
                continue;

            if (bucket.ItemIds.Remove(itemId))
                await storage.Buckets.SaveBucket(bucket);
        }
    }

    /// <summary>
    ///     Retrieves all item IDs for a specific keyword across all its buckets.
    /// </summary>
    /// <param name="keyword">The keyword metadata containing bucket count.</param>
    /// <param name="direction">The direction to traverse buckets.</param>
    /// <returns>A hash set of all item IDs indexed under this keyword.</returns>
    /// <remarks>
    ///     Loads all buckets for the keyword (from 0 to NumBuckets-1) and unions
    ///     their item IDs. The traversal direction affects the order buckets are loaded
    ///     but doesn't affect the final result set (a HashSet has no order).
    /// </remarks>
    private async Task<HashSet<TItemId>> GetItemsForKeyword(
        Keyword keyword,
        SearchDirection direction = SearchDirection.LatestFirst
    )
    {
        var resultItemIds = new HashSet<TItemId>();
        if (keyword.NumBuckets == 0)
            return resultItemIds;

        var currentIndex = direction == SearchDirection.LatestFirst ? (int)(keyword.NumBuckets - 1) : 0;

        while (currentIndex >= 0 && currentIndex < keyword.NumBuckets)
        {
            var bucketId = new BucketId($"{keyword.Value}:{currentIndex}");
            var bucket = await storage.Buckets.LoadBucket(bucketId);
            bucket?.ItemIds.ToList().ForEach(x => resultItemIds.Add(x));

            currentIndex += direction == SearchDirection.LatestFirst ? -1 : 1;
        }

        return resultItemIds;
    }

    /// <summary>
    ///     Generates a list of bucket IDs for multiple keywords in the specified traversal direction.
    /// </summary>
    /// <param name="keywords">The list of keywords to generate bucket IDs for.</param>
    /// <param name="direction">
    ///     The direction to traverse buckets. <see cref="SearchDirection.LatestFirst"/> generates IDs
    ///     from highest to lowest bucket index; <see cref="SearchDirection.OldestFirst"/> generates from
    ///     lowest to highest.
    /// </param>
    /// <returns>A list of bucket IDs for all keywords in the specified order.</returns>
    /// <remarks>
    ///     <para>
    ///         For each keyword, this method generates bucket IDs from 0 to NumBuckets-1 in the order
    ///         specified by <paramref name="direction"/>. Keywords with NumBuckets = 0 are skipped.
    ///     </para>
    ///     <para>
    ///         Example: For keyword "coffee" with NumBuckets = 3 and LatestFirst direction:
    ///         Returns ["coffee:2", "coffee:1", "coffee:0"]
    ///     </para>
    ///     <para>
    ///         This method is primarily used by <see cref="DeindexToken"/> to determine which buckets
    ///         to search when removing an item from the index.
    ///     </para>
    /// </remarks>
    private static List<BucketId> ListBucketIds(
        List<Keyword> keywords,
        SearchDirection direction = SearchDirection.LatestFirst
    )
    {
        var bucketIds = new List<BucketId>();
        foreach (var keyword in keywords)
        {
            if (keyword.NumBuckets == 0)
                continue;

            var currentIndex = direction == SearchDirection.LatestFirst ? (int)(keyword.NumBuckets - 1) : 0;

            while (currentIndex >= 0 && currentIndex < keyword.NumBuckets)
            {
                bucketIds.Add(new BucketId($"{keyword.Value}:{currentIndex}"));
                currentIndex += direction == SearchDirection.LatestFirst ? -1 : 1;
            }
        }

        return bucketIds;
    }

    /// <summary>
    ///     Tokenizes text into normalized search tokens.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <returns>A hash set of unique lowercase tokens.</returns>
    /// <remarks>
    ///     <para>
    ///         Tokenization rules:
    ///         <list type="bullet">
    ///             <item>Splits on whitespace and most punctuation</item>
    ///             <item>Preserves hyphens, underscores, and @ symbols within tokens</item>
    ///             <item>Removes empty or whitespace-only tokens</item>
    ///             <item>Converts all tokens to lowercase</item>
    ///             <item>Returns unique tokens (duplicates removed)</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Examples:
    ///         <list type="bullet">
    ///             <item>"Hello, World!" → ["hello", "world"]</item>
    ///             <item>"foo-bar" → ["foo-bar"]</item>
    ///             <item>"user@example.com" → ["user@example", "com"]</item>
    ///             <item>"check_status" → ["check_status"]</item>
    ///             <item>"Order #12345" → ["order", "12345"]</item>
    ///         </list>
    ///     </para>
    /// </remarks>
    private static HashSet<string> Tokenize(string text)
    {
        return TokenBoundaryRegex()
            .Split(text)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.ToLowerInvariant())
            .ToHashSet();
    }

    /// <summary>
    ///     Gets the compiled regular expression for splitting text into tokens.
    /// </summary>
    /// <returns>
    ///     A regex that matches sequences of characters that should be treated as token delimiters,
    ///     excluding hyphens, underscores, and @ symbols.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Uses source-generated regex for performance. The pattern matches any character that is NOT:
    ///         <list type="bullet">
    ///             <item>A letter (a-z, A-Z)</item>
    ///             <item>A digit (0-9)</item>
    ///             <item>A hyphen (-)</item>
    ///             <item>An underscore (_)</item>
    ///             <item>An @ symbol (@)</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         This allows tokens like "foo-bar", "user@domain", and "check_status" to remain intact
    ///         while still splitting on spaces, punctuation, and other special characters.
    ///     </para>
    /// </remarks>
    [GeneratedRegex(@"[^a-zA-Z0-9\-_@]+")]
    private static partial Regex TokenBoundaryRegex();
}
