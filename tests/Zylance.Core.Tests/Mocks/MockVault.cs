using Zylance.Contract.Api.Ledger;
using Zylance.Core.Vault.Interfaces;
using Zylance.Core.Vault.Managers;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Tests.Mocks;

/// <summary>
///     Lightweight in-memory IVault implementation for tests that can detect
///     whether it has been disposed.
/// </summary>
public class MockVault : IVault, IAsyncDisposable
{
    private readonly MockAccountManager _accounts = new();
    private readonly MockLedgerManager _ledgers = new();
    private readonly MockMetadataManager _metadata = new();

    public MockVault()
    {
        VaultId = Guid.NewGuid();
    }

    public bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        IsDisposed = true;
        await Task.CompletedTask;
    }

    public Guid VaultId { get; }

    public bool Locked => false;

    public IAccountManager Accounts => _accounts;

    public ILedgerManager Ledgers => _ledgers;

    public IMetadataManager Metadata => _metadata;

    public IVaultScope CreateScope()
    {
        return new MockVaultScope(this);
    }

    private void ThrowIfDisposed()
    {
        if (!IsDisposed)
            return;

        throw new ObjectDisposedException(nameof(MockVault));
    }

    // --- Mock managers ---
    private class MockMetadataManager : IMetadataManager
    {
        private readonly Dictionary<string, string> _store = new();

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            // Lookup without owner check here; the owning MockVault will perform checks
            _ = cancellationToken;
            return Task.FromResult<string?>(_store.TryGetValue(key, out var v) ? v : null);
        }

        public Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            _store[key] = value;
            return Task.CompletedTask;
        }
    }

    private class MockAccountManager : IAccountManager
    {
        private readonly List<AccountModel> _items = new();

        public Task<AccountModel> GetAsync(string recordId)
        {
            var item = _items.Find(a => a.Id == recordId) ?? throw new KeyNotFoundException();
            return Task.FromResult(item);
        }

        public Task<List<AccountModel>> GetAllAsync()
        {
            return Task.FromResult(new List<AccountModel>(_items));
        }

        public Task<AccountModel> SaveAsync(AccountModel record)
        {
            _items.Add(record);
            return Task.FromResult(record);
        }

        public Task<List<AccountModel>> SaveAsync(List<AccountModel> records)
        {
            _items.AddRange(records);
            return Task.FromResult(records);
        }

        public Task<AccountModel> DeleteAsync(string recordId)
        {
            var item = _items.Find(a => a.Id == recordId) ?? throw new KeyNotFoundException();
            _items.Remove(item);
            return Task.FromResult(item);
        }

        public Task<List<AccountModel>> DeleteAsync(List<AccountModel> records)
        {
            foreach (var r in records)
                _items.RemoveAll(i => i.Id == r.Id);
            return Task.FromResult(records);
        }

        public Task<List<AccountModel>> ListAsync()
        {
            return Task.FromResult(new List<AccountModel>(_items));
        }
    }

    private class MockLedgerManager : ILedgerManager
    {
        private readonly List<LedgerEntryModel> _items = new();

        public Task<LedgerEntryModel> GetAsync(Guid recordId)
        {
            var item = _items.Find(i => i.Id == recordId) ?? throw new KeyNotFoundException();
            return Task.FromResult(item);
        }

        public Task<List<LedgerEntryModel>> GetAllAsync()
        {
            return Task.FromResult(new List<LedgerEntryModel>(_items));
        }

        public Task<LedgerEntryModel> SaveAsync(LedgerEntryModel record)
        {
            _items.Add(record);
            return Task.FromResult(record);
        }

        public Task<List<LedgerEntryModel>> SaveAsync(List<LedgerEntryModel> records)
        {
            _items.AddRange(records);
            return Task.FromResult(records);
        }

        public Task<LedgerEntryModel> DeleteAsync(Guid recordId)
        {
            var item = _items.Find(a => a.Id == recordId) ?? throw new KeyNotFoundException();
            _items.Remove(item);
            return Task.FromResult(item);
        }

        public Task<List<LedgerEntryModel>> DeleteAsync(List<LedgerEntryModel> records)
        {
            foreach (var r in records)
                _items.RemoveAll(i => i.Id == r.Id);
            return Task.FromResult(records);
        }

        public Task<CursorList<LedgerEntryModel>> ListAsync(LedgerFilter? filter)
        {
            var cursor = new CursorList<LedgerEntryModel>
            {
                Items = new List<LedgerEntryModel>(_items),
                Cursor = string.Empty,
                TotalCount = (ulong)_items.Count,
            };

            return Task.FromResult(cursor);
        }

        public Task<CursorList<LedgerEntryModel>> SearchAsync(string searchText, LedgerFilter? filter)
        {
            var cursor = new CursorList<LedgerEntryModel>
            {
                Items = new List<LedgerEntryModel>(_items),
                Cursor = string.Empty,
                TotalCount = (ulong)_items.Count,
            };

            return Task.FromResult(cursor);
        }
    }

    private class MockVaultScope : IVaultScope
    {
        private bool _disposed;

        public MockVaultScope(MockVault vault)
        {
            Vault = vault;
        }

        public IVault Vault { get; }

        public Task Commit()
        {
            return Task.CompletedTask;
        }

        public Task Rollback()
        {
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;
            await Task.CompletedTask;
        }
    }
}
