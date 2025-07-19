using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentTabsInTitleBarTests
{
    [AvaloniaFact]
    public void TabStrip_Moved_To_TitleBar_When_Enabled()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var host = new HostWindow { DataContext = root, ShowTabsInTitleBar = true };
        host.ApplyTemplate();

        var titleBar = host.FindDescendantOfType<HostWindowTitleBar>();
        var tabStrip = titleBar?.GetVisualDescendants().OfType<DocumentTabStrip>().FirstOrDefault();

        Assert.NotNull(tabStrip);
    }

    [AvaloniaFact]
    public void Layout_And_WholeWindowPseudoClasses_Applied()
    {
        var host = new HostWindow { ShowTabsInTitleBar = true, DocumentChromeControlsWholeWindow = true };
        host.ApplyTemplate();

        Assert.Contains(":documentchromecontrolswindow", host.Classes);
        Assert.Contains(":documenttabsintitlebar", host.Classes);
    }

    [AvaloniaFact]
    public void DocumentActivation_Works_With_TabStrip_In_TitleBar()
    {
        var factory = new Factory();
        var documentDock = new DocumentDock { Factory = factory, VisibleDockables = factory.CreateList<IDockable>() };
        var doc = new Document { Title = "Doc" };
        factory.AddDockable(documentDock, doc);
        factory.SetActiveDockable(doc);

        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, documentDock);
        root.Factory = factory;

        var host = new HostWindow { DataContext = root, ShowTabsInTitleBar = true };
        host.ApplyTemplate();

        factory.SetFocusedDockable(root, doc);

        Assert.Equal(doc, root.FocusedDockable);
    }
}
