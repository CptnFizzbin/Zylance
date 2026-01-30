using Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;
using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Tests.Search.BucketedSearch.Lib;

public class MemoryBucketManager<TItemId> : IBucketManager<TItemId>
{
    private readonly Dictionary<BucketId, Bucket<TItemId>> _buckets = new();

    public uint MaxItemsPerBucket { get; init; } = 100;

    public Task<Bucket<TItemId>?> LoadBucket(BucketId bucketId)
    {
        return _buckets.TryGetValue(bucketId, out var bucket)
            ? Task.FromResult<Bucket<TItemId>?>(bucket)
            : Task.FromResult<Bucket<TItemId>?>(null);
    }

    public Task SaveBucket(Bucket<TItemId> bucket)
    {
        _buckets[bucket.Id] = bucket;
        return Task.CompletedTask;
    }
}
