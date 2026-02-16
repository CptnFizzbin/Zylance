using Microsoft.EntityFrameworkCore;
using Serilog;
using Zylance.Core.Logging;
using Zylance.Core.Vault.Managers;
using Zylance.Core.Vault.Models;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Managers;

/// <summary>
///     Local implementation of IAccountManager using Entity Framework Core.
/// </summary>
public class LocalAccountManager(LocalVaultDbContext dbContext) : IAccountManager
{
    private static readonly ILogger Log = ZyLogger.ForContext<LocalAccountManager>();

    /// <summary>
    ///     Gets an account by its string ID.
    /// </summary>
    /// <param name="recordId">The account ID as a string.</param>
    /// <returns>The account data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the account is not found</exception>
    public async Task<AccountModel> GetAsync(string recordId)
    {
        var entity = await dbContext.Accounts.FindAsync(recordId);
        return entity is null
            ? throw new KeyNotFoundException($"Account with ID {recordId} not found")
            : AccountEntity.ToModel(entity);
    }

    /// <summary>
    ///     Gets all accounts in the vault as a list.
    /// </summary>
    /// <returns>All account models in the vault.</returns>
    public Task<List<AccountModel>> GetAllAsync()
    {
        return Task.FromResult(dbContext.Accounts.Select(AccountEntity.ToModel).ToList());
    }

    /// <summary>
    ///     Saves a list of accounts. Each account is created or updated as needed.
    /// </summary>
    /// <param name="records">The accounts to save.</param>
    /// <returns>The saved account models.</returns>
    public async Task<List<AccountModel>> SaveAsync(List<AccountModel> records)
    {
        var savedEntries = new List<AccountModel>();
        foreach (var record in records)
        {
            var savedEntry = await SaveAsync(record);
            savedEntries.Add(savedEntry);
        }

        return savedEntries;
    }

    /// <summary>
    ///     Deletes an account by its string ID.
    /// </summary>
    /// <param name="recordId">The account ID as a string.</param>
    /// <returns>The deleted account data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the account is not found</exception>
    public async Task<AccountModel> DeleteAsync(string recordId)
    {
        var entity =
            await dbContext.Accounts.FindAsync(recordId)
            ?? throw new KeyNotFoundException($"Account with ID {recordId} not found");
        dbContext.Accounts.Remove(entity);
        await dbContext.SaveChangesAsync();
        return AccountEntity.ToModel(entity);
    }

    /// <summary>
    ///     Deletes a list of accounts by their string IDs.
    /// </summary>
    /// <param name="records">The accounts to delete.</param>
    /// <returns>The deleted account models.</returns>
    public async Task<List<AccountModel>> DeleteAsync(List<AccountModel> records)
    {
        var deletedEntries = new List<AccountModel>();
        foreach (var record in records)
        {
            var entity = await DeleteAsync(record.Id);
            deletedEntries.Add(entity);
        }

        return deletedEntries;
    }

    /// <summary>
    ///     Lists all accounts in the vault.
    /// </summary>
    /// <returns>A cursor list of all accounts</returns>
    public async Task<List<AccountModel>> ListAsync()
    {
        var entities = await dbContext.Accounts.ToListAsync();
        var items = entities.Select(AccountEntity.ToModel).ToList();
        return items;
    }

    /// <summary>
    ///     Saves an account. Creates a new account if the ID doesn't exist, or updates
    ///     an existing one.
    /// </summary>
    /// <param name="record">The account data to save</param>
    /// <returns>The saved account data</returns>
    public async Task<AccountModel> SaveAsync(AccountModel record)
    {
        var entity = await dbContext.Accounts.FindAsync(record.Id);

        if (entity is null)
        {
            // Create new account
            entity = new AccountEntity
            {
                Id = record.Id,
                Name = record.Name,
                Type = record.Type,
                Balance = record.Balance,
            };
            dbContext.Accounts.Add(entity);
        }
        else
        {
            // Update existing account
            entity.Name = record.Name;
            entity.Type = record.Type;
            entity.Balance = record.Balance;
        }

        await dbContext.SaveChangesAsync();
        return AccountEntity.ToModel(entity);
    }

    /// <summary>
    ///     Gets an account by its Guid ID.
    /// </summary>
    /// <param name="recordId">The account ID as a Guid.</param>
    /// <returns>The account data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the account is not found</exception>
    public async Task<AccountModel> GetAsync(Guid recordId)
    {
        // Convert Guid to string for lookup
        var entity = await dbContext.Accounts.FindAsync(recordId.ToString());
        return entity is null
            ? throw new KeyNotFoundException($"Account with ID {recordId} not found")
            : AccountEntity.ToModel(entity);
    }

    /// <summary>
    ///     Deletes an account by its Guid ID.
    /// </summary>
    /// <param name="recordId">The account ID as a Guid.</param>
    /// <returns>The deleted account data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the account is not found</exception>
    public async Task<AccountModel> DeleteAsync(Guid recordId)
    {
        // Convert Guid to string for lookup
        var entity =
            await dbContext.Accounts.FindAsync(recordId.ToString())
            ?? throw new KeyNotFoundException($"Account with ID {recordId} not found");
        dbContext.Accounts.Remove(entity);
        await dbContext.SaveChangesAsync();
        return AccountEntity.ToModel(entity);
    }
}
