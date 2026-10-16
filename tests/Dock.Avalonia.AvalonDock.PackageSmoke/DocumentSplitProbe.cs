using System;
using System.IO;
using System.Linq;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class DocumentSplitProbe
{
    internal static void Verify()
    {
        foreach (DockOperation direction in new[] { DockOperation.Left, DockOperation.Top, DockOperation.Right, DockOperation.Bottom })
        {
            LayoutAnchorable first = new() { ContentId = "first", Content = "First" };
            LayoutAnchorable second = new() { ContentId = "second", Content = "Second" };
            LayoutAnchorable third = new() { ContentId = "third", Content = "Third" };
            LayoutAnchorablePane source = new(first) { Children = { second, third } };
            LayoutDocumentPane target = new(new LayoutDocument { ContentId = "document", Content = "Document" });
            DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { target, source } } } };
            DockManager engine = new(new DockService());

            if (!engine.ValidateDockable(source.DockModel, target.DockModel, DragAction.Move, direction, true) ||
                first.Parent is not LayoutDocumentPane created ||
                created.Parent is not LayoutDocumentPaneGroup || !ReferenceEquals(created.Parent, target.Parent) ||
                created.Children.Count != 3 || !ReferenceEquals(created.Children[0], first) ||
                !ReferenceEquals(created.Children[1], second) || !ReferenceEquals(created.Children[2], third))
                throw new Exception("The packaged document split lost its pane type, document group or tab order.");

            using StringWriter writer = new();
            new XmlLayoutSerializer(manager).Serialize(writer);
            new XmlLayoutSerializer(manager).Deserialize(new StringReader(writer.ToString()));
            LayoutAnchorable restored = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(item => item.ContentId == "first");
            if (restored.Parent is not LayoutDocumentPane { Parent: LayoutDocumentPaneGroup } ||
                !Equals(restored.Content, "First"))
                throw new Exception("The packaged split layout did not survive XML restoration.");
        }
    }
}
