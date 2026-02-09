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
    public void DocumentDock_Default_LayoutMode_Tabbed()
    {
        var dock = new DocumentDock();
        Assert.Equal(DocumentLayoutMode.Tabbed, dock.LayoutMode);
    }

    [AvaloniaFact]
    public void DocumentDock_Default_EmptyContent_Value()
    {
        var dock = new DocumentDock();
        Assert.Equal("No documents open", dock.EmptyContent);
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
    public void SetDocumentDockLayoutModeMdi_Changes_LayoutMode()
    {
        var factory = new Factory();
        var dock = new DocumentDock { Factory = factory };
        factory.SetDocumentDockLayoutModeMdi(dock);

        Assert.Equal(DocumentLayoutMode.Mdi, dock.LayoutMode);
    }
}
