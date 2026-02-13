namespace Zylance.Vault.Remote.Search.BucketedSearch.Models;

/// <summary>
///     Tracks the indexing state of an individual item.
/// </summary>
/// <typeparam name="TItemId">The type of item identifier.</typeparam>
/// <remarks>
///     <para>
///         Flags prevent duplicate indexing and enable efficient batch operations by tracking
///         which items are currently in the search index.
///     </para>
///     <para>
///         When an item is added to the index, IsIndexed is set to true. When removed, it's set to false.
///         The flag entry persists even after removal to maintain historical tracking.
///     </para>
/// </remarks>
public record ItemFlags<TItemId>
    where TItemId : notnull
{
    /// <summary>
    ///     Gets the unique identifier for the item.
    /// </summary>
    public required TItemId ItemId { get; init; }

    /// <summary>
    ///     Gets a value indicating whether this item is currently indexed in the search engine.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>true - Item is indexed and searchable</item>
    ///         <item>false - Item is not indexed (never added, or was removed)</item>
    ///     </list>
    ///     Operations check this flag to determine whether to add, update, or skip items.
    /// </remarks>
    public bool IsIndexed { get; init; }
}
