#nullable disable

using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;

public interface IFlagManager<TItemId>
    where TItemId : notnull
{
    Task<ItemFlags<TItemId>> GetFlagAsync(TItemId itemId);
    Task<Dictionary<TItemId, ItemFlags<TItemId>>> GetFlagsAsync(List<TItemId> itemId);

    Task SaveFlagAsync(ItemFlags<TItemId> itemFlags);
    Task SaveFlagsAsync(List<ItemFlags<TItemId>> flags);
}
