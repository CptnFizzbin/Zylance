namespace Zylance.Core.Vault.Managers;

/// <summary>
///     Generic interface for record managers (CRUD operations) used by vault
///     implementations.
/// </summary>
/// <typeparam name="TId">The type of the record identifier.</typeparam>
/// <typeparam name="TRecord">The type of the record being managed.</typeparam>
public interface IRecordManager<in TId, TRecord>
{
    /// <summary>
    ///     Asserts that a record with the specified id exists, throwing if not found.
    /// </summary>
    /// <returns>The record with the specified id, or throws if not found.</returns>
    public async Task AssertExists(TId recordId)
    {
        await GetAsync(recordId);
    }

    /// <summary>
    ///     Retrieves a record by id.
    /// </summary>
    /// <param name="recordId">The identifier of the record to retrieve.</param>
    /// <returns>The record with the specified id, or throws if not found.</returns>
    public Task<TRecord> GetAsync(TId recordId);

    /// <summary>
    ///     Retrieves all records.
    /// </summary>
    /// <returns>All records managed by this manager.</returns>
    public Task<List<TRecord>> GetAllAsync();

    /// <summary>
    ///     Saves a record and returns the saved instance.
    /// </summary>
    /// <param name="record">The record to save.</param>
    /// <returns>The saved record instance.</returns>
    public Task<TRecord> SaveAsync(TRecord record);

    /// <summary>
    ///     Saves multiple records and returns the saved instances.
    /// </summary>
    /// <param name="records">The records to save.</param>
    /// <returns>The saved record instances.</returns>
    public Task<List<TRecord>> SaveAsync(List<TRecord> records);

    /// <summary>
    ///     Deletes a record by id and returns the deleted record.
    /// </summary>
    /// <param name="recordId">The identifier of the record to delete.</param>
    /// <returns>The deleted record instance.</returns>
    public Task<TRecord> DeleteAsync(TId recordId);

    /// <summary>
    ///     Deletes multiple records and returns the deleted records.
    /// </summary>
    /// <param name="records">The records to delete.</param>
    /// <returns>The deleted record instances.</returns>
    public Task<List<TRecord>> DeleteAsync(List<TRecord> records);
}
