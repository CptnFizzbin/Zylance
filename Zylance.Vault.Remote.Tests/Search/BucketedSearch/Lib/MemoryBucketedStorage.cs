using Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;

namespace Zylance.Vault.Remote.Tests.Search.BucketedSearch.Lib;

public class MemoryBucketedStorage<TItemId>(uint maxItemsPerBucket = 100)
    : IBucketedStorage<TItemId> where TItemId : notnull
{
    public IBucketManager<TItemId> Buckets { get; } = new MemoryBucketManager<TItemId>
        { MaxItemsPerBucket = maxItemsPerBucket };

    public IGlossary Glossary { get; } = new MemoryGlossary();

    public IFlagManager<TItemId> Flags { get; } = new MemoryFlagManager<TItemId>();
}
