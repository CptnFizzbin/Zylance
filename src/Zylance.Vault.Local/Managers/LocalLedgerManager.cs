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
    /// <inheritdoc />
    public async Task<CursorList<LedgerEntryModel>> ListAsync(LedgerFilter? filter)
    {
        var query = ApplyFilter(dbContext.LedgerEntries.AsQueryable(), filter);

        query = query.OrderByDescending(e => e.Timestamp).ThenByDescending(e => e.Id);

        var totalCount = (ulong)await query.CountAsync();

        // TODO: Apply pagination using filter.PageSize and filter.Cursor
        // TODO: update LedgerGrid to support infinite scrolling and pass appropriate filter parameters
        var entities = await query.ToListAsync();
        var hasNextPage = false;

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

    /// <inheritdoc />
    public async Task<LedgerEntryModel> GetAsync(Guid recordId)
    {
        var entity = await dbContext.LedgerEntries.FindAsync(recordId);
        return entity is null
            ? throw new KeyNotFoundException($"Ledger entry with ID {recordId} not found")
            : LedgerEntryEntity.ToModel(entity);
    }

    /// <inheritdoc />
    public Task<List<LedgerEntryModel>> GetAllAsync()
    {
        return Task.FromResult(dbContext.LedgerEntries.Select(LedgerEntryEntity.ToModel).ToList());
    }

    /// <inheritdoc />
    public async Task<LedgerEntryModel> DeleteAsync(Guid recordId)
    {
        var entity =
            await dbContext.LedgerEntries.FindAsync(recordId)
            ?? throw new KeyNotFoundException($"Ledger entry with ID {recordId} not found");

        dbContext.LedgerEntries.Remove(entity);
        await dbContext.SaveChangesAsync();
        return LedgerEntryEntity.ToModel(entity);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task<List<LedgerEntryModel>> FindByTrxIdsAsync(IEnumerable<string> trxIds)
    {
        var entries = dbContext
            .LedgerEntries.Where(e => trxIds.Contains(e.TrxId))
            .ToList()
            .Select(LedgerEntryEntity.ToModel)
            .ToList();
        return Task.FromResult(entries);
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

        if (DateTimeOffset.TryParse(filter?.StartTimestamp, out var startTimestamp))
            query = query.Where(e => e.Timestamp >= startTimestamp);

        if (DateTimeOffset.TryParse(filter?.EndTimestamp, out var endTimestamp))
            query = query.Where(e => e.Timestamp <= endTimestamp);

        var cursor = LedgerCursor.Decode(filter?.Cursor);
        if (cursor is not null)
            query = query.Where(e =>
                e.Timestamp < cursor.Timestamp || (e.Timestamp == cursor.Timestamp && e.Id.CompareTo(cursor.Id) < 0)
            );

        return query;
    }
}
