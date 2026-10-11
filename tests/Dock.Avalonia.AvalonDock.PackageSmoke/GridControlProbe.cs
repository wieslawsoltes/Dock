using System;
using System.IO;
using System.Linq;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

public sealed class GridManagerProbe : DockingManager
{
    public int Changes { get; private set; }
    protected override void OnLayoutRootPanelChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Changes++;
        base.OnLayoutRootPanelChanged(e);
    }
}

internal static class GridControlProbe
{
    internal static void Verify()
    {
        GridControlWindow window = new();
        GridManagerProbe manager = window.FindControl<GridManagerProbe>("Manager")!;
        try
        {
            window.Show(); Update();
            LayoutPanelControl root = manager.LayoutRootPanel ?? throw new Exception("The packaged root grid was not created.");
            LayoutDocumentPaneGroupControl documents = manager.GetVisualDescendants().OfType<LayoutDocumentPaneGroupControl>().Single();
            LayoutAnchorablePaneGroupControl tools = manager.GetVisualDescendants().OfType<LayoutAnchorablePaneGroupControl>().Single();
            CheckGrid<ILayoutPanelElement>(root, manager.Layout.RootPanel);
            CheckGrid<ILayoutDocumentPane>(documents, manager.Layout.RootPanel.Children[0]);
            CheckGrid<ILayoutAnchorablePane>(tools, manager.Layout.RootPanel.Children[1]);
            LayoutDocumentPaneGroup documentModel = (LayoutDocumentPaneGroup)documents.Model;
            LayoutAnchorablePaneGroup toolModel = (LayoutAnchorablePaneGroup)tools.Model;
            if (((LayoutDocumentPane)documentModel.Children[0]).DockModel is StyledElement { Parent: Control })
                throw new Exception("The packaged native create-button host parented a model into its visual controls.");
            if (((LayoutDocumentPane)documentModel.Children[0]).DockWidth != GridLength.Star ||
                ((LayoutAnchorablePane)toolModel.Children[0]).DockHeight != GridLength.Star || toolModel.DockWidth != new GridLength(400))
                throw new Exception("The packaged grids did not apply the group/panel length policies.");
            toolModel.Orientation = global::Avalonia.Layout.Orientation.Horizontal;
            Update();
            if (tools.Orientation != toolModel.Orientation || ((LayoutAnchorablePane)toolModel.Children[0]).DockWidth != GridLength.Star)
                throw new Exception("The packaged group did not follow its live model orientation.");
            LayoutContent document = ((LayoutDocumentPane)documentModel.Children[0]).Children[0];
            Control view = manager.GetLayoutItemFromModel(document)!.View;
            int changes = manager.Changes;
            manager.SetValue(DockingManager.LayoutRootPanelProperty, null);
            Update();
            if (manager.GetVisualDescendants().Contains(root) || root.Children.Count != 0)
                throw new Exception("Clearing the packaged root property left its control displayed.");
            manager.LayoutRootPanel = root;
            Update();
            if (manager.Changes != changes + 2 || !root.GetVisualDescendants().Contains(view))
                throw new Exception("Restoring the packaged root did not reuse the content view and notify the external override.");
            manager.Layout = new LayoutRoot();
            Update();
            if (ReferenceEquals(root, manager.LayoutRootPanel) || !ReferenceEquals(manager.LayoutRootPanel?.Model, manager.Layout.RootPanel))
                throw new Exception("Replacing the packaged layout did not replace its public grid.");
            PixelSizingProbe.Verify(window, manager);
            AutoSizingProbe.Verify(window, manager);
            VerifyLengthHooks();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }

        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    private static void CheckGrid<T>(LayoutGridControl<T> control, ILayoutElement model) where T : class, ILayoutPanelElement
    {
        if (!ReferenceEquals(control.Model, model) || !ReferenceEquals(control.Tag, model) || !Equals(control.Background, Brushes.Transparent))
            throw new Exception("The packaged grid types or compiled styles did not resolve in both XML namespaces.");
    }

    private static void VerifyLengthHooks()
    {
        SizePaneProbe pane = new();
        SizeGroupProbe group = new() { Children = { pane } };
        group.DockWidth = new GridLength(320);
        group.DockHeight = new GridLength(240);
        if (pane.DockWidth != group.DockWidth || pane.DockHeight != group.DockHeight ||
            pane.WidthChanges != 1 || pane.HeightChanges != 1 || group.WidthChanges != 1 || group.HeightChanges != 1)
            throw new Exception("Packaged length hooks did not propagate pixel sizes to the group's only child.");
        group.DockWidth = new GridLength(320);
        group.DockHeight = GridLength.Auto;
        if (group.WidthChanges != 1 || pane.WidthChanges != 1 || pane.HeightChanges != 1)
            throw new Exception("Packaged length hooks did not preserve equal values and independent non-pixel child sizes.");
        using StringReader text = new("<LayoutAnchorablePaneGroup DockWidth=\"410\" DockHeight=\"280\" />");
        using XmlReader reader = XmlReader.Create(text);
        group.ReadXml(reader);
        if (group.DockWidth != new GridLength(410) || group.DockHeight != new GridLength(280) ||
            group.WidthChanges != 1 || group.HeightChanges != 2 || pane.DockWidth != new GridLength(320) || pane.DockHeight != new GridLength(240))
            throw new Exception("Packaged XML restoration invoked sizing hooks or overwrote existing child lengths.");
    }

    private sealed class SizePaneProbe : LayoutAnchorablePane
    {
        public int WidthChanges { get; private set; }
        public int HeightChanges { get; private set; }
        protected override void OnDockWidthChanged() { WidthChanges++; base.OnDockWidthChanged(); }
        protected override void OnDockHeightChanged() { HeightChanges++; base.OnDockHeightChanged(); }
    }
    private sealed class SizeGroupProbe : LayoutAnchorablePaneGroup
    {
        public int WidthChanges { get; private set; }
        public int HeightChanges { get; private set; }
        protected override void OnDockWidthChanged() { WidthChanges++; base.OnDockWidthChanged(); }
        protected override void OnDockHeightChanged() { HeightChanges++; base.OnDockHeightChanged(); }
    }
}
