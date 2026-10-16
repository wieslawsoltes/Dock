using System.IO;
using System.Xml;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutInterfaceMembershipTests
{
    [AvaloniaTheory]
    [InlineData("panel", true, false, true)]
    [InlineData("documents", true, true, true)]
    [InlineData("document-group", true, true, true)]
    [InlineData("tools", true, true, true)]
    [InlineData("tool-group", true, true, true)]
    [InlineData("anchor-group", false, false, false)]
    [InlineData("anchor-side", false, false, false)]
    [InlineData("positionable", false, false, false)]
    [InlineData("application-panel", true, false, false)]
    [InlineData("document", false, false, false)]
    [InlineData("tool", false, false, false)]
    [InlineData("document-window", false, false, false)]
    [InlineData("tool-window", false, false, true)]
    public void Public_Interfaces_Follow_The_Model_Contract(string kind, bool panel, bool pane, bool visibility)
    {
        object model = kind switch
        {
            "panel" => new LayoutPanel(), "documents" => new LayoutDocumentPane(),
            "document-group" => new LayoutDocumentPaneGroup(), "tools" => new LayoutAnchorablePane(),
            "tool-group" => new LayoutAnchorablePaneGroup(), "anchor-group" => new LayoutAnchorGroup(),
            "anchor-side" => new LayoutAnchorSide(), "positionable" => new ApplicationGroup(),
            "application-panel" => new ApplicationPanel(), "document" => new LayoutDocument(),
            "tool" => new LayoutAnchorable(), "document-window" => new LayoutDocumentFloatingWindow(),
            _ => new LayoutAnchorableFloatingWindow()
        };
        Assert.Equal(panel, model is ILayoutPanelElement);
        Assert.Equal(pane, model is ILayoutPane);
        Assert.Equal(visibility, model is ILayoutElementWithVisibility);
    }

    [AvaloniaFact]
    public void An_Empty_Group_Read_Uses_Its_Class_Visibility_Method_Without_Dispatching_An_Additional_Interface()
    {
        ApplicationVisibilityGroup group = new();
        using XmlReader reader = XmlReader.Create(new StringReader("<Group />"));
        group.ReadXml(reader);
        Assert.Equal(1, group.ClassCalls);
        Assert.Equal(0, group.InterfaceCalls);
        Assert.False(group.IsVisible);
        ((ILayoutElementWithVisibility)group).ComputeVisibility();
        Assert.Equal(1, group.InterfaceCalls);
    }

    private class ApplicationGroup : LayoutPositionableGroup<ILayoutPanelElement> { }
    private sealed class ApplicationPanel : ApplicationGroup, ILayoutPanelElement { }
    private sealed class ApplicationVisibilityGroup : ApplicationGroup, ILayoutElementWithVisibility
    {
        internal int ClassCalls { get; private set; }
        internal int InterfaceCalls { get; private set; }
        protected override bool GetVisibility() { ClassCalls++; return false; }
        void ILayoutElementWithVisibility.ComputeVisibility() => InterfaceCalls++;
    }
}
