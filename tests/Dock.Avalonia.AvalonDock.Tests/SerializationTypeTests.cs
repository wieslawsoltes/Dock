using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SerializationTypeTests
{
    [AvaloniaFact]
    public void Registered_Nested_Types_Retain_Metadata_Content_Selection_And_Active_State()
    {
        object payload = new();
        CustomDocument document = new() { ContentId = "editor", Content = payload, Marker = "document & metadata", Description = "description" };
        CustomPane pane = new() { Marker = "pane metadata", ShowHeader = false };
        pane.Children.Add(new LayoutDocument { ContentId = "other", Content = new object() });
        pane.Children.Add(document);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        document.IsActive = true;
        XmlLayoutSerializer serializer = CreateSerializer(manager);

        string xml = Save(serializer);
        Assert.Single(XElement.Parse(xml).Descendants("ApplicationPane"));
        Assert.Single(XElement.Parse(xml).Descendants("ApplicationDocument"));
        serializer.Deserialize(new StringReader(xml));

        CustomPane restoredPane = Assert.Single(manager.Layout.Descendents().OfType<CustomPane>());
        CustomDocument restored = Assert.Single(restoredPane.Children.OfType<CustomDocument>());
        Assert.Equal("pane metadata", restoredPane.Marker);
        Assert.False(restoredPane.ShowHeader);
        Assert.Equal("document & metadata", restored.Marker);
        Assert.Equal("description", restored.Description);
        Assert.Same(payload, restored.Content);
        Assert.Same(restored, restoredPane.SelectedContent);
        Assert.Same(restored, manager.Layout.ActiveContent);
        Assert.Equal(new[] { "before", "after" }, restored.ReadEvents);
        Assert.Equal(new[] { "before", "after" }, restoredPane.ReadEvents);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Registered_Floating_Window_Types_Restore_Their_Nested_Custom_Contents(bool tools)
    {
        DockingManager manager = new();
        object payload = new();
        if (tools)
            manager.Layout.FloatingWindows.Add(new CustomToolWindow
            {
                Marker = "tool window",
                RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new CustomTool { ContentId = "tool", Content = payload, Marker = "tool" }))
            });
        else
            manager.Layout.FloatingWindows.Add(new CustomDocumentWindow
            {
                Marker = "document window",
                RootDocument = new CustomDocument { ContentId = "document", Content = payload, Marker = "document" }
            });
        XmlLayoutSerializer serializer = CreateSerializer(manager);
        serializer.Deserialize(new StringReader(Save(serializer)));
        LayoutFloatingWindow window = Assert.Single(manager.Layout.FloatingWindows);
        LayoutContent restored = Assert.Single(window.Descendents().OfType<LayoutContent>());
        Assert.Same(payload, restored.Content);
        Assert.True(restored.IsFloating);
        if (tools)
        {
            Assert.Equal("tool window", Assert.IsType<CustomToolWindow>(window).Marker);
            Assert.Equal("tool", Assert.IsType<CustomTool>(restored).Marker);
        }
        else
        {
            Assert.Equal("document window", Assert.IsType<CustomDocumentWindow>(window).Marker);
            Assert.Equal("document", Assert.IsType<CustomDocument>(restored).Marker);
        }
    }

    [AvaloniaFact]
    public void Hidden_Custom_Tools_Are_Restored_With_Their_Return_Container()
    {
        CustomTool tool = new() { ContentId = "tool", Content = new object(), Marker = "hidden metadata" };
        LayoutAnchorablePane pane = new(tool);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel { Children = { pane, new LayoutDocumentPane() } } } };
        tool.Hide();
        XmlLayoutSerializer serializer = CreateSerializer(manager);
        serializer.Deserialize(new StringReader(Save(serializer)));
        CustomTool restored = Assert.IsType<CustomTool>(Assert.Single(manager.Layout.Hidden));
        Assert.Equal("hidden metadata", restored.Marker);
        restored.Show();
        Assert.IsType<LayoutAnchorablePane>(restored.Parent);
        Assert.Same(tool.Content, restored.Content);
    }

    [AvaloniaTheory]
    [InlineData("LayoutDocument")]
    [InlineData("RootPanel")]
    [InlineData("Hidden")]
    public void Builtin_Xml_Names_Cannot_Be_Replaced(string name)
    {
        LayoutSerializationTypes types = new();
        Assert.Throws<ArgumentException>(() => types.Register(name, static () => new CustomDocument()));
    }

    [AvaloniaFact]
    public void Registry_Rejects_Invalid_And_Ambiguous_Entries()
    {
        LayoutSerializationTypes types = new();
        Assert.Throws<ArgumentNullException>(() => types.Register<CustomDocument>(null!, static () => new()));
        Assert.Throws<ArgumentNullException>(() => types.Register<CustomDocument>("Document", null!));
        Assert.Throws<XmlException>(() => types.Register("not a name", static () => new CustomDocument()));
        types.Register("Document", static () => new CustomDocument());
        Assert.Throws<ArgumentException>(() => types.Register("Document", static () => new CustomTool()));
        Assert.Throws<ArgumentException>(() => types.Register("OtherDocument", static () => new CustomDocument()));
    }

    [AvaloniaTheory]
    [InlineData("unknown")]
    [InlineData("throws")]
    [InlineData("null")]
    [InlineData("attached")]
    [InlineData("derived")]
    public void Invalid_Custom_Nodes_Leave_The_Managers_Existing_Layout_Intact(string failure)
    {
        DockingManager manager = new();
        LayoutRoot previous = manager.Layout;
        LayoutDocument attached = new();
        LayoutDocumentPane pane = new(attached);
        XmlLayoutSerializer serializer = new(manager);
        if (failure != "unknown")
            serializer.KnownTypes.Register<LayoutDocument>("ApplicationDocument", () => failure switch
            {
                "throws" => throw new InvalidOperationException("factory failure"),
                "null" => null!,
                "attached" => attached,
                _ => new CustomDocument()
            });
        string xml = "<LayoutRoot><RootPanel><LayoutDocumentPane><ApplicationDocument ContentId='custom'/></LayoutDocumentPane></RootPanel></LayoutRoot>";
        if (failure == "unknown") Assert.Throws<XmlException>(() => serializer.Deserialize(new StringReader(xml)));
        else Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(new StringReader(xml)));
        Assert.Same(previous, manager.Layout);
        Assert.Same(pane, attached.Parent);
    }

    [AvaloniaFact]
    public void Read_Hook_Registrations_Apply_To_The_Next_Operation_And_Do_Not_Change_The_Current_Snapshot()
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        bool registered = false;
        serializer.KnownTypes.Register("ApplicationDocument", () => new CustomDocument
        {
            BeforeRead = () =>
            {
                if (registered) return;
                registered = true;
                serializer.KnownTypes.Register("ApplicationTool", static () => new CustomTool());
            }
        });
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        const string xml = "<LayoutRoot><RootPanel><LayoutDocumentPane><ApplicationDocument/><ApplicationTool/></LayoutDocumentPane></RootPanel></LayoutRoot>";
        Assert.Throws<XmlException>(() => serializer.Deserialize(new StringReader(xml)));
        serializer.Deserialize(new StringReader(xml));
        Assert.Single(manager.Layout.Descendents().OfType<CustomDocument>());
        Assert.Single(manager.Layout.Descendents().OfType<CustomTool>());
    }

    private static XmlLayoutSerializer CreateSerializer(DockingManager manager)
    {
        XmlLayoutSerializer serializer = new(manager);
        serializer.KnownTypes.Register("ApplicationDocument", static () => new CustomDocument());
        serializer.KnownTypes.Register("ApplicationTool", static () => new CustomTool());
        serializer.KnownTypes.Register("ApplicationPane", static () => new CustomPane());
        serializer.KnownTypes.Register("ApplicationDocumentWindow", static () => new CustomDocumentWindow());
        serializer.KnownTypes.Register("ApplicationToolWindow", static () => new CustomToolWindow());
        return serializer;
    }

    private static string Save(XmlLayoutSerializer serializer)
    {
        using StringWriter writer = new();
        serializer.Serialize(writer);
        return writer.ToString();
    }

    private sealed class CustomDocument : LayoutDocument
    {
        public string? Marker { get; set; }
        public Action? BeforeRead { get; init; }
        public List<string> ReadEvents { get; } = new();
        public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("Marker", Marker); base.WriteXml(writer); }
        public override void ReadXml(XmlReader reader)
        {
            ReadEvents.Add("before");
            BeforeRead?.Invoke();
            Marker = reader.GetAttribute("Marker");
            base.ReadXml(reader);
            ReadEvents.Add("after");
        }
    }

    private sealed class CustomTool : LayoutAnchorable
    {
        public string? Marker { get; set; }
        public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("Marker", Marker); base.WriteXml(writer); }
        public override void ReadXml(XmlReader reader) { Marker = reader.GetAttribute("Marker"); base.ReadXml(reader); }
    }

    private sealed class CustomPane : LayoutDocumentPane
    {
        public string? Marker { get; set; }
        public List<string> ReadEvents { get; } = new();
        public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("Marker", Marker); base.WriteXml(writer); }
        public override void ReadXml(XmlReader reader)
        {
            ReadEvents.Add("before");
            Marker = reader.GetAttribute("Marker");
            base.ReadXml(reader);
            ReadEvents.Add("after");
        }
    }

    private sealed class CustomDocumentWindow : LayoutDocumentFloatingWindow
    {
        public string? Marker { get; set; }
        public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("Marker", Marker); base.WriteXml(writer); }
        public override void ReadXml(XmlReader reader) { Marker = reader.GetAttribute("Marker"); base.ReadXml(reader); }
    }

    private sealed class CustomToolWindow : LayoutAnchorableFloatingWindow
    {
        public string? Marker { get; set; }
        public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("Marker", Marker); base.WriteXml(writer); }
        public override void ReadXml(XmlReader reader) { Marker = reader.GetAttribute("Marker"); base.ReadXml(reader); }
    }
}
