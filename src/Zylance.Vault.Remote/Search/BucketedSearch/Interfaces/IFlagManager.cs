#nullable disable

using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;

/// <summary>
///     Manages item flags that track which items are currently indexed in the search engine.
/// </summary>
/// <typeparam name="TItemId">The type of item identifier.</typeparam>
/// <remarks>
///     <para>
///         Flags serve two critical purposes:
///         <list type="number">
///             <item>Prevent duplicate indexing - calling AddItem on an already-indexed item is a no-op</item>
///             <item>Enable efficient batch operations - quickly identify which items need processing</item>
///         </list>
///     </para>
///     <para>
///         In zero-knowledge implementations, flag data should be encrypted to prevent the storage
///         layer from knowing which items are indexed.
///     </para>
/// </remarks>
public interface IFlagManager<TItemId>
    where TItemId : notnull
{
    /// <summary>
    ///     Retrieves the indexing flag for a specific item.
    /// </summary>
    /// <param name="itemId">The item identifier to look up.</param>
    /// <returns>
    ///     The item's flag state. If the item has never been indexed, returns a new flag
    ///     with IsIndexed = false.
    /// </returns>
    /// <remarks>
    ///     This method should never return null. For items without existing flags, it returns
    ///     a default ItemFlags object to simplify indexing logic.
    /// </remarks>
    Task<ItemFlags<TItemId>> GetFlagAsync(TItemId itemId);

    /// <summary>
    ///     Retrieves flags for multiple items in a batch operation.
    /// </summary>
    /// <param name="itemId">List of item identifiers to look up.</param>
    /// <returns>
    ///     A dictionary mapping item IDs to their flag states. Items without existing flags
    ///     are not included in the dictionary.
    /// </returns>
    /// <remarks>
    ///     This is more efficient than calling <see cref="GetFlagAsync" /> multiple times
    ///     as it allows for batch retrieval from storage.
    /// </remarks>
    Task<Dictionary<TItemId, ItemFlags<TItemId>>> GetFlagsAsync(List<TItemId> itemId);

    /// <summary>
    ///     Saves or updates a single item's flag state.
    /// </summary>
    /// <param name="itemFlags">The flag to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <remarks>
    ///     Implementations should use upsert semantics (insert or update based on ItemId).
    /// </remarks>
    Task SaveFlagAsync(ItemFlags<TItemId> itemFlags);

    /// <summary>
    ///     Saves or updates multiple item flags in a batch operation.
    /// </summary>
    /// <param name="flags">List of flags to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <remarks>
    ///     This is more efficient than calling <see cref="SaveFlagAsync" /> multiple times
    ///     as it allows for batch persistence to storage.
    /// </remarks>
    Task SaveFlagsAsync(List<ItemFlags<TItemId>> flags);
}
