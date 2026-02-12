using Microsoft.Playwright;

namespace Zylance.Desktop.Tests.Lib.PageContexts;

public class MenuBarContext(IPage page)
{
    private ILocator MenuBar => page.Locator("#menu-bar");
    public ILocator MenuButton(string id) => MenuBar.Locator($"#menuButton-{id}");
    public ILocator Menu(string id) => MenuBar.Locator($"#menu-{id}");
    public Task OpenMenuAsync(string id) => MenuButton(id).ClickAsync();
    public Task OpenFileMenuAsync => OpenMenuAsync("file");

    public async Task ClickMenuItemAsync(string menuId, string itemId)
    {
        await OpenMenuAsync(menuId);
        await MenuBar.Locator($"#menuItem-{itemId}").ClickAsync();
    }

    public Task NewVaultAsync => ClickMenuItemAsync("file", "newVault");
    public Task OpenVaultAsync => ClickMenuItemAsync("file", "openVault");
    public Task CloseVaultAsync => ClickMenuItemAsync("file", "closeVault");
    public Task ExitAsync => ClickMenuItemAsync("file", "exit");
}
