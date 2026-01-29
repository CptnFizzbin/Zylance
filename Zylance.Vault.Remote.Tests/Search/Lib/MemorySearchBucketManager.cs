using Zylance.Vault.Remote.Search;

namespace Zylance.Vault.Remote.Tests.Search.Lib;

public class MemorySearchBucketManager<TItemId> : ISearchBucketManager<TItemId>
{
    private readonly Dictionary<BucketId, SearchBucket<TItemId>> _buckets = new();

    public required uint MaxItemsPerBucket { get; init; }

    public Task<SearchBucket<TItemId>?> LoadBucket(BucketId bucketId)
    {
        return _buckets.TryGetValue(bucketId, out var bucket)
            ? Task.FromResult<SearchBucket<TItemId>?>(bucket)
            : Task.FromResult<SearchBucket<TItemId>?>(null);
    }

    public Task SaveBucket(SearchBucket<TItemId> bucket)
    {
        _buckets[bucket.Id] = bucket;
        return Task.CompletedTask;
    }
}
