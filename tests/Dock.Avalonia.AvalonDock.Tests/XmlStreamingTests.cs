using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class XmlStreamingTests
{
    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    public void A_Later_Syntax_Error_Retains_Group_Metadata_And_Completed_Children(string kind)
    {
        LayoutElement element = Group(kind);
        using XmlReader reader = Reader("<Group DockMinWidth='87'>" + Child(kind) + "\n<");
        Assert.Throws<XmlException>(() => ((IXmlSerializable)element).ReadXml(reader));
        Assert.Equal(87, element.DockModel.MinWidth);
        Assert.Equal(1, ((ILayoutContainer)element).ChildrenCount);
        Assert.Same(element, Assert.Single(((ILayoutContainer)element).Children).Parent);
        if (element is LayoutAnchorablePane tools) Assert.Equal(-1, tools.SelectedContentIndex);
    }

    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    public void Group_Metadata_Failure_Retains_The_Caller_Reader_On_The_Failing_Attribute(string kind)
    {
        LayoutElement element = Group(kind);
        using XmlReader reader = Reader("<Outer>\n<Group DockMinWidth='87' FloatingHeight='invalid' /></Outer>");
        reader.ReadStartElement("Outer");
        reader.MoveToContent();
        Assert.Throws<FormatException>(() => ((IXmlSerializable)element).ReadXml(reader));
        Assert.Equal(87, element.DockModel.MinWidth);
        Assert.Equal(XmlNodeType.Attribute, reader.NodeType);
        Assert.Equal("FloatingHeight", reader.LocalName);
        Assert.Equal("invalid", reader.Value);
        Assert.Equal(2, reader.Depth);
        Assert.Equal(2, ((IXmlLineInfo)reader).LineNumber);
    }

    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    [InlineData("anchor-group")]
    [InlineData("anchor-side")]
    public void An_Unknown_Child_Is_Rejected_After_Preceding_Children_Are_Attached(string kind)
    {
        LayoutElement element = Group(kind);
        using XmlReader reader = Reader("<Group>" + Child(kind) + "<Unregistered /></Group>");
        Assert.Throws<ArgumentException>(() => ((IXmlSerializable)element).ReadXml(reader));
        Assert.Equal("Unregistered", reader.LocalName);
        Assert.Same(element, Assert.Single(((ILayoutContainer)element).Children).Parent);
    }

    [AvaloniaFact]
    public void A_Collection_Observer_Sees_The_Next_Node_And_Its_Error_Is_Not_Wrapped_By_The_Child_Read()
    {
        LayoutDocumentPane pane = new();
        ApplicationException failure = new("observer");
        using XmlReader reader = Reader("<Group><LayoutDocument ContentId='first' /><LayoutDocument ContentId='next' /></Group>");
        pane.Children.CollectionChanged += (_, _) =>
        {
            Assert.Equal(XmlNodeType.Element, reader.NodeType);
            Assert.Equal("next", reader.GetAttribute("ContentId"));
            throw failure;
        };
        Assert.Same(failure, Assert.Throws<ApplicationException>(() => pane.ReadXml(reader)));
        Assert.Equal("first", Assert.Single(pane.Children).ContentId);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Reads_Replace_Each_Child_Immediately_And_Allow_Caller_Element_Names(bool tools)
    {
        LayoutFloatingWindow window = Window(tools);
        List<string?> changes = new();
        window.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == (tools ? "RootPanel" : "RootDocument")) changes.Add(Content(window)?.ContentId);
        };
        using XmlReader reader = Reader("<ApplicationWindow>" + FloatingChild(tools, "first") + FloatingChild(tools, "second") + "</ApplicationWindow><After />");
        window.ReadXml(reader);
        Assert.Equal(new[] { "first", "second" }, changes);
        Assert.Equal("second", Content(window)!.ContentId);
        Assert.Equal("After", reader.LocalName);
        Assert.Equal(450, window.DockWindow.Width);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Reads_Retain_The_First_Replacement_When_A_Later_Child_Fails(bool tools)
    {
        LayoutFloatingWindow window = Window(tools);
        using XmlReader reader = Reader("<Window>" + FloatingChild(tools, "retained") + "<Missing /></Window>");
        Assert.Throws<ArgumentException>(() => window.ReadXml(reader));
        Assert.Equal("retained", Content(window)!.ContentId);
        Assert.Equal("Missing", reader.LocalName);
    }

    [AvaloniaTheory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public void Empty_Floating_Reads_Retain_Existing_Content(bool tools, bool selfClosing)
    {
        LayoutFloatingWindow window = Window(tools);
        using (XmlReader initial = Reader("<Window>" + FloatingChild(tools, "existing") + "</Window>")) window.ReadXml(initial);
        LayoutContent content = Content(window)!;
        using XmlReader reader = Reader(selfClosing ? "<Empty /><Next />" : "<Empty></Empty><Next />");
        window.ReadXml(reader);
        Assert.Same(content, Content(window));
        Assert.Equal("Next", reader.LocalName);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Incoming_Floating_Bounds_Do_Not_Overwrite_The_Outgoing_Child(bool tools)
    {
        LayoutFloatingWindow window = Window(tools);
        using (XmlReader initial = Reader("<Window>" + FloatingChild(tools, "old") + "</Window>")) window.ReadXml(initial);
        LayoutContent old = Content(window)!;
        old.FloatingWidth = 900;
        using XmlReader reader = Reader("<Window>" + FloatingChild(tools, "new") + "</Window>");
        window.ReadXml(reader);
        Assert.Equal(900, old.FloatingWidth);
        Assert.Equal(450, window.DockWindow.Width);
        Assert.Equal(450, Content(window)!.FloatingWidth);
    }

    [AvaloniaFact]
    public void Floating_Xml_Uses_Descendant_Geometry_When_The_New_Root_Has_No_Size()
    {
        LayoutAnchorableFloatingWindow window = new()
        {
            RootPanel = new LayoutAnchorablePaneGroup { FloatingWidth = 900, FloatingHeight = 800 }
        };
        using XmlReader reader = Reader("<Window><LayoutAnchorablePaneGroup><LayoutAnchorablePane FloatingWidth='515' FloatingHeight='415' FloatingLeft='Infinity' FloatingTop='NaN'><LayoutAnchorable IsMaximized='true' /></LayoutAnchorablePane></LayoutAnchorablePaneGroup></Window>");
        window.ReadXml(reader);
        Assert.Equal(515, window.DockWindow.Width);
        Assert.Equal(415, window.DockWindow.Height);
        Assert.Equal(0, window.DockWindow.X);
        Assert.Equal(0, window.DockWindow.Y);
        Assert.True(window.RootPanel!.IsMaximized);
    }

    [AvaloniaFact]
    public void Root_Panel_Replacement_Is_Published_Before_Its_Staged_Children_Are_Attached()
    {
        LayoutRoot root = new();
        List<int> counts = new();
        root.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(root.RootPanel)) counts.Add(root.RootPanel.ChildrenCount);
        };
        using XmlReader reader = Reader("<LayoutRoot><RootPanel><LayoutDocumentPane /></RootPanel><Unknown /></LayoutRoot>");
        Assert.Throws<ArgumentException>(() => root.ReadXml(reader));
        Assert.Equal(new[] { 0 }, counts);
        Assert.IsType<LayoutDocumentPane>(Assert.Single(root.RootPanel.Children));
        Assert.Equal("Unknown", reader.LocalName);
    }

    [AvaloniaTheory]
    [InlineData("TopSide")]
    [InlineData("RightSide")]
    [InlineData("LeftSide")]
    [InlineData("BottomSide")]
    public void Root_Side_Failure_Leaves_The_Replacement_Empty_And_Retains_Previous_Sections(string section)
    {
        LayoutRoot root = new();
        LayoutPanel previous = root.RootPanel;
        LayoutAnchorSide oldSide = Side(root, section);
        using XmlReader reader = Reader("<LayoutRoot><RootPanel /><" + section + "><LayoutAnchorGroup><LayoutAnchorable /></LayoutAnchorGroup><Unknown /></" + section + "></LayoutRoot>");
        Assert.Throws<ArgumentException>(() => root.ReadXml(reader));
        Assert.NotSame(previous, root.RootPanel);
        Assert.NotSame(oldSide, Side(root, section));
        Assert.Empty(Side(root, section).Children);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Root_Collection_Failure_Clears_Old_Contents_Without_Attaching_Partial_Results(bool hidden)
    {
        LayoutRoot root = new();
        root.Hidden.Add(new LayoutAnchorable { ContentId = "old" });
        root.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument { ContentId = "old" } });
        string name = hidden ? "Hidden" : "FloatingWindows";
        string child = hidden ? "<LayoutAnchorable ContentId='new' />" : "<LayoutDocumentFloatingWindow><LayoutDocument ContentId='new' /></LayoutDocumentFloatingWindow>";
        using XmlReader reader = Reader("<LayoutRoot><RootPanel /><" + name + ">" + child + "<Unknown /></" + name + "></LayoutRoot>");
        Assert.Throws<ArgumentException>(() => root.ReadXml(reader));
        if (hidden) { Assert.Empty(root.Hidden); Assert.Single(root.FloatingWindows); }
        else { Assert.Empty(root.FloatingWindows); Assert.Single(root.Hidden); }
    }

    [AvaloniaFact]
    public void Root_Panel_Child_Failure_Preserves_The_Previous_Panel()
    {
        LayoutRoot root = new();
        LayoutPanel previous = root.RootPanel;
        using XmlReader reader = Reader("<LayoutRoot><RootPanel><LayoutDocumentPane /><Unknown /></RootPanel></LayoutRoot>");
        Assert.Throws<ArgumentException>(() => root.ReadXml(reader));
        Assert.Same(previous, root.RootPanel);
        Assert.Same(root, previous.Parent);
    }

    [AvaloniaFact]
    public void Registered_Hooks_Observe_Document_Depth_Line_Information_And_Real_Siblings()
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        TrackedPane? pane = null;
        TrackedDocument? document = null;
        serializer.KnownTypes.Register("ApplicationPane", () => pane = new TrackedPane());
        serializer.KnownTypes.Register("ApplicationDocument", () => document = new TrackedDocument());
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        const string xml = "<Envelope>\n<LayoutRoot>\n<RootPanel>\n<ApplicationPane>\n<ApplicationDocument ContentId='first' />\n<LayoutDocument ContentId='second' />\n</ApplicationPane>\n<LayoutDocumentPane />\n</RootPanel>\n</LayoutRoot><After /></Envelope>";
        using XmlReader reader = Reader(xml);
        reader.ReadStartElement("Envelope");
        serializer.Deserialize(reader);
        Assert.NotNull(pane);
        Assert.NotNull(document);
        Assert.Equal(3, pane.BeforeDepth);
        Assert.Equal(4, document.BeforeDepth);
        Assert.Equal(5, document.BeforeLine);
        Assert.Equal("LayoutDocument", document.AfterName);
        Assert.Equal("LayoutDocumentPane", pane.AfterName);
        Assert.Equal("After", reader.LocalName);
        Assert.Equal(2, pane.ChildrenCount);
        Assert.False(reader.EOF);
    }

    [AvaloniaTheory]
    [InlineData("<RootPanel DockMinWidth='invalid' />", 1)]
    [InlineData("<RootPanel><LayoutDocumentPane DockMinWidth='invalid' /></RootPanel>", 2)]
    [InlineData("<RootPanel><LayoutDocumentPane><LayoutDocument CanClose='invalid' /></LayoutDocumentPane></RootPanel>", 3)]
    public void Deserialization_Wraps_Each_Model_Boundary_And_Retains_The_Original_Error(string contents, int wrappers)
    {
        DockingManager manager = new();
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer serializer = new(manager);
        using XmlReader reader = Reader("<LayoutRoot>\n" + contents + "</LayoutRoot>");
        Exception? error = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(reader));
        for (int i = 0; i < wrappers; i++)
        {
            InvalidOperationException wrapper = Assert.IsType<InvalidOperationException>(error);
            Assert.Contains("XML document (2,", wrapper.Message);
            error = wrapper.InnerException;
        }
        Assert.IsType<FormatException>(error);
        Assert.Equal(XmlNodeType.Attribute, reader.NodeType);
        Assert.Same(previous, manager.Layout);
    }

    [AvaloniaFact]
    public void Custom_Read_Hook_Errors_Keep_Their_Identity_Inside_The_Xml_Wrappers()
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        ApplicationException failure = new("custom read");
        serializer.KnownTypes.Register("ApplicationDocument", () => new TrackedDocument { Failure = failure });
        Exception? error = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutDocumentPane><ApplicationDocument /></LayoutDocumentPane></RootPanel></LayoutRoot>")));
        for (int i = 0; i < 3; i++) error = Assert.IsType<InvalidOperationException>(error).InnerException;
        Assert.Same(failure, error);
    }

    [AvaloniaFact]
    public void Content_Recovery_Errors_Are_Outside_The_Xml_Wrapper_And_The_Serializer_Remains_Usable()
    {
        DockingManager manager = new();
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer serializer = new(manager);
        ApplicationException failure = new("content recovery");
        bool fail = true;
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            if (fail) throw failure;
            args.Content = new object();
        };
        const string xml = "<LayoutRoot><RootPanel><LayoutDocumentPane><LayoutDocument ContentId='document' /></LayoutDocumentPane></RootPanel></LayoutRoot>";
        Assert.Same(failure, Assert.Throws<ApplicationException>(() => serializer.Deserialize(new StringReader(xml))));
        Assert.Same(previous, manager.Layout);
        fail = false;
        serializer.Deserialize(new StringReader(xml));
        Assert.NotSame(previous, manager.Layout);
        Assert.NotNull(Assert.Single(manager.Layout.Descendents().OfType<LayoutContent>()).Content);
    }

    private static XmlReader Reader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });
        reader.MoveToContent();
        return reader;
    }

    private static LayoutElement Group(string kind) => kind switch
    {
        "panel" => new LayoutPanel(), "document-pane" => new LayoutDocumentPane(), "tool-pane" => new LayoutAnchorablePane(),
        "document-group" => new LayoutDocumentPaneGroup(), "tool-group" => new LayoutAnchorablePaneGroup(),
        "anchor-group" => new LayoutAnchorGroup(), "anchor-side" => new LayoutAnchorSide(),
        _ => throw new ArgumentException(nameof(kind))
    };

    private static string Child(string kind) => kind switch
    {
        "panel" or "document-group" => "<LayoutDocumentPane />", "tool-group" => "<LayoutAnchorablePane />",
        "document-pane" => "<LayoutDocument />", "anchor-side" => "<LayoutAnchorGroup />", _ => "<LayoutAnchorable />"
    };

    private static LayoutFloatingWindow Window(bool tools) => tools ? new LayoutAnchorableFloatingWindow() : new LayoutDocumentFloatingWindow();
    private static LayoutContent? Content(LayoutFloatingWindow window) => window.Descendents().OfType<LayoutContent>().SingleOrDefault();
    private static string FloatingChild(bool tools, string id) => tools
        ? "<LayoutAnchorablePaneGroup FloatingWidth='450'><LayoutAnchorablePane><LayoutAnchorable ContentId='" + id + "' /></LayoutAnchorablePane></LayoutAnchorablePaneGroup>"
        : "<LayoutDocument ContentId='" + id + "' FloatingWidth='450' />";
    private static LayoutAnchorSide Side(LayoutRoot root, string name) => name switch
    {
        "TopSide" => root.TopSide!, "RightSide" => root.RightSide!, "LeftSide" => root.LeftSide!, _ => root.BottomSide!
    };

    private sealed class TrackedPane : LayoutDocumentPane
    {
        public int BeforeDepth { get; private set; }
        public string? AfterName { get; private set; }
        public override void ReadXml(XmlReader reader)
        {
            BeforeDepth = reader.Depth;
            base.ReadXml(reader);
            reader.MoveToContent();
            AfterName = reader.LocalName;
        }
    }

    private sealed class TrackedDocument : LayoutDocument
    {
        public int BeforeDepth { get; private set; }
        public int BeforeLine { get; private set; }
        public string? AfterName { get; private set; }
        public ApplicationException? Failure { get; init; }
        public override void ReadXml(XmlReader reader)
        {
            if (Failure is { } failure) throw failure;
            BeforeDepth = reader.Depth;
            BeforeLine = ((IXmlLineInfo)reader).LineNumber;
            base.ReadXml(reader);
            reader.MoveToContent();
            AfterName = reader.LocalName;
        }
    }
}
