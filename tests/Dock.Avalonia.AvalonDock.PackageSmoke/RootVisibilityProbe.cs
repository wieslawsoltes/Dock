using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avalonia.Layout;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class RootVisibilityProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("root spelling", () =>
        {
            LayoutRoot root = Read("<RootPanel Orientation='vertical' />");
            if (root.RootPanel.Orientation != Orientation.Horizontal) throw new Exception("Root orientation used the nested panel parser.");
        });
        Check("root fallback", () =>
        {
            LayoutRoot root = Read("<RootPanel Orientation='Sideways' />");
            if (root.RootPanel.Orientation != Orientation.Horizontal) throw new Exception("Root orientation did not fall back to horizontal.");
        });
        Check("root child filtering", () =>
        {
            LayoutRoot root = Read("<RootPanel><LayoutDocument /><LayoutAnchorGroup /><LayoutDocumentPane /></RootPanel>");
            if (root.RootPanel.ChildrenCount != 1 || root.RootPanel.Children[0] is not LayoutDocumentPane)
                throw new Exception("A root retained a recognized non-panel child.");
        });
        Check("side child filtering", () =>
        {
            LayoutRoot root = Read("<RootPanel />", "<TopSide><LayoutDocument /><LayoutAnchorGroup /></TopSide>");
            if (root.TopSide!.ChildrenCount != 1) throw new Exception("A root side did not consume a non-group child.");
        });
        Check("public interface membership", () =>
        {
            object side = new LayoutAnchorSide();
            object group = new LayoutAnchorGroup();
            object panel = new LayoutPanel();
            if (side is ILayoutPane or ILayoutPanelElement or ILayoutElementWithVisibility ||
                group is ILayoutPane or ILayoutPanelElement or ILayoutElementWithVisibility ||
                panel is ILayoutPane || panel is not ILayoutPanelElement)
                throw new Exception("Generic groups supplied interfaces that belong to concrete models.");
        });
        Check("visibility base calls", () =>
        {
            ParentPanel parent = new();
            DocumentPane pane = new() { Parent = parent };
            parent.Calls = 0;
            pane.Notify();
            if (parent.Calls != 2) throw new Exception("The concrete visibility hook omitted a parent recomputation.");
        });
        Check("empty group visibility dispatch", () =>
        {
            VisibilityGroup group = new();
            using XmlReader reader = XmlReader.Create(new StringReader("<Group />"));
            group.ReadXml(reader);
            if (group.InterfaceCalls != 0 || group.IsVisible) throw new Exception("The base reader dispatched an extra visibility interface.");
        });
        if (failures.Count != 0) throw new Exception("Packaged root/visibility failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static LayoutRoot Read(string panel, string top = "<TopSide />")
    {
        LayoutRoot root = new();
        using XmlReader reader = XmlReader.Create(new StringReader("<LayoutRoot>" + panel + top + "<RightSide /><LeftSide /><BottomSide /><FloatingWindows /><Hidden /></LayoutRoot>"));
        root.ReadXml(reader);
        return root;
    }
    private sealed class ParentPanel : LayoutPanel
    {
        internal int Calls { get; set; }
        protected override bool GetVisibility() { Calls++; return base.GetVisibility(); }
    }
    private sealed class DocumentPane : LayoutDocumentPane
    {
        internal void Notify() => base.OnIsVisibleChanged();
    }
    private sealed class VisibilityGroup : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutElementWithVisibility
    {
        internal int InterfaceCalls { get; private set; }
        protected override bool GetVisibility() => false;
        void ILayoutElementWithVisibility.ComputeVisibility() => InterfaceCalls++;
    }
}
