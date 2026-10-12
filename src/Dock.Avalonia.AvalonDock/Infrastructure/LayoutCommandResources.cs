using Avalonia.Controls;
using AvalonDock.Properties;

namespace AvalonDock.Infrastructure;

// Keep dynamic theme keys while sourcing their initial values from the public resources.
internal sealed class LayoutCommandResources : ResourceDictionary
{
    public LayoutCommandResources()
    {
        Add("ToolChromeControlCloseString", Resources.Document_Close);
        Add("ToolChromeControlFloatString", Resources.Anchorable_Float);
        Add("ToolChromeControlDockString", Resources.Anchorable_Dock);
        Add("ToolChromeControlDockAsDocumentString", Resources.Anchorable_DockAsDocument);
        Add("ToolChromeControlAutoHideString", Resources.Anchorable_AutoHide);
        Add("DocumentTabStripItemCloseOtherTabsString", Resources.Document_CloseAllButThis);
        Add("DocumentTabStripItemCloseAllTabsString", Resources.Document_CloseAll);
        Add("AvalonDockHideString", Resources.Anchorable_Hide);
        Add("AvalonDockNewHorizontalTabGroupString", Resources.Document_NewHorizontalTabGroup);
        Add("AvalonDockNewVerticalTabGroupString", Resources.Document_NewVerticalTabGroup);
        Add("AvalonDockMoveToNextTabGroupString", Resources.Document_MoveToNextTabGroup);
        Add("AvalonDockMoveToPreviousTabGroupString", Resources.Document_MoveToPreviousTabGroup);
    }
}
