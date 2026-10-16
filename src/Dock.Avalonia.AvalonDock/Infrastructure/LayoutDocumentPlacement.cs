using System;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutDocumentPlacement
{
    public static void Dock(LayoutContent content)
    {
        if (content.Root is not LayoutRoot root) throw new InvalidOperationException("The content must belong to a layout.");
        if (content.Parent is LayoutDocumentPane) return;
        if (content.PreviousContainer is LayoutDocumentPane)
        {
            content.Dock();
            return;
        }

        LayoutDocumentPane? destination = root.LastFocusedDocument?.Parent as LayoutDocumentPane;
        if (root.LastFocusedDocument is null)
        {
            foreach (ILayoutElement element in root.Descendents())
                if (element is LayoutDocumentPane pane) { destination = pane; break; }
        }
        LayoutOperationContext.RequireManager(content).NotifyLayoutOperation(content, floating: false, preview: true);
        if (destination is null)
        {
            LayoutPanel previousPanel = root.RootPanel;
            LayoutPanel panel = new() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            panel.Children.Add(previousPanel);
            root.RootPanel = panel;
            destination = new LayoutDocumentPane();
            panel.Children.Add(destination);
        }
        destination.Children.Add(content);
        LayoutOperationContext.RequireManager(content).NotifyLayoutOperation(content, floating: false, preview: false);
        root.CollectGarbage();
        content.SetFloating(false);
        LayoutOperationContext.SelectAndActivate(content);
    }
}
