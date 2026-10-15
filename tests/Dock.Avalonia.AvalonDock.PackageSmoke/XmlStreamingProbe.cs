using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace PackageConsumer;

internal static class XmlStreamingProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("attribute position", () =>
        {
            LayoutDocumentPane pane = new();
            using XmlReader reader = Reader("<Pane DockMinWidth='93' FloatingHeight='bad' />");
            try { pane.ReadXml(reader); throw new Exception("Invalid metadata was accepted."); }
            catch (FormatException) { }
            if (pane.DockMinWidth != 93 || reader.NodeType != XmlNodeType.Attribute || reader.LocalName != "FloatingHeight")
                throw new Exception("The failing attribute or preceding metadata was lost.");
        });
        Check("partial root", () =>
        {
            LayoutRoot root = new();
            LayoutPanel previous = root.RootPanel;
            using XmlReader reader = Reader("<LayoutRoot><RootPanel /><Unknown /></LayoutRoot>");
            try { root.ReadXml(reader); throw new Exception("Unknown XML was accepted."); }
            catch (ArgumentException) { }
            if (ReferenceEquals(previous, root.RootPanel) || reader.LocalName != "Unknown")
                throw new Exception("A completed root section was rolled back.");
        });
        Check("floating replacements", () =>
        {
            LayoutDocumentFloatingWindow window = new();
            int calls = 0;
            window.RootDocumentChanged += (_, _) => calls++;
            using XmlReader reader = Reader("<Window><LayoutDocument ContentId='first' /><LayoutDocument ContentId='second' /></Window><After />");
            window.ReadXml(reader);
            if (calls != 2 || window.RootDocument?.ContentId != "second" || reader.LocalName != "After")
                throw new Exception("Floating children were not restored sequentially.");
        });
        Check("nested errors", () =>
        {
            XmlLayoutSerializer serializer = new(new DockingManager());
            Exception? error = null;
            try { serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutDocumentPane><LayoutDocument CanClose='bad' /></LayoutDocumentPane></RootPanel></LayoutRoot>")); }
            catch (Exception failure) { error = failure; }
            for (int i = 0; i < 3; i++)
                error = error is InvalidOperationException ? error.InnerException : throw new Exception("Missing nested XML error wrapper.");
            if (error is not FormatException) throw new Exception("The original metadata error was lost.");
        });
        Check("external hook context", () =>
        {
            XmlLayoutSerializer serializer = new(new DockingManager());
            StreamingDocument? document = null;
            serializer.KnownTypes.Register("ApplicationDocument", () => document = new StreamingDocument());
            serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
            using XmlReader reader = Reader("<Envelope><LayoutRoot>\n<RootPanel><LayoutDocumentPane><ApplicationDocument ContentId='custom' /><LayoutDocument /></LayoutDocumentPane></RootPanel></LayoutRoot><After /></Envelope>");
            reader.ReadStartElement("Envelope");
            serializer.Deserialize(reader);
            if (document is null || document.Depth != 4 || document.Line != 2 || document.Next != "LayoutDocument" || reader.LocalName != "After")
                throw new Exception("A registered reader lost its source depth, line, or siblings.");
        });
        if (failures.Count != 0) throw new Exception("Packaged XML streaming failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static XmlReader Reader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });
        reader.MoveToContent();
        return reader;
    }

    private sealed class StreamingDocument : LayoutDocument
    {
        public int Depth { get; private set; }
        public int Line { get; private set; }
        public string? Next { get; private set; }
        public override void ReadXml(XmlReader reader)
        {
            Depth = reader.Depth;
            Line = reader is IXmlLineInfo info ? info.LineNumber : 0;
            base.ReadXml(reader);
            Next = reader.LocalName;
        }
    }
}
