using Microsoft.EntityFrameworkCore;
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
    /// <summary>
    ///     Gets an account by its ID.
    /// </summary>
    /// <param name="recordId">The account ID</param>
    /// <returns>The account data</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the account is not found</exception>
    public async Task<AccountModel> GetAsync(Guid recordId)
    {
        var entity = await dbContext.Accounts.FindAsync(recordId);
        return entity is null
            ? throw new KeyNotFoundException($"Account with ID {recordId} not found")
            : EntityToModel(entity);
    }

    /// <summary>
    ///     Deletes an account by its ID.
    /// </summary>
    /// <param name="recordId">The account ID to delete</param>
    /// <returns>The deleted account data</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the account is not found</exception>
    public async Task<AccountModel> DeleteAsync(Guid recordId)
    {
        var entity =
            await dbContext.Accounts.FindAsync(recordId)
            ?? throw new KeyNotFoundException($"Account with ID {recordId} not found");

        dbContext.Accounts.Remove(entity);
        await dbContext.SaveChangesAsync();

        return EntityToModel(entity);
    }

    /// <summary>
    ///     Lists all accounts in the vault.
    /// </summary>
    /// <returns>A cursor list of all accounts</returns>
    public async Task<List<AccountModel>> ListAsync()
    {
        var entities = await dbContext.Accounts.ToListAsync();
        var items = entities.Select(EntityToModel).ToList();
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
        var id = Guid.Parse(record.Id);
        var entity = await dbContext.Accounts.FindAsync(id);

        if (entity is null)
        {
            // Create new account
            entity = new AccountEntity
            {
                Id = id,
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
        return EntityToModel(entity);
    }

    /// <summary>
    ///     Converts an AccountEntity to AccountData.
    ///     Why this pattern? In clean architecture, we separate our domain models
    ///     (AccountData) from
    ///     our persistence models (AccountEntity). This allows us to:
    ///     - Keep database concerns out of the core business logic
    ///     - Change database structure without affecting the API contract
    ///     - Use different data formats (protobuf) for network transport
    /// </summary>
    private static AccountModel EntityToModel(AccountEntity entity)
    {
        return new AccountModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Type = entity.Type,
            Balance = entity.Balance,
        };
    }
}
