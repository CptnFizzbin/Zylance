namespace Zylance.Vault.Remote.Search;

public interface IZkSearchEngine<TItemId> where TItemId : notnull
{
    public Task AddItemAsync(TItemId itemId, string content);
    public Task AddItemsAsync(List<(TItemId itemId, string content)> items);

    // oldContent is required to properly remove indexed terms because the storage
    // should not know what terms were indexed for the item.
    public Task UpdateItemAsync(TItemId itemId, string oldContent, string newContent);
    public Task UpdateItemsAsync(List<(TItemId itemId, string oldContent, string newContent)> items);

    // Content is required to properly remove indexed terms because the storage
    // should not know what terms were indexed for the item.
    public Task RemoveItemAsync(TItemId itemId, string content);
    public Task RemoveItemsAsync(List<(TItemId itemId, string content)> items);

    public Task<List<TItemId>> SearchAsync(
        string searchTerms,
        SearchDirection direction = SearchDirection.LatestFirst,
        bool fuzzy = true
    );
}
