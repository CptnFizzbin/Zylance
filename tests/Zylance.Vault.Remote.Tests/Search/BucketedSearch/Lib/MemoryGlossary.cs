using Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;
using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Tests.Search.BucketedSearch.Lib;

public class MemoryGlossary : IGlossary
{
    private readonly Dictionary<string, Keyword> _keywords = new();

    public Keyword Get(string keyword)
    {
        return _keywords.GetValueOrDefault(keyword) ?? new Keyword { Value = keyword };
    }

    public List<Keyword> GetAll()
    {
        return _keywords.Values.ToList();
    }

    public Task Save(Keyword keyword)
    {
        _keywords[keyword.Value] = keyword;
        return Task.CompletedTask;
    }
}
