namespace Zylance.Core.Lib.Vault;

/// <summary>
/// Generic interface for record managers (CRUD operations) used by vault implementations.
/// </summary>
public interface IRecordManager<in TId, TRecord>
{
    /// <summary>
    /// Retrieves a record by id.
    /// </summary>
    public Task<TRecord> GetAsync(TId recordId);

    /// <summary>
    /// Saves a record and returns the saved instance.
    /// </summary>
    public Task<TRecord> SaveAsync(TRecord record);

    /// <summary>
    /// Deletes a record by id and returns the deleted record.
    /// </summary>
    public Task<TRecord> DeleteAsync(TId recordId);
}
