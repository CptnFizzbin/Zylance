using Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;
using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Tests.Search.BucketedSearch.Lib;

public class MemoryFlagManager<TItemId> : IFlagManager<TItemId>
    where TItemId : notnull
{
    private readonly Dictionary<TItemId, ItemFlags<TItemId>> _flags = new();

    public Task<ItemFlags<TItemId>> GetFlagAsync(TItemId itemId)
    {
        return Task.FromResult(_flags.GetValueOrDefault(itemId) ?? new ItemFlags<TItemId> { ItemId = itemId });
    }

    public Task<Dictionary<TItemId, ItemFlags<TItemId>>> GetFlagsAsync(List<TItemId> itemId)
    {
        return Task.FromResult(_flags);
    }

    public Task SaveFlagAsync(ItemFlags<TItemId> itemFlags)
    {
        _flags[itemFlags.ItemId] = itemFlags;
        return Task.CompletedTask;
    }

    public Task SaveFlagsAsync(List<ItemFlags<TItemId>> flags)
    {
        flags.ForEach(f => _flags[f.ItemId] = f);
        return Task.CompletedTask;
    }
}
