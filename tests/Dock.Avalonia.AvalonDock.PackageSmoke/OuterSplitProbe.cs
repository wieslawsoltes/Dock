using System;
using System.IO;
using System.Linq;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class OuterSplitProbe
{
    internal static void Verify()
    {
        foreach (DockOperation direction in new[] { DockOperation.OuterLeft, DockOperation.OuterTop, DockOperation.OuterRight, DockOperation.OuterBottom })
        {
            LayoutAnchorable first = new() { ContentId = "first", Content = "First", CanDockAsTabbedDocument = false };
            LayoutAnchorable second = new() { ContentId = "second", Content = "Second", CanDockAsTabbedDocument = false };
            LayoutAnchorablePane pane = new(first) { Children = { second } };
            LayoutAnchorablePaneGroup source = new(pane);
            LayoutDocumentPane target = new(new LayoutDocument { ContentId = "document", Content = "Document" });
            LayoutDocumentPaneGroup region = new() { Children = { new LayoutDocumentPaneGroup(target) } };
            DockingManager manager = new() { AllowMixedOrientation = true,
                Layout = new() { RootPanel = new() { Children = { region, source } } } };
            DockManager engine = new(new DockService());
            if (!engine.ValidateDockable(source.DockModel, target.DockModel, DragAction.Move, direction, true) ||
                !ReferenceEquals(source.Parent, region.Parent) || !ReferenceEquals(pane.Parent, source) ||
                !ReferenceEquals(first.Parent, pane) || !ReferenceEquals(second.Parent, pane) ||
                !ReferenceEquals(pane.Children[0], first) || !ReferenceEquals(pane.Children[1], second))
                throw new Exception("The packaged outer split lost its tool group, pane identity or tab order.");
            using StringWriter writer = new();
            new XmlLayoutSerializer(manager).Serialize(writer);
            new XmlLayoutSerializer(manager).Deserialize(new StringReader(writer.ToString()));
            LayoutAnchorable restored = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(item => item.ContentId == "first");
            if (restored.Parent is not LayoutAnchorablePane { Parent: LayoutAnchorablePaneGroup } || !Equals(restored.Content, "First"))
                throw new Exception("The packaged outer split did not survive XML restoration.");
        }
    }
}
