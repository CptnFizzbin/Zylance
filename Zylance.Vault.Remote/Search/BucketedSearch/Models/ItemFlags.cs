namespace Zylance.Vault.Remote.Search.BucketedSearch.Models;

public record ItemFlags<TItemId> where TItemId : notnull
{
    public required TItemId ItemId { get; init; }
    public bool IsIndexed { get; init; }
}
