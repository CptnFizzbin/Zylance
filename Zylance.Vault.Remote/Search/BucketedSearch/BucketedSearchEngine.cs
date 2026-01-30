using System.Text.RegularExpressions;
using Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;
using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Search.BucketedSearch;

public partial class BucketedSearchEngine<TItemId>(IBucketedStorage<TItemId> storage)
    : IZkSearchEngine<TItemId> where TItemId : notnull
{
    public async Task AddItemAsync(TItemId itemId, string content)
    {
        var itemFlags = await storage.Flags.GetFlagAsync(itemId);
        if (itemFlags.IsIndexed) return;

        await IndexItem(itemId, content);

        await storage.Flags.SaveFlagAsync(itemFlags with { IsIndexed = true });
    }

    public async Task AddItemsAsync(List<(TItemId itemId, string content)> items)
    {
        var itemIds = items.Select(i => i.itemId).ToList();
        var itemFlags = await storage.Flags.GetFlagsAsync(itemIds);

        var indexedItemIds = itemIds
            .Where(id => itemFlags.GetValueOrDefault(id)?.IsIndexed ?? false)
            .ToHashSet();

        var itemsToIndex = items
            .Where(item => !indexedItemIds.Contains(item.itemId))
            .ToList();

        foreach (var item in itemsToIndex)
            await IndexItem(item.itemId, item.content);

        var updatedFlags = itemIds
            .Where(id => !indexedItemIds.Contains(id))
            .Select(id => itemFlags.GetValueOrDefault(id) is { } existingFlags
                ? existingFlags with { IsIndexed = true }
                : new ItemFlags<TItemId> { ItemId = id, IsIndexed = true })
            .ToList();

        await storage.Flags.SaveFlagsAsync(updatedFlags);
    }

    public async Task UpdateItemAsync(TItemId itemId, string oldContent, string content)
    {
        if (!(await storage.Flags.GetFlagAsync(itemId)).IsIndexed) return;

        await ReindexItem(itemId, oldContent, content);
    }

    public async Task UpdateItemsAsync(List<(TItemId itemId, string oldContent, string newContent)> items)
    {
        var itemIds = items.Select(i => i.itemId).ToList();
        var itemFlags = await storage.Flags.GetFlagsAsync(itemIds);

        var indexedItemIds = itemIds
            .Where(id => itemFlags.GetValueOrDefault(id)?.IsIndexed ?? false)
            .ToHashSet();

        var itemsToReindex = items
            .Where(item => indexedItemIds.Contains(item.itemId))
            .ToList();

        foreach (var item in itemsToReindex)
            await ReindexItem(item.itemId, item.oldContent, item.newContent);
    }

    public async Task RemoveItemAsync(TItemId itemId, string content)
    {
        var itemFlags = await storage.Flags.GetFlagAsync(itemId);
        if (!itemFlags.IsIndexed) return;

        await DeindexItem(itemId, content);

        await storage.Flags.SaveFlagAsync(itemFlags with { IsIndexed = false });
    }

    public async Task RemoveItemsAsync(List<(TItemId itemId, string content)> items)
    {
        var itemIds = items.Select(i => i.itemId).ToList();
        var itemFlags = await storage.Flags.GetFlagsAsync(itemIds);

        var indexedItemIds = itemIds
            .Where(id => itemFlags.GetValueOrDefault(id)?.IsIndexed ?? false)
            .ToHashSet();

        var itemsToDeindex = items
            .Where(item => indexedItemIds.Contains(item.itemId))
            .ToList();

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
        if (searchTokens.Count == 0) return [];

        HashSet<TItemId>? resultItemIds = null;

        // For each search token, find all matching keywords and union their items
        foreach (var searchToken in searchTokens)
        {
            var matchingKeywords = storage.Glossary
                .GetAll()
                .Where(k => fuzzy
                    ? k.Value.Contains(searchToken)
                    : k.Value == searchToken)
                .ToList();

            var itemsForThisToken = new HashSet<TItemId>();
            foreach (var keyword in matchingKeywords)
            {
                var keywordItems = await GetItemsForKeyword(keyword, direction);
                foreach (var item in keywordItems) itemsForThisToken.Add(item);
            }

            resultItemIds = resultItemIds is null
                ? itemsForThisToken
                : resultItemIds.Intersect(itemsForThisToken).ToHashSet();

            // Early exit: if any token yields no results, the final result is empty
            if (resultItemIds.Count == 0) break;
        }

        return resultItemIds?.ToList() ?? [];
    }

    private async Task IndexItem(TItemId itemId, string text)
    {
        await Task.WhenAll(Tokenize(text).Select(token => IndexToken(itemId, token)));
    }

    private async Task ReindexItem(TItemId itemId, string oldText, string newText)
    {
        var oldTokens = Tokenize(oldText);
        var newTokens = Tokenize(newText);

        foreach (var token in oldTokens.Except(newTokens))
            await DeindexToken(itemId, token);

        foreach (var token in newTokens.Except(oldTokens))
            await IndexToken(itemId, token);
    }

    private async Task DeindexItem(TItemId itemId, string text)
    {
        await Task.WhenAll(Tokenize(text).Select(token => DeindexToken(itemId, token)));
    }

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
            bucket = await storage.Buckets.LoadBucket(lastBucketId)
                ?? new Bucket<TItemId> { Id = lastBucketId };

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

    private async Task DeindexToken(TItemId itemId, string token)
    {
        var keyword = storage.Glossary.Get(token);
        if (keyword.NumBuckets == 0) return;

        var bucketIds = ListBucketIds([keyword]);
        foreach (var bucketId in bucketIds)
        {
            var bucket = await storage.Buckets.LoadBucket(bucketId);
            if (bucket is null) continue;

            if (bucket.ItemIds.Remove(itemId))
                await storage.Buckets.SaveBucket(bucket);
        }
    }

    private static List<BucketId> ListBucketIds(
        List<Keyword> keywords,
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

    private async Task<HashSet<TItemId>> GetItemsForKeyword(
        Keyword keyword,
        SearchDirection direction = SearchDirection.LatestFirst
    )
    {
        var resultItemIds = new HashSet<TItemId>();
        if (keyword.NumBuckets == 0) return resultItemIds;

        var currentIndex = direction == SearchDirection.LatestFirst
            ? (int)(keyword.NumBuckets - 1)
            : 0;

        while (currentIndex >= 0 && currentIndex < keyword.NumBuckets)
        {
            var bucketId = new BucketId($"{keyword.Value}:{currentIndex}");
            var bucket = await storage.Buckets.LoadBucket(bucketId);
            bucket?.ItemIds.ToList().ForEach(x => resultItemIds.Add(x));

            currentIndex += direction == SearchDirection.LatestFirst
                ? -1
                : 1;
        }

        return resultItemIds;
    }


    private static HashSet<string> Tokenize(string text)
    {
        return TokenBoundaryRegex().Split(text)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.ToLowerInvariant())
            .ToHashSet();
    }

    [GeneratedRegex(@"\W+")]
    private static partial Regex TokenBoundaryRegex();
}
