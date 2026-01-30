#nullable disable

namespace Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;

public interface IBucketedStorage<TItemId>
    where TItemId : notnull
{
    IGlossary Glossary { get; }
    IBucketManager<TItemId> Buckets { get; }
    IFlagManager<TItemId> Flags { get; }
}
