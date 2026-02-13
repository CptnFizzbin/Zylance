using Microsoft.Playwright;

namespace Zylance.Desktop.Tests.Lib.Screens;

public class VaultSelectScreen(IPage page)
{
    private ILocator IsActive => page.GetByRole(AriaRole.Heading, new() { Name = "Select Your Vault" });
    public ILocator VaultCreateButton => page.GetByRole(AriaRole.Button, new() { Name = "Create New Vault" });
    public ILocator VaultOpenButton => page.GetByRole(AriaRole.Button, new() { Name = "Open Existing Vault" });

    public Task WaitForActiveAsync()
    {
        return Assertions.Expect(IsActive).ToBeVisibleAsync();
    }
}
