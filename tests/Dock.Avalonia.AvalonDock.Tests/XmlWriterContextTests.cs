using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class XmlWriterContextTests
{
    [AvaloniaTheory]
    [InlineData("document", 3)]
    [InlineData("document-group", 4)]
    [InlineData("root-panel", 1)]
    [InlineData("top-side", 1)]
    [InlineData("anchor-group", 2)]
    [InlineData("pinned-tool", 3)]
    [InlineData("hidden-tool", 1)]
    [InlineData("floating-window", 1)]
    [InlineData("floating-document", 2)]
    [InlineData("floating-tool", 4)]
    public void Whole_Layout_Errors_Retain_Their_Serializer_Boundaries(string location, int wrappers)
    {
        ApplicationException failure = new("application hook");
        DockingManager manager = new() { Layout = Create(location, failure) };
        using StringWriter text = new();
        using XmlWriter writer = XmlWriter.Create(text);
        AssertError(Record.Exception(() => new XmlLayoutSerializer(manager).Serialize(writer)), failure, wrappers);
    }

    [AvaloniaTheory]
    [InlineData("document", 2)]
    [InlineData("document-group", 3)]
    [InlineData("root-panel", 0)]
    [InlineData("top-side", 0)]
    [InlineData("anchor-group", 1)]
    [InlineData("pinned-tool", 2)]
    [InlineData("hidden-tool", 0)]
    [InlineData("floating-window", 0)]
    [InlineData("floating-document", 1)]
    [InlineData("floating-tool", 3)]
    public void Direct_Root_Writes_Do_Not_Add_A_Whole_Layout_Error_Boundary(string location, int wrappers)
    {
        ApplicationException failure = new("application hook");
        LayoutRoot root = Create(location, failure);
        using StringWriter text = new();
        using XmlWriter writer = XmlWriter.Create(text);
        writer.WriteStartElement("LayoutRoot");
        AssertError(Record.Exception(() => root.WriteXml(writer)), failure, wrappers);
    }

    [AvaloniaFact]
    public void A_Whole_Layout_Hook_Sees_The_Caller_Writer_Settings_Prefixes_And_Xml_Context()
    {
        using StringWriter text = new();
        using XmlWriter writer = XmlWriter.Create(text, new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment, Indent = true, IndentChars = "\t", NewLineChars = "\r\n"
        });
        XmlWriterSettings? settings = writer.Settings;
        writer.WriteStartElement("Envelope", "urn:envelope");
        writer.WriteAttributeString("xmlns", "custom", null, "urn:application");
        writer.WriteAttributeString("xml", "lang", null, "pl-PL");
        writer.WriteAttributeString("xml", "space", null, "preserve");
        int calls = 0;
        CallbackDocument document = new(w =>
        {
            calls++;
            Assert.NotNull(w.Settings);
            Assert.Equal(settings!.ConformanceLevel, w.Settings.ConformanceLevel);
            Assert.Equal(settings.Indent, w.Settings.Indent);
            Assert.Equal(settings.IndentChars, w.Settings.IndentChars);
            Assert.Equal(settings.NewLineChars, w.Settings.NewLineChars);
            Assert.Equal("custom", w.LookupPrefix("urn:application"));
            Assert.Equal("pl-PL", w.XmlLang);
            Assert.Equal(XmlSpace.Preserve, w.XmlSpace);
            Assert.Equal(WriteState.Element, w.WriteState);
            w.WriteAttributeString("custom", "value", "urn:application", "retained");
        });
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        new XmlLayoutSerializer(manager).Serialize(writer);
        Assert.Equal(1, calls);
        Assert.Contains("</LayoutRoot>", text.ToString()); // Serialize flushed the caller's writer.
        writer.WriteElementString("Following", "still open");
        writer.WriteEndElement();
        writer.Flush();
        XElement envelope = XElement.Parse(text.ToString());
        Assert.NotNull(envelope.Element("LayoutRoot"));
        XElement saved = Assert.Single(envelope.Descendants("LayoutDocument"));
        Assert.Equal("retained", (string?)saved.Attribute(XName.Get("value", "urn:application")));
        Assert.NotNull(envelope.Element(XName.Get("Following", "urn:envelope")));
    }

    [AvaloniaFact]
    public void Direct_Group_Children_Use_Their_Own_Empty_Namespace()
    {
        LayoutDocumentPane pane = new(new LayoutDocument());
        using StringWriter text = new();
        using XmlWriter writer = XmlWriter.Create(text);
        writer.WriteStartElement("Pane", "urn:application");
        pane.WriteXml(writer);
        writer.WriteEndElement();
        writer.Flush();
        Assert.NotNull(XElement.Parse(text.ToString()).Element("LayoutDocument"));
    }

    [AvaloniaFact]
    public void A_Caller_Writer_Failure_Prevents_The_Later_Model_Hook()
    {
        ApplicationException failure = new("writer failed");
        int calls = 0;
        CallbackDocument document = new(_ => calls++);
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        using StringWriter text = new();
        using ObservingWriter writer = new(text) { Start = name => { if (name == "LayoutDocument") throw failure; } };
        AssertError(Record.Exception(() => new XmlLayoutSerializer(manager).Serialize(writer)), failure, 3);
        Assert.Equal(0, calls);
        Assert.Contains("RootPanel", text.ToString());
    }

    [AvaloniaFact]
    public void A_Failing_Model_Leaves_Preceding_Output_In_The_Caller_Writer()
    {
        ApplicationException failure = new("second child");
        LayoutDocumentPane pane = new(new LayoutDocument { Title = "first" });
        pane.Children.Add(new CallbackDocument(_ => throw failure) { Title = "second" });
        pane.Children.Add(new LayoutDocument { Title = "third" });
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        using StringWriter text = new();
        using ObservingWriter writer = new(text);
        AssertError(Record.Exception(() => new XmlLayoutSerializer(manager).Serialize(writer)), failure, 3);
        writer.Flush();
        Assert.Contains("Title=\"first\"", text.ToString());
        Assert.Contains("Title=\"second\"", text.ToString());
        Assert.DoesNotContain("third", text.ToString());
        Assert.DoesNotContain("</LayoutRoot>", text.ToString());
        Assert.NotEqual(WriteState.Closed, writer.WriteState);
    }

    [AvaloniaTheory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 0)]
    public void Flush_Errors_Are_Outside_Their_Own_Serializer_Boundary(int failingFlush, int wrappers)
    {
        IOException failure = new("flush failed");
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(new LayoutDocument())) } };
        using StringWriter text = new();
        using ObservingWriter writer = new(text) { AfterFlush = count => { if (count == failingFlush) throw failure; } };
        AssertError(Record.Exception(() => new XmlLayoutSerializer(manager).Serialize(writer)), failure, wrappers);
        Assert.Equal(failingFlush, writer.Flushes);
    }

    [AvaloniaFact]
    public void Direct_Content_Writes_Keep_Their_Original_Error()
    {
        ApplicationException failure = new("direct content");
        using StringWriter text = new();
        using XmlWriter writer = XmlWriter.Create(text);
        writer.WriteStartElement("Document");
        Assert.Same(failure, Record.Exception(() => new CallbackDocument(_ => throw failure).WriteXml(writer)));
    }

    [AvaloniaTheory]
    [InlineData("text")]
    [InlineData("stream")]
    public void Text_And_Stream_Overloads_Keep_Nested_Errors_And_Leave_Output_Open(string entry)
    {
        ApplicationException failure = new("application hook");
        XmlWriter? retained = null;
        LayoutDocumentPane pane = new(new LayoutDocument { Title = "first" });
        pane.Children.Add(new CallbackDocument(w => { retained = w; throw failure; }));
        XmlLayoutSerializer serializer = new(new DockingManager { Layout = new() { RootPanel = new(pane) } });
        if (entry == "text")
        {
            using StringWriter text = new();
            AssertError(Record.Exception(() => serializer.Serialize(text)), failure, 3);
            Assert.NotNull(retained);
            Assert.NotEqual(WriteState.Closed, retained.WriteState);
            retained.Flush();
            Assert.Contains("RootPanel", text.ToString());
            Assert.DoesNotContain("</LayoutRoot>", text.ToString());
            retained.Close();
            text.Write("caller owns output");
        }
        else
        {
            using MemoryStream stream = new();
            AssertError(Record.Exception(() => serializer.Serialize(stream)), failure, 3);
            Assert.NotNull(retained);
            Assert.NotEqual(WriteState.Closed, retained.WriteState);
            retained.Flush();
            Assert.True(stream.Length > 0);
            Assert.DoesNotContain("</LayoutRoot>", System.Text.Encoding.UTF8.GetString(stream.ToArray()));
            retained.Close();
            Assert.True(stream.CanWrite);
        }
    }

    [AvaloniaTheory]
    [InlineData("text")]
    [InlineData("stream")]
    public void A_Retained_Text_Or_Stream_Writer_Stays_Usable_After_Success(string entry)
    {
        XmlWriter? retained = null;
        XmlLayoutSerializer serializer = new(new DockingManager
        {
            Layout = new() { RootPanel = new(new LayoutDocumentPane(new CallbackDocument(w => retained = w))) }
        });
        using StringWriter text = new();
        using MemoryStream stream = new();
        if (entry == "text") serializer.Serialize(text);
        else serializer.Serialize(stream);
        Assert.NotNull(retained);
        retained.WriteComment("application continuation");
        retained.Flush();
        string output = entry == "text" ? text.ToString() : System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("<!--application continuation-->", output);
        retained.Close();
    }

    private static void AssertError(Exception? actual, Exception failure, int wrappers)
    {
        for (int index = 0; index < wrappers; index++)
        {
            InvalidOperationException wrapper = Assert.IsType<InvalidOperationException>(actual);
            Assert.Equal("There was an error generating the XML document.", wrapper.Message);
            actual = wrapper.InnerException;
        }
        Assert.Same(failure, actual);
    }

    [AvaloniaTheory]
    [InlineData("stream")]
    [InlineData("file")]
    public void Byte_Output_Uses_Utf8_Without_A_Byte_Order_Mark(string entry)
    {
        XmlLayoutSerializer serializer = new(new DockingManager());
        byte[] bytes;
        if (entry == "stream")
        {
            using MemoryStream stream = new();
            serializer.Serialize(stream);
            bytes = stream.ToArray();
        }
        else
        {
            string path = Path.GetTempFileName();
            try { serializer.Serialize(path); bytes = File.ReadAllBytes(path); }
            finally { File.Delete(path); }
        }
        Assert.Equal((byte)'<', bytes[0]);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", System.Text.Encoding.UTF8.GetString(bytes));
    }

    [AvaloniaFact]
    public void Text_Output_Uses_The_Caller_Encoding_In_Its_Declaration()
    {
        using StringWriter text = new();
        new XmlLayoutSerializer(new DockingManager()).Serialize(text);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-16\"?>", text.ToString());
    }

    private static LayoutRoot Create(string location, Exception failure)
    {
        LayoutRoot root = new();
        CallbackDocument document = new(_ => throw failure);
        CallbackTool tool = new(failure);
        switch (location)
        {
            case "document": root.RootPanel = new(new LayoutDocumentPane(document)); break;
            case "document-group": root.RootPanel = new(new LayoutDocumentPaneGroup(new LayoutDocumentPane(document))); break;
            case "root-panel": root.RootPanel = new FailingPanel(failure); break;
            case "top-side": root.TopSide = new FailingSide(failure); break;
            case "anchor-group": root.TopSide.Children.Add(new FailingAnchorGroup(failure)); break;
            case "pinned-tool": LayoutAnchorGroup group = new(); group.Children.Add(tool); root.TopSide.Children.Add(group); break;
            case "hidden-tool": root.Hidden.Add(tool); break;
            case "floating-window": root.FloatingWindows.Add(new FailingWindow(failure) { RootDocument = new LayoutDocument() }); break;
            case "floating-document": root.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = document }); break;
            case "floating-tool": root.FloatingWindows.Add(new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(tool)) }); break;
            default: throw new ArgumentException(location);
        }
        return root;
    }

    private sealed class CallbackDocument(Action<XmlWriter> callback) : LayoutDocument
    {
        public override void WriteXml(XmlWriter writer) { base.WriteXml(writer); callback(writer); }
    }
    private sealed class CallbackTool(Exception error) : LayoutAnchorable
    {
        public override void WriteXml(XmlWriter writer) => throw error;
    }
    private sealed class FailingPanel(Exception error) : LayoutPanel
    {
        public override void WriteXml(XmlWriter writer) => throw error;
    }
    private sealed class FailingSide(Exception error) : LayoutAnchorSide
    {
        public override void WriteXml(XmlWriter writer) => throw error;
    }
    private sealed class FailingAnchorGroup(Exception error) : LayoutAnchorGroup
    {
        public override void WriteXml(XmlWriter writer) => throw error;
    }
    private sealed class FailingWindow(Exception error) : LayoutDocumentFloatingWindow
    {
        public override void WriteXml(XmlWriter writer) => throw error;
    }
    private sealed class ObservingWriter(TextWriter text) : XmlTextWriter(text)
    {
        internal Action<string>? Start { get; init; }
        internal Action<int>? AfterFlush { get; init; }
        internal int Flushes { get; private set; }
        public override void WriteStartElement(string? prefix, string localName, string? ns)
        {
            Start?.Invoke(localName);
            base.WriteStartElement(prefix, localName, ns);
        }
        public override void Flush()
        {
            base.Flush();
            AfterFlush?.Invoke(++Flushes);
        }
    }
}
