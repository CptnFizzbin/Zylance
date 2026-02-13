using Microsoft.Playwright;

namespace Zylance.Desktop.Tests.Lib.Components.MenuBar;

public class MenuBarComponent(ILocator menuBar)
{
    public static MenuBarComponent GetFromPage(IPage page)
    {
        return new MenuBarComponent(page.GetByRole(AriaRole.Menubar, new() { Name = "Desktop Menu Bar" }));
    }

    public async Task<FileMenuComponent> OpenFileMenuAsync()
    {
        await menuBar.GetByRole(AriaRole.Button, new() { Name = "File" }).ClickAsync();
        return new FileMenuComponent(menuBar.GetByRole(AriaRole.Menu, new() { Name = "File" }));
    }
}
