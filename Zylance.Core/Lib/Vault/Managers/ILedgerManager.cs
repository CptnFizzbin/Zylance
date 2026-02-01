using Zylance.Contract.Api.Ledger;
using Zylance.Contract.Models.Ledger;

namespace Zylance.Core.Lib.Vault.Managers;

public interface ILedgerManager : IRecordManager<Guid, LedgerEntryData>
{
    public Task<CursorList<LedgerEntryData>> ListAsync(LedgerFilter? filter);
    public Task<CursorList<LedgerEntryData>> SearchAsync(string searchText, LedgerFilter? filter);
}
