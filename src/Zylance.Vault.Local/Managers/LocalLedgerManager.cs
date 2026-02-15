using Microsoft.EntityFrameworkCore;
using Zylance.Contract.Api.Ledger;
using Zylance.Core.Vault.Managers;
using Zylance.Core.Vault.Models;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Managers;

/// <summary>
///     Local implementation of ILedgerManager using Entity Framework Core.
/// </summary>
public class LocalLedgerManager(LocalVaultDbContext dbContext) : ILedgerManager
{
    /// <summary>
    ///     Lists ledger entries with optional filtering and pagination.
    ///     Why use LINQ queries? EF Core translates LINQ expressions into efficient
    ///     SQL queries,
    ///     allowing us to work with strongly-typed C# expressions while getting
    ///     database-level
    ///     performance benefits like filtering and sorting before loading data into
    ///     memory.
    /// </summary>
    /// <param name="filter">Optional filter criteria for pagination and filtering</param>
    /// <returns>A cursor-based paginated list of ledger entries</returns>
    public async Task<CursorList<LedgerEntryModel>> ListAsync(LedgerFilter? filter)
    {
        var query = ApplyFilter(dbContext.LedgerEntries.AsQueryable(), filter);

        query = query.OrderByDescending(e => e.Timestamp).ThenByDescending(e => e.Id);

        var totalCount = (ulong)await query.CountAsync();

        var pageSize =
            filter is not null && filter.PageSize > 0
                ? Math.Min((int)filter.PageSize, LedgerCursor.MaxPageSize)
                : LedgerCursor.DefaultPageSize;

        var entities = await query.Take(pageSize + 1).ToListAsync();

        var hasNextPage = entities.Count > pageSize;
        if (hasNextPage)
            entities = entities.Take(pageSize).ToList();

        var items = entities.Select(LedgerEntryEntity.ToModel).ToList();

        var nextCursor = string.Empty;
        if (!hasNextPage || items.Count <= 0)
            return new CursorList<LedgerEntryModel>
            {
                Cursor = nextCursor,
                TotalCount = totalCount,
                Items = items,
            };

        var lastEntry = entities[^1];
        nextCursor = new LedgerCursor { Timestamp = lastEntry.Timestamp, Id = lastEntry.Id }.Encode();

        return new CursorList<LedgerEntryModel>
        {
            Cursor = nextCursor,
            TotalCount = totalCount,
            Items = items,
        };
    }

    /// <summary>
    ///     Gets a ledger entry by its ID.
    /// </summary>
    /// <param name="recordId">The ledger entry ID</param>
    /// <returns>The ledger entry data</returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when the ledger entry is not
    ///     found
    /// </exception>
    public async Task<LedgerEntryModel> GetAsync(Guid recordId)
    {
        var entity = await dbContext.LedgerEntries.FindAsync(recordId);
        return entity is null
            ? throw new KeyNotFoundException($"Ledger entry with ID {recordId} not found")
            : LedgerEntryEntity.ToModel(entity);
    }

    /// <summary>
    ///     Returns all ledger entries in the database as models.
    /// </summary>
    /// <returns>All ledger entries as models.</returns>
    public Task<List<LedgerEntryModel>> GetAllAsync()
    {
        return Task.FromResult(dbContext.LedgerEntries.Select(LedgerEntryEntity.ToModel).ToList());
    }

    /// <summary>
    ///     Deletes a ledger entry by its ID.
    /// </summary>
    /// <param name="recordId">The ledger entry ID to delete</param>
    /// <returns>The deleted ledger entry data</returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when the ledger entry is not
    ///     found
    /// </exception>
    public async Task<LedgerEntryModel> DeleteAsync(Guid recordId)
    {
        var entity =
            await dbContext.LedgerEntries.FindAsync(recordId)
            ?? throw new KeyNotFoundException($"Ledger entry with ID {recordId} not found");

        dbContext.LedgerEntries.Remove(entity);
        await dbContext.SaveChangesAsync();
        return LedgerEntryEntity.ToModel(entity);
    }

    /// <summary>
    ///     Deletes each ledger entry in the provided list by ID and returns the
    ///     deleted entries.
    /// </summary>
    /// <param name="records">The ledger entries to delete.</param>
    /// <returns>The deleted ledger entries.</returns>
    public async Task<List<LedgerEntryModel>> DeleteAsync(List<LedgerEntryModel> records)
    {
        // Deletes each ledger entry in the provided list by ID and returns the deleted entries.
        var deletedEntries = new List<LedgerEntryModel>();
        foreach (var record in records)
        {
            var entity = await DeleteAsync(record.Id);
            deletedEntries.Add(entity);
        }

        return deletedEntries;
    }

    /// <summary>
    ///     Searches ledger entries with text search and optional filtering.
    /// </summary>
    /// <param name="searchText">The text to search for in payee and memo fields</param>
    /// <param name="filter">Optional filter criteria for pagination and filtering</param>
    /// <returns>A cursor-based paginated list of ledger entries matching the search</returns>
    public async Task<CursorList<LedgerEntryModel>> SearchAsync(string searchText, LedgerFilter? filter)
    {
        var query = ApplyFilter(dbContext.LedgerEntries.AsQueryable(), filter);

        // Apply text search to payee and memo fields
        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(e => e.Payee.Contains(searchText) || e.Memo.Contains(searchText));

        query = query.OrderByDescending(e => e.Timestamp).ThenByDescending(e => e.Id);

        // Get total count before pagination
        var totalCount = (ulong)await query.CountAsync();

        var pageSize =
            filter is not null && filter.PageSize > 0
                ? Math.Min((int)filter.PageSize, LedgerCursor.MaxPageSize)
                : LedgerCursor.DefaultPageSize;

        var entities = await query.Take(pageSize + 1).ToListAsync();

        var hasNextPage = entities.Count > pageSize;
        if (hasNextPage)
            entities = entities.Take(pageSize).ToList();

        var items = entities.Select(LedgerEntryEntity.ToModel).ToList();

        var nextCursor = string.Empty;
        if (!hasNextPage || items.Count <= 0)
            return new CursorList<LedgerEntryModel>
            {
                Cursor = nextCursor,
                TotalCount = totalCount,
                Items = items,
            };

        var lastEntry = entities[^1];
        nextCursor = new LedgerCursor { Timestamp = lastEntry.Timestamp, Id = lastEntry.Id }.Encode();

        var nextFilter = filter?.Clone() ?? new LedgerFilter();
        nextFilter.Cursor = nextCursor;

        return new CursorList<LedgerEntryModel>
        {
            Cursor = nextCursor,
            TotalCount = totalCount,
            NextPage = () => SearchAsync(searchText, nextFilter),
            Items = items,
        };
    }

    /// <summary>
    ///     Saves a ledger entry. Creates a new entry if the ID doesn't exist, or
    ///     updates an existing one.
    /// </summary>
    /// <param name="record">The ledger entry data to save</param>
    /// <returns>The saved ledger entry data</returns>
    public async Task<LedgerEntryModel> SaveAsync(LedgerEntryModel record)
    {
        var id = record.Id;
        var accountId = record.AccountId;
        var entity = await dbContext.LedgerEntries.FindAsync(id);

        if (entity is null)
        {
            // Create new ledger entry
            entity = new LedgerEntryEntity
            {
                Id = id,
                AccountId = accountId,
                Timestamp = record.Timestamp,
                Payee = record.Payee,
                Memo = record.Memo,
                TrxId = record.TrxId,
                Amount = record.Amount,
            };
            dbContext.LedgerEntries.Add(entity);
        }
        else
        {
            // Update existing ledger entry
            entity.AccountId = accountId;
            entity.Timestamp = record.Timestamp;
            entity.Payee = record.Payee;
            entity.Memo = record.Memo;
            entity.TrxId = record.TrxId;
            entity.Amount = record.Amount;
        }

        await dbContext.SaveChangesAsync();
        return LedgerEntryEntity.ToModel(entity);
    }

    /// <summary>
    ///     Saves each ledger entry in the provided list and returns the saved entries.
    /// </summary>
    /// <param name="records">The ledger entries to save.</param>
    /// <returns>The saved ledger entries.</returns>
    public async Task<List<LedgerEntryModel>> SaveAsync(List<LedgerEntryModel> records)
    {
        // Saves each ledger entry in the provided list and returns the saved entries.
        var savedEntries = new List<LedgerEntryModel>();
        foreach (var record in records)
        {
            var savedEntry = await SaveAsync(record);
            savedEntries.Add(savedEntry);
        }

        return savedEntries;
    }

    /// <summary>
    ///     Applies filter criteria to a ledger entries query.
    ///     Why separate this method? Following DRY (Don't Repeat Yourself) principle -
    ///     this logic
    ///     is shared between ListAsync and SearchAsync, so extracting it reduces
    ///     duplication and
    ///     makes the code easier to maintain and test.
    /// </summary>
    /// <param name="query">The base query to apply filters to</param>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>The filtered query</returns>
    private static IQueryable<LedgerEntryEntity> ApplyFilter(IQueryable<LedgerEntryEntity> query, LedgerFilter? filter)
    {
        if (filter is not null && !string.IsNullOrEmpty(filter.AccountId))
            query = query.Where(e => e.AccountId == filter.AccountId);

        if (filter?.StartTimestamp > 0)
        {
            var startTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(filter.StartTimestamp);
            query = query.Where(e => e.Timestamp >= startTimestamp);
        }

        if (filter?.EndTimestamp > 0)
        {
            var endTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(filter.EndTimestamp);
            query = query.Where(e => e.Timestamp <= endTimestamp);
        }

        var cursor = LedgerCursor.Decode(filter?.Cursor);
        if (cursor is not null)
            query = query.Where(e =>
                e.Timestamp < cursor.Timestamp || (e.Timestamp == cursor.Timestamp && e.Id.CompareTo(cursor.Id) < 0)
            );

        return query;
    }
}
