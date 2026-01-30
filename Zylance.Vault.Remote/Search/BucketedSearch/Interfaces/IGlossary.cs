#nullable disable

using Zylance.Vault.Remote.Search.BucketedSearch.Models;

namespace Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;

public interface IGlossary
{
    public Keyword Get(string keyword);

    public List<Keyword> GetAll();

    public Task Save(Keyword keyword);
}
