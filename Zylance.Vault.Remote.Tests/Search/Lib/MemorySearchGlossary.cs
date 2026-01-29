using Zylance.Vault.Remote.Search;

namespace Zylance.Vault.Remote.Tests.Search.Lib;

public class MemorySearchGlossary : ISearchGlossary
{
    private readonly Dictionary<string, SearchKeyword> _keywords = new();

    public List<SearchKeyword> GetKeywords()
    {
        return _keywords.Values.ToList();
    }

    public SearchKeyword GetOrAddKeyword(string token)
    {
        if (_keywords.TryGetValue(token, out var value))
            return value;

        value = new SearchKeyword { Value = token, NumBuckets = 0 };
        _keywords[token] = value;
        return value;
    }

    public Task SaveKeyword(SearchKeyword keyword)
    {
        _keywords[keyword.Value] = keyword;
        return Task.CompletedTask;
    }
}
