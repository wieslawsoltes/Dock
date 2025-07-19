using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentDockPropertiesTests
{
    [AvaloniaFact]
    public void DocumentDock_Default_EnableWindowDrag_False()
    {
        var dock = new DocumentDock();
        Assert.False(dock.EnableWindowDrag);
    }

    [AvaloniaFact]
    public void DocumentDock_Default_TabsLayout_Top()
    {
        var dock = new DocumentDock();
        Assert.Equal(DocumentTabLayout.Top, dock.TabsLayout);
    }

    [AvaloniaFact]
    public void SetDocumentDockTabsLayoutLeft_Changes_TabsLayout()
    {
        var factory = new Factory();
        var dock = new DocumentDock { Factory = factory };
        factory.SetDocumentDockTabsLayoutLeft(dock);

        Assert.Equal(DocumentTabLayout.Left, dock.TabsLayout);
    }

    [AvaloniaFact]
    public void SetDocumentDockTabsLayoutRight_Changes_TabsLayout()
    {
        var factory = new Factory();
        var dock = new DocumentDock { Factory = factory };
        factory.SetDocumentDockTabsLayoutRight(dock);

        Assert.Equal(DocumentTabLayout.Right, dock.TabsLayout);
    }

    [AvaloniaFact]
    public void SetDocumentDockTabsLayoutTop_Changes_TabsLayout()
    {
        var factory = new Factory();
        var dock = new DocumentDock { Factory = factory };
        dock.TabsLayout = DocumentTabLayout.Left;
        factory.SetDocumentDockTabsLayoutTop(dock);

        Assert.Equal(DocumentTabLayout.Top, dock.TabsLayout);
    }

    [AvaloniaFact]
    public void Set_TabsLayout_TitleBar()
    {
        var dock = new DocumentDock();
        dock.TabsLayout = DocumentTabLayout.TitleBar;

        Assert.Equal(DocumentTabLayout.TitleBar, dock.TabsLayout);
    }
}
