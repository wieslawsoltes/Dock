using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace PackageConsumer;

internal static class SerializationProbe
{
    internal static void Verify()
    {
        CustomDocument document = new() { ContentId = "shared", Content = new object() };
        LayoutAnchorable tool = new() { ContentId = "shared", Content = new object() };
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children =
        { new LayoutDocumentPane(document), new LayoutAnchorablePane(tool) } } } };
        XmlLayoutSerializer serializer = new(manager);
        using StringWriter text = new();
        serializer.Serialize(text);
        if (document.Writes != 1 || !text.ToString().Contains("Custom=\"external\""))
            throw new Exception("The packaged serializer bypassed the external WriteXml override.");
        serializer.Deserialize(new StringReader(text.ToString()));
        if (!ReferenceEquals(document.Content, manager.Layout.Descendents().OfType<LayoutDocument>().Single().Content) ||
            !ReferenceEquals(tool.Content, manager.Layout.Descendents().OfType<LayoutAnchorable>().Single().Content))
            throw new Exception("The packaged serializer confused matching document and tool identifiers.");

        XmlLayoutSerializer callbackSerializer = new(manager);
        List<string> order = new();
        int closing = 0;
        callbackSerializer.LayoutSerializationCallback += (_, args) =>
        {
            order.Add(args.Model is LayoutAnchorable ? "tool" : "document");
            if (args.Model is not LayoutAnchorable) return;
            args.Model.CanClose = false;
            args.Model.Closing += (_, e) => { closing++; e.Cancel = true; };
            args.Cancel = true;
        };
        callbackSerializer.Deserialize(new StringReader(text.ToString()));
        if (!order.SequenceEqual(new[] { "tool", "document" }) || closing != 1 ||
            manager.Layout.Descendents().OfType<LayoutAnchorable>().Single().Content is not null)
            throw new Exception("The packaged serializer changed callback phases or bypassed closing cancellation.");
    }

    private sealed class CustomDocument : LayoutDocument
    {
        public int Writes { get; private set; }
        public override void WriteXml(XmlWriter writer)
        {
            Writes++;
            base.WriteXml(writer);
            writer.WriteAttributeString("Custom", "external");
        }
    }
}
