using System;
using System.Linq;
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
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }

        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    private static void CheckGrid<T>(LayoutGridControl<T> control, ILayoutElement model) where T : class, ILayoutPanelElement
    {
        if (!ReferenceEquals(control.Model, model) || !ReferenceEquals(control.Tag, model) || !Equals(control.Background, Brushes.Transparent))
            throw new Exception("The packaged grid types or compiled styles did not resolve in both XML namespaces.");
    }
}
