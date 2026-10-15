using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;

namespace PackageConsumer;

internal static class AutoHideReturnProbe
{
    internal static void Verify()
    {
        LayoutAnchorable tool = new() { ContentId = "floating-tool", Content = "Tool", CanAutoHide = false };
        DockingManager manager = new() { AutoHideWindowClosingTimer = int.MaxValue, Layout = new() { RootPanel = new() { Children =
        { new LayoutDocumentPane(new LayoutDocument { ContentId = "document", Content = "Editor" }), new LayoutAnchorablePane(tool) } } } };
        Window owner = new() { Width = 800, Height = 600 };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Content = manager;
        try
        {
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            tool.Float();
            tool.ToggleAutoHide();
            if (!tool.IsAutoHidden || !tool.IsFloating || !ReferenceEquals(manager.Layout.RightSide, tool.Parent?.Parent))
                throw new Exception("The packaged explicit auto-hide did not pin floating content in the manager's strip.");
            XmlLayoutSerializer serializer = new(manager);
            using StringWriter text = new();
            serializer.Serialize(text);
            serializer.Deserialize(new StringReader(text.ToString()));
            tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
            LayoutAnchorablePane previous = (LayoutAnchorablePane)tool.PreviousContainer!;
            manager.DockFactory.PreviewPinnedDockable(tool.DockModel);
            manager.DockFactory.HidePreviewingDockables(manager.DockLayout);
            if (!ReferenceEquals(manager.DockLayout, manager.DockFactory.FindRoot(tool.DockModel)))
                throw new Exception("The packaged preview cleanup lost the root containing the pin.");
            tool.ToggleAutoHide();
            if (!ReferenceEquals(previous, tool.Parent) || !tool.IsFloating)
                throw new Exception("The packaged auto-hide return lost its floating pane.");

            LayoutAnchorable hidden = new() { ContentId = "hidden" };
            LayoutAnchorable restored = new() { ContentId = "restored" };
            LayoutAnchorGroup group = new() { Children = { hidden, restored } };
            manager.Layout.LeftSide!.Children.Add(group);
            hidden.Hide();
            restored.ToggleAutoHide();
            if (!ReferenceEquals(restored.Parent, hidden.PreviousContainer))
                throw new Exception("The packaged unpin did not redirect a hidden tool's return location.");
            hidden.Show();
            if (!ReferenceEquals(restored.Parent, hidden.Parent) || hidden.IsAutoHidden)
                throw new Exception("The packaged hidden tool returned to an obsolete anchor group.");
        }
        finally { manager.Layout.FloatingWindows.Clear(); owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
