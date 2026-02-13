using Microsoft.Playwright;

namespace Zylance.Desktop.Tests.Lib.Components.MenuBar;

public class FileMenuComponent(ILocator menu)
{
    public ILocator NewVault => menu.GetByRole(AriaRole.Menuitem, new() { Name = "New Vault" });
    public ILocator OpenVault => menu.GetByRole(AriaRole.Menuitem, new() { Name = "Open Vault" });
    public ILocator CloseVault => menu.GetByRole(AriaRole.Menuitem, new() { Name = "Close Vault" });
    public ILocator Exit => menu.GetByRole(AriaRole.Menuitem, new() { Name = "Exit" });
}
