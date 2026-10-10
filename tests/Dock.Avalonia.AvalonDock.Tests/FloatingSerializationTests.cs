using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingSerializationTests
{
    [AvaloniaFact]
    public void Floating_Document_And_Tool_Round_Trip_Preserves_Content_Geometry_And_Return_Pane()
    {
        object documentContent = new();
        object toolContent = new();
        LayoutDocument document = new()
        {
            ContentId = "document", Content = documentContent, FloatingLeft = 80, FloatingTop = 100,
            FloatingWidth = 620, FloatingHeight = 460, IsMaximized = true
        };
        LayoutDocumentPane pane = new(document);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        document.Float();
        LayoutAnchorableFloatingWindow toolWindow = new()
        {
            RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable
            {
                ContentId = "tool", Content = toolContent
            })) { FloatingLeft = 120, FloatingTop = 140, FloatingWidth = 500, FloatingHeight = 350 }
        };
        manager.Layout.FloatingWindows.Add(toolWindow);
        XmlLayoutSerializer serializer = new(manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        Assert.Contains("<LayoutDocumentFloatingWindow>", writer.ToString());
        Assert.Contains("<LayoutAnchorableFloatingWindow>", writer.ToString());
        serializer.Deserialize(new StringReader(writer.ToString()));

        Assert.Equal(2, manager.Layout.FloatingWindows.Count);
        LayoutDocumentFloatingWindow restoredDocumentWindow = manager.Layout.FloatingWindows.OfType<LayoutDocumentFloatingWindow>().Single();
        LayoutDocument restoredDocument = restoredDocumentWindow.RootDocument!;
        Assert.Same(documentContent, restoredDocument.Content);
        Assert.Same(manager.Layout, restoredDocument.Root);
        Assert.Equal(80, restoredDocumentWindow.DockWindow.X);
        Assert.Equal(100, restoredDocumentWindow.DockWindow.Y);
        Assert.Equal(620, restoredDocumentWindow.DockWindow.Width);
        Assert.Equal(460, restoredDocumentWindow.DockWindow.Height);
        Assert.Equal(DockWindowState.Maximized, restoredDocumentWindow.DockWindow.WindowState);
        LayoutAnchorableFloatingWindow restoredToolWindow = manager.Layout.FloatingWindows.OfType<LayoutAnchorableFloatingWindow>().Single();
        LayoutAnchorable restoredTool = restoredToolWindow.Descendents().OfType<LayoutAnchorable>().Single();
        Assert.Same(toolContent, restoredTool.Content);
        Assert.Equal(500, restoredToolWindow.RootPanel!.FloatingWidth);
        Assert.Equal(350, restoredToolWindow.DockWindow.Height);
        Assert.Equal(120, restoredTool.FloatingLeft);
        Assert.Equal(140, restoredTool.FloatingTop);
        LayoutDocumentPane returnPane = Assert.IsType<LayoutDocumentPane>(restoredDocument.PreviousContainer);
        Assert.Same(manager.Layout, returnPane.Root);
        restoredDocument.Dock();
        Assert.Same(restoredDocument, Assert.Single(returnPane.Children));
        Assert.Single(manager.Layout.FloatingWindows);
        Assert.False(restoredDocument.IsFloating);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Xml_Interface_Consumes_One_Element_And_Preserves_Metadata(bool tool)
    {
        LayoutFloatingWindow original = tool
            ? new LayoutAnchorableFloatingWindow
            {
                RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable { ContentId = "content" }))
                { FloatingWidth = 560, FloatingHeight = 410, FloatingLeft = 30, FloatingTop = 50, IsMaximized = true }
            }
            : new LayoutDocumentFloatingWindow
            {
                RootDocument = new LayoutDocument
                { ContentId = "content", FloatingWidth = 560, FloatingHeight = 410, FloatingLeft = 30, FloatingTop = 50, IsMaximized = true }
            };
        string elementName = tool ? nameof(LayoutAnchorableFloatingWindow) : nameof(LayoutDocumentFloatingWindow);
        using StringWriter text = new();
        using (XmlWriter writer = XmlWriter.Create(text, new XmlWriterSettings { OmitXmlDeclaration = true }))
        {
            writer.WriteStartElement("Container");
            writer.WriteStartElement(elementName);
            ((IXmlSerializable)original).WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteElementString("After", "Still readable");
            writer.WriteEndElement();
        }
        LayoutFloatingWindow restored = tool
            ? new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup { FloatingWidth = 900, FloatingHeight = 800 } }
            : new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() };
        using XmlReader reader = XmlReader.Create(new StringReader(text.ToString()));
        reader.MoveToContent();
        reader.ReadStartElement("Container");
        ((IXmlSerializable)restored).ReadXml(reader);
        Assert.Null(((IXmlSerializable)restored).GetSchema());
        Assert.Equal("Still readable", reader.ReadElementContentAsString("After", ""));
        Assert.Equal("content", Assert.Single(restored.Descendents().OfType<LayoutContent>()).ContentId);
        Assert.Equal(560, restored.DockWindow.Width);
        Assert.Equal(410, restored.DockWindow.Height);
        Assert.Equal(30, restored.DockWindow.X);
        Assert.Equal(50, restored.DockWindow.Y);
        Assert.Equal(DockWindowState.Maximized, restored.DockWindow.WindowState);
    }

    [AvaloniaFact]
    public void Missing_And_Canceled_Floating_Content_Is_Cleaned_Up_And_Tools_Can_Be_Restored()
    {
        const string xml = """
            <LayoutRoot>
              <RootPanel><LayoutDocumentPane /></RootPanel>
              <FloatingWindows>
                <LayoutDocumentFloatingWindow />
                <LayoutDocumentFloatingWindow><LayoutDocument ContentId="cancel" /></LayoutDocumentFloatingWindow>
                <LayoutDocumentFloatingWindow><LayoutDocument ContentId="missing" /></LayoutDocumentFloatingWindow>
                <LayoutAnchorableFloatingWindow>
                  <LayoutAnchorablePaneGroup FloatingWidth="500" FloatingHeight="350">
                    <LayoutAnchorablePane><LayoutAnchorable ContentId="tool" /></LayoutAnchorablePane>
                  </LayoutAnchorablePaneGroup>
                </LayoutAnchorableFloatingWindow>
              </FloatingWindows>
            </LayoutRoot>
            """;
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.LayoutSerializationCallback += (_, args) => args.Cancel = args.Model.ContentId == "cancel";
        serializer.Deserialize(new StringReader(xml));
        LayoutAnchorable hidden = Assert.Single(manager.Layout.Hidden);
        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(manager.Layout.FloatingWindows));
        Assert.True(hidden.IsHidden);
        Assert.Same(manager.Layout, hidden.Root);
        Assert.False(window.IsVisible);
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutDocument>());
        hidden.Content = new object();
        hidden.Show();
        Assert.True(window.IsVisible);
        Assert.True(hidden.IsFloating);
        Assert.Empty(manager.Layout.Hidden);
        Assert.Same(hidden, Assert.Single(window.Descendents().OfType<LayoutAnchorable>()));
    }

    [AvaloniaTheory]
    [InlineData("<LayoutDocumentFloatingWindow><LayoutDocument /><LayoutDocument /></LayoutDocumentFloatingWindow>")]
    [InlineData("<LayoutAnchorableFloatingWindow><LayoutPanel /></LayoutAnchorableFloatingWindow>")]
    [InlineData("<LayoutDocumentFloatingWindow><LayoutAnchorable /></LayoutDocumentFloatingWindow>")]
    [InlineData("<LayoutDocument />")]
    public void Invalid_Floating_Xml_Does_Not_Replace_The_Layout(string floatingXml)
    {
        DockingManager manager = new();
        LayoutRoot original = manager.Layout;
        string xml = "<LayoutRoot><RootPanel /><FloatingWindows>" + floatingXml + "</FloatingWindows></LayoutRoot>";
        Assert.Throws<XmlException>(() => new XmlLayoutSerializer(manager).Deserialize(new StringReader(xml)));
        Assert.Same(original, manager.Layout);
    }

    [AvaloniaFact]
    public void Live_Floating_Xml_Restore_Closes_Old_Host_And_Preserves_Editor_Content()
    {
        FloatingFixture view = new();
        DockingManager manager = view.FindControl<DockingManager>("Manager")!;
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            LayoutDocumentFloatingWindow original = (LayoutDocumentFloatingWindow)manager.Layout.FloatingWindows[0];
            HostWindow oldHost = Assert.IsType<HostWindow>(original.DockWindow.Host);
            TextBox editor = Assert.IsType<TextBox>(original.RootDocument!.Content);
            editor.Text = "Unsaved floating edits";
            original.RootDocument.FloatingWidth = 550;
            XmlLayoutSerializer serializer = new(manager);
            using StringWriter writer = new();
            serializer.Serialize(writer);
            serializer.Deserialize(new StringReader(writer.ToString()));
            Dispatcher.UIThread.RunJobs();
            LayoutDocumentFloatingWindow restored = (LayoutDocumentFloatingWindow)Assert.Single(manager.Layout.FloatingWindows);
            HostWindow newHost = Assert.IsType<HostWindow>(restored.DockWindow.Host);
            Assert.False(oldHost.IsVisible);
            Assert.NotSame(oldHost, newHost);
            Assert.True(newHost.IsVisible);
            Assert.Equal(550, newHost.Width);
            Assert.Same(editor, restored.RootDocument!.Content);
            Assert.Equal("Unsaved floating edits", editor.Text);
            Assert.Contains(editor, newHost.GetVisualDescendants().OfType<TextBox>());
            Assert.True(editor.Bounds.Width > 0);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }

    [AvaloniaFact]
    public void Native_Hiding_Of_A_Floating_Tool_Uses_Main_Hidden_Collection_And_Round_Trips()
    {
        LayoutAnchorable tool = new() { ContentId = "tool", Content = new object() };
        LayoutAnchorableFloatingWindow window = new() { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(tool)) };
        DockingManager manager = new();
        manager.Layout.FloatingWindows.Add(window);
        manager.DockFactory.HideDockable(tool.DockModel);
        Assert.Same(tool, Assert.Single(manager.Layout.Hidden));
        Assert.True(tool.IsHidden);
        Assert.False(window.IsVisible);
        XmlLayoutSerializer serializer = new(manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        LayoutAnchorable restored = Assert.Single(manager.Layout.Hidden);
        restored.Show();
        Assert.True(restored.IsFloating);
        Assert.Same(restored, Assert.Single(manager.Layout.FloatingWindows[0].Descendents().OfType<LayoutAnchorable>()));
    }

    [AvaloniaFact]
    public void Hiding_The_Last_Floating_Tool_Closes_The_Host_And_Show_Reopens_It()
    {
        FloatingFixture view = new();
        DockingManager manager = view.FindControl<DockingManager>("Manager")!;
        LayoutAnchorable tool = new() { ContentId = "tool", Content = new TextBlock { Text = "Floating tool" } };
        LayoutAnchorableFloatingWindow window = new() { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(tool)) };
        manager.Layout.FloatingWindows.Add(window);
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            HostWindow oldHost = Assert.IsType<HostWindow>(window.DockWindow.Host);
            tool.Hide();
            Dispatcher.UIThread.RunJobs();
            Assert.False(oldHost.IsVisible);
            Assert.Null(window.DockWindow.Host);
            Assert.True(tool.IsHidden);
            Assert.Same(tool, Assert.Single(manager.Layout.Hidden));
            tool.Show();
            Dispatcher.UIThread.RunJobs();
            HostWindow newHost = Assert.IsType<HostWindow>(window.DockWindow.Host);
            Assert.True(newHost.IsVisible);
            Assert.NotSame(oldHost, newHost);
            Assert.True(tool.IsFloating);
            Assert.Contains(newHost.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Floating tool" && text.Bounds.Width > 0);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }
}
