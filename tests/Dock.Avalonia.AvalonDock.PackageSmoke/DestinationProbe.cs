using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Dock.Model;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class DestinationProbe
{
    internal static void Verify()
    {
        LayoutDocument document = new() { ContentId = "document", Content = "Editor" };
        LayoutAnchorable first = new() { ContentId = "first", Content = "First tool" };
        LayoutAnchorable second = new() { ContentId = "second", Content = "Second tool" };
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children =
        {
            new LayoutDocumentPane(document), new LayoutAnchorablePane(first), new LayoutAnchorablePane(second)
        } } } };
        Window owner = new() { Width = 800, Height = 500 };
        owner.Styles.Add(new AvalonDockTheme()); owner.Content = manager;
        try
        {
            owner.Show(); owner.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            second.Float(); Dispatcher.UIThread.RunJobs();
            DockManager engine = new(new DockService());
            IDockable target = second.DockModel.Owner!;
            if (manager.DockFactory is not IDockDropPolicy ||
                engine.IsDockTargetVisible(document.DockModel, target, DockOperation.Top) ||
                engine.ValidateDockable(document.DockModel, target, DragAction.Move, DockOperation.Top, true))
                throw new Exception("The packaged policy accepted a document into a tool floating layout.");
            if (!engine.ValidateDockable(first.DockModel, target, DragAction.Move, DockOperation.Top, true))
                throw new Exception("The packaged policy rejected a valid floating tool split.");
            LayoutAnchorableFloatingWindow floating = first.FindParent<LayoutAnchorableFloatingWindow>()
                ?? throw new Exception("The native split lost the public floating owner.");
            if (!ReferenceEquals(floating, second.FindParent<LayoutAnchorableFloatingWindow>()) ||
                floating.Descendents().Any(node => node is LayoutPanel))
                throw new Exception("The native floating split did not preserve its typed pane groups.");
            using StringWriter writer = new();
            XmlLayoutSerializer serializer = new(manager);
            serializer.Serialize(writer);
            serializer.Deserialize(new StringReader(writer.ToString()));
            if (manager.Layout.Descendents().OfType<LayoutAnchorable>().Count() != 2)
                throw new Exception("The packaged floating split lost tool content during XML restoration.");
        }
        finally { manager.Layout.FloatingWindows.Clear(); owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
