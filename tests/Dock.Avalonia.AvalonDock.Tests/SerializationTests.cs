using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SerializationTests
{
    [AvaloniaFact]
    public void Round_Trip_Preserves_Content_Selection_And_Pane_Constraints()
    {
        object firstContent = new();
        object secondContent = new();
        LayoutDocument first = new() { ContentId = "one", Title = "First", Content = firstContent, CanFloat = false };
        LayoutDocument second = new() { ContentId = "two", Title = "Second", Content = secondContent, Description = "Editor", CanMove = false };
        LayoutDocumentPane pane = new(first) { DockWidth = new GridLength(2, GridUnitType.Star), DockMinWidth = 123 };
        pane.Children.Add(second);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        manager.Layout.ActiveContent = second;
        second.IsSelected = true;
        XmlLayoutSerializer serializer = new(manager);
        StringWriter writer = new();
        serializer.Serialize(writer);
        Assert.Contains("<RootPanel", writer.ToString());
        Assert.DoesNotContain("DockModel", writer.ToString());
        serializer.Deserialize(new StringReader(writer.ToString()));
        LayoutDocument[] documents = manager.Layout.Descendents().OfType<LayoutDocument>().ToArray();
        Assert.Equal(2, documents.Length);
        Assert.Same(firstContent, documents[0].Content);
        Assert.Same(secondContent, documents[1].Content);
        Assert.False(documents[0].CanFloat);
        Assert.False(documents[1].CanMove);
        Assert.True(documents[1].IsSelected);
        Assert.Equal("Editor", documents[1].Description);
        LayoutDocumentPane restoredPane = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocumentPane>());
        Assert.Equal(new GridLength(2, GridUnitType.Star), restoredPane.DockWidth);
        Assert.Equal(123, restoredPane.DockMinWidth);
    }

    [AvaloniaFact]
    public void Imports_Upstream_Xml_And_Invokes_Content_Callbacks()
    {
        const string xml = """
            <LayoutRoot>
              <RootPanel Orientation="Horizontal">
                <LayoutDocumentPaneGroup Orientation="Vertical">
                  <LayoutDocumentPane>
                    <LayoutDocument Title="Editor" ContentId="editor" IsSelected="True" CanClose="False" />
                    <LayoutDocument Title="Missing" ContentId="missing" />
                  </LayoutDocumentPane>
                </LayoutDocumentPaneGroup>
              </RootPanel>
              <TopSide /><RightSide /><LeftSide /><BottomSide /><FloatingWindows /><Hidden />
            </LayoutRoot>
            """;
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        object editor = new();
        int callbacks = 0;
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            callbacks++;
            if (args.Model.ContentId == "editor") args.Content = editor;
            else args.Cancel = true;
        };
        serializer.Deserialize(new StringReader(xml));
        LayoutDocument restored = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
        Assert.Same(editor, restored.Content);
        Assert.False(restored.CanClose);
        Assert.Equal(2, callbacks);
    }

    [AvaloniaFact]
    public void Hidden_Tool_Round_Trip_Restores_To_Original_Pane()
    {
        LayoutAnchorable tool = new() { ContentId = "properties", Content = new object() };
        LayoutAnchorablePane pane = new(tool);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        tool.Hide();
        Assert.Same(tool, Assert.Single(manager.Layout.Hidden));
        StringWriter writer = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        LayoutAnchorable hidden = Assert.Single(manager.Layout.Hidden);
        Assert.True(hidden.IsHidden);
        hidden.Show();
        Assert.Empty(manager.Layout.Hidden);
        Assert.Same(hidden, Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorablePane>()).Children[0]);
    }

    [AvaloniaTheory]
    [InlineData("<Unexpected />")]
    [InlineData("<LayoutRoot><RootPanel><Unknown /></RootPanel></LayoutRoot>")]
    [InlineData("<!DOCTYPE x [<!ENTITY external SYSTEM 'file:///should-not-be-read'>]><LayoutRoot>&external;</LayoutRoot>")]
    [InlineData("<LayoutRoot><RootPanel Orientation='Sideways' /></LayoutRoot>")]
    public void Invalid_Xml_Does_Not_Replace_The_Current_Layout(string xml)
    {
        DockingManager manager = new();
        LayoutRoot previous = manager.Layout;
        Assert.Throws<XmlException>(() => new XmlLayoutSerializer(manager).Deserialize(new StringReader(xml)));
        Assert.Same(previous, manager.Layout);
    }

    [AvaloniaFact]
    public void Caller_Owned_Streams_Remain_Open_And_File_Overloads_Work()
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        using MemoryStream stream = new();
        serializer.Serialize(stream);
        Assert.True(stream.CanWrite);
        stream.Position = 0;
        serializer.Deserialize(stream);
        Assert.True(stream.CanRead);
        string path = Path.GetTempFileName();
        try
        {
            serializer.Serialize(path);
            serializer.Deserialize(path);
            Assert.Contains("LayoutRoot", File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }

    [AvaloniaFact]
    public void Callback_Failure_Does_Not_Replace_Current_Layout()
    {
        DockingManager manager = new();
        LayoutRoot previous = manager.Layout;
        XmlLayoutSerializer serializer = new(manager);
        serializer.LayoutSerializationCallback += (_, _) => throw new InvalidOperationException("Content unavailable");
        Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutDocumentPane><LayoutDocument ContentId='one' /></LayoutDocumentPane></RootPanel></LayoutRoot>")));
        Assert.Same(previous, manager.Layout);
    }
}
