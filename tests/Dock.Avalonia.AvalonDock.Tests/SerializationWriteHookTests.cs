using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SerializationWriteHookTests
{
    [AvaloniaTheory]
    [InlineData("text")]
    [InlineData("stream")]
    [InlineData("xml")]
    [InlineData("file")]
    [InlineData("model")]
    public void All_Layout_Writers_Dispatch_Nested_Overrides_Once_And_Restore_Ordinary_Content(string entry)
    {
        List<Probe> probes = new();
        Probe New(string name) { Probe probe = new(name); probes.Add(probe); return probe; }
        MarkedDocument document = new(New("document")) { ContentId = "document", Content = new object(), Description = "Editor" };
        MarkedTool tool = new(New("tool")) { ContentId = "tool", Content = new object(), CanHide = false };
        MarkedDocumentPane documentPane = new(New("document-pane")); documentPane.Children.Add(document);
        MarkedDocumentGroup documentGroup = new(New("document-group")); documentGroup.Children.Add(documentPane);
        MarkedToolPane toolPane = new(New("tool-pane")); toolPane.Children.Add(tool);
        MarkedToolGroup toolGroup = new(New("tool-group")); toolGroup.Children.Add(toolPane);
        MarkedPanel panel = new(New("panel")); panel.Children.Add(documentGroup); panel.Children.Add(toolGroup);
        MarkedAnchorGroup anchorGroup = new(New("anchor-group"));
        anchorGroup.Children.Add(new MarkedTool(New("pinned")) { ContentId = "pinned", Content = new object() });
        MarkedSide side = new(New("side")); side.Children.Add(anchorGroup);
        DockingManager manager = new() { Layout = new() { RootPanel = panel, LeftSide = side } };
        manager.Layout.Hidden.Add(new MarkedTool(New("hidden")) { ContentId = "hidden", Content = new object() });
        manager.Layout.FloatingWindows.Add(new MarkedDocumentWindow(New("document-window"))
        { RootDocument = new MarkedDocument(New("floating-document")) { ContentId = "floating-document", Content = new object() } });
        MarkedToolPane floatingPane = new(New("floating-pane"));
        floatingPane.Children.Add(new MarkedTool(New("floating-tool")) { ContentId = "floating-tool", Content = new object() });
        MarkedToolGroup floatingGroup = new(New("floating-group")); floatingGroup.Children.Add(floatingPane);
        manager.Layout.FloatingWindows.Add(new MarkedToolWindow(New("tool-window")) { RootPanel = floatingGroup });
        Dictionary<string, object?> contents = manager.Layout.Descendents().OfType<LayoutContent>().ToDictionary(x => Assert.IsType<string>(x.ContentId), x => x.Content);
        XmlLayoutSerializer serializer = new(manager);

        string xml = Write(entry, manager, serializer);

        XElement root = XElement.Parse(xml);
        XNamespace metadata = "urn:dock:write-probe";
        Assert.Equal(probes.Count, root.Descendants().Attributes(metadata + "marker").Count());
        foreach (Probe probe in probes)
        {
            Assert.Equal(1, probe.Writes);
            Assert.Single(root.Descendants(), node => (string?)node.Attribute(metadata + "marker") == probe.Name);
        }
        Assert.Contains("Description=\"Editor\"", xml);
        Assert.Contains("CanHide=\"False\"", xml);
        Assert.DoesNotContain("MarkedDocument", xml);
        // These unregistered write-only subclasses append comment nodes. A
        // streaming built-in reader does not consume custom child payloads;
        // applications must register a corresponding ReadXml override for them.
        Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(new StringReader(xml)));
        foreach (XComment comment in root.DescendantNodes().OfType<XComment>().ToArray()) comment.Remove();
        foreach (XElement element in root.Descendants())
            if (!element.Nodes().Any()) element.RemoveNodes();
        serializer.Deserialize(new StringReader(root.ToString()));
        foreach (LayoutContent content in manager.Layout.Descendents().OfType<LayoutContent>())
            Assert.Same(contents[Assert.IsType<string>(content.ContentId)], content.Content);
        Assert.Equal(contents.Count, manager.Layout.Descendents().OfType<LayoutContent>().Count());
    }

    [AvaloniaFact]
    public void A_Failing_Write_Hook_Preserves_The_Existing_File_And_A_Later_Save_Can_Retry()
    {
        FailingDocument document = new() { ContentId = "document", Fail = true };
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        XmlLayoutSerializer serializer = new(manager);
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "previous layout");
            Assert.Throws<InvalidOperationException>(() => serializer.Serialize(path));
            Assert.Equal("previous layout", File.ReadAllText(path));
            document.Fail = false;
            serializer.Serialize(path);
            Assert.Equal("retry", (string?)XElement.Load(path).Descendants("LayoutDocument").Single().Attribute("Custom"));
            Assert.Equal(2, document.Writes);
        }
        finally { File.Delete(path); }
    }

    [AvaloniaFact]
    public void Standalone_Content_Overrides_Can_Read_And_Write_Their_Own_Metadata()
    {
        CustomDocument source = new() { ContentId = "standalone", Marker = "extra & data" };
        using StringWriter text = new();
        using (XmlWriter writer = XmlWriter.Create(text))
        { writer.WriteStartElement("Document"); source.WriteXml(writer); writer.WriteEndElement(); }
        object content = new();
        CustomDocument restored = new() { Content = content };
        using XmlReader reader = XmlReader.Create(new StringReader(text.ToString()));
        restored.ReadXml(reader);
        Assert.Equal(source.Marker, restored.Marker);
        Assert.Equal(source.ContentId, restored.ContentId);
        Assert.Same(content, restored.Content);
        Assert.True(reader.EOF);
    }

    private static string Write(string entry, DockingManager manager, XmlLayoutSerializer serializer)
    {
        using StringWriter text = new();
        if (entry == "text") serializer.Serialize(text);
        else if (entry == "stream")
        {
            using MemoryStream stream = new(); serializer.Serialize(stream);
            Assert.True(stream.CanWrite); stream.Position = 0;
            using StreamReader reader = new(stream, Encoding.UTF8, true, leaveOpen: true); return reader.ReadToEnd();
        }
        else if (entry == "file")
        {
            string path = Path.GetTempFileName();
            try { serializer.Serialize(path); return File.ReadAllText(path); }
            finally { File.Delete(path); }
        }
        else
        {
            using XmlWriter writer = XmlWriter.Create(text, new XmlWriterSettings { CloseOutput = false });
            if (entry == "xml") serializer.Serialize(writer);
            else { writer.WriteStartElement("LayoutRoot"); manager.Layout.WriteXml(writer); writer.WriteEndElement(); }
        }
        return text.ToString();
    }

    private sealed class Probe(string name)
    {
        public string Name { get; } = name;
        public int Writes { get; private set; }
        public void Before(XmlWriter writer)
        {
            Writes++;
            Assert.Equal(WriteState.Element, writer.WriteState);
            Assert.Equal(XmlSpace.None, writer.XmlSpace);
            writer.WriteAttributeString("probe", "marker", "urn:dock:write-probe", Name);
            Assert.Equal("probe", writer.LookupPrefix("urn:dock:write-probe"));
        }
        public void After(XmlWriter writer) => writer.WriteComment("written: " + Name);
    }

    private sealed class FailingDocument : LayoutDocument
    {
        public bool Fail { get; set; }
        public int Writes { get; private set; }
        public override void WriteXml(XmlWriter writer)
        {
            Writes++;
            base.WriteXml(writer);
            if (Fail) throw new InvalidOperationException("Custom serialization failed.");
            writer.WriteAttributeString("Custom", "retry");
        }
    }

    private sealed class CustomDocument : LayoutDocument
    {
        public string? Marker { get; set; }
        public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("Marker", Marker); base.WriteXml(writer); }
        public override void ReadXml(XmlReader reader) { reader.MoveToContent(); Marker = reader.GetAttribute("Marker"); base.ReadXml(reader); }
    }

    private sealed class MarkedDocument(Probe probe) : LayoutDocument
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedTool(Probe probe) : LayoutAnchorable
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedPanel(Probe probe) : LayoutPanel
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedDocumentGroup(Probe probe) : LayoutDocumentPaneGroup
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedToolGroup(Probe probe) : LayoutAnchorablePaneGroup
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedDocumentPane(Probe probe) : LayoutDocumentPane
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedToolPane(Probe probe) : LayoutAnchorablePane
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedSide(Probe probe) : LayoutAnchorSide
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedAnchorGroup(Probe probe) : LayoutAnchorGroup
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedDocumentWindow(Probe probe) : LayoutDocumentFloatingWindow
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }

    private sealed class MarkedToolWindow(Probe probe) : LayoutAnchorableFloatingWindow
    {
        public override void WriteXml(XmlWriter writer) { probe.Before(writer); base.WriteXml(writer); probe.After(writer); }
    }
}
