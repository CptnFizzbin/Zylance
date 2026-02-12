using Microsoft.Playwright;

namespace Zylance.Desktop.Tests.Lib.PageContexts;

public class MenuContext(ILocator menu)
{
    public Task ClickMenuItemAsync(string itemName)
    {
        return menu.GetByRole(
            AriaRole.Menuitem,
            new LocatorGetByRoleOptions { Name = itemName }
        ).ClickAsync();
    }
}
