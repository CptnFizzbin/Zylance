using Microsoft.Playwright;

namespace Zylance.Desktop.Tests.Lib.Screens;

public class VaultSelectScreen(IPage page)
{
    public ILocator VaultCreateButton => page.GetByRole(AriaRole.Button, new() { Name = "Create New Vault" });
    public ILocator VaultOpenButton => page.GetByRole(AriaRole.Button, new() { Name = "Open Existing Vault" });

    public Task<bool> IsActiveAsync()
    {
        return page.GetByRole(AriaRole.Heading, new() { Name = "Select Your Vault" }).IsVisibleAsync();
    }
}
