namespace Zylance.Core.Lib.Vault;

public interface IRecordManager<in TId, TRecord>
{
    public Task<TRecord> GetAsync(TId recordId);
    public Task<TRecord> SaveAsync(TRecord record);
    public Task<TRecord> DeleteAsync(TId recordId);
}
