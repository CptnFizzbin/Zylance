using Microsoft.Playwright;

namespace Zylance.Desktop.Tests.Lib.Components.MenuBar;

public class MenuComponent(ILocator element)
{
    public ILocator MenuItem(string itemName)
    {
        return element.GetByRole(AriaRole.Menuitem, new LocatorGetByRoleOptions { Name = itemName });
    }
}
