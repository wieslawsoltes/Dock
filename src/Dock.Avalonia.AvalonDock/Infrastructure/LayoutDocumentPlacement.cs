using System;
using AvalonDock.Layout;
using Dock.Model;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal static class LayoutDocumentPlacement
{
    public static void Dock(LayoutContent content)
    {
        if (content.Root is not LayoutRoot root) throw new InvalidOperationException("The content must belong to a layout.");
        if (content.Parent is LayoutDocumentPane || content.DockModel.Owner is not IDock source ||
            !DockCapabilityResolver.IsEnabled(content.DockModel, DockCapability.DockAsDocument,
                DockCapabilityResolver.ResolveOperationDock(content.DockModel))) return;
        using IDockOperationScope? scope = root.Operations.Begin(content.DockModel, LayoutOperation.Dock);
        if (content.PreviousContainer is LayoutDocumentPane previous && ReferenceEquals(previous.Root, root))
        {
            content.Dock();
            scope?.Complete();
            return;
        }

        LayoutDocumentPane? destination = root.LastFocusedDocument?.Parent as LayoutDocumentPane;
        if (destination is null || !ReferenceEquals(destination.Root, root) || destination.FindParent<LayoutFloatingWindow>() is not null)
        {
            destination = null;
            // A native floating root has its own document dock. Search the
            // manager's main panel so docking cannot target that same window.
            foreach (ILayoutElement element in root.RootPanel.Descendents())
                if (element is LayoutDocumentPane pane) { destination = pane; break; }
        }
        if (destination is null)
        {
            LayoutPanel previousPanel = root.RootPanel;
            LayoutPanel panel = new() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            root.RootPanel = panel;
            panel.Children.Add(previousPanel);
            destination = new LayoutDocumentPane();
            panel.Children.Add(destination);
        }
        root.DockFactory.MoveDockable(source, (IDock)destination.DockModel, content.DockModel, null);
        content.IsSelected = true;
        content.IsActive = true;
        root.CollectGarbage();
        scope?.Complete();
    }
}
