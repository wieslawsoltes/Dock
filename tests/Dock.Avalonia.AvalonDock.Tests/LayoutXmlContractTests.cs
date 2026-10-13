using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutXmlContractTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Content_Xml_Restores_Metadata_Without_Replacing_The_Native_Model_Or_Application_Content(bool tool)
    {
        LayoutContent source = tool
            ? new LayoutAnchorable { CanHide = false, CanAutoHide = false, AutoHideWidth = 220 }
            : new LayoutDocument { Description = "Description", CanMove = false };
        source.Title = "Title & details";
        source.ContentId = "content";
        source.CanClose = false;
        source.CanFloat = false;
        source.FloatingWidth = 500;
        source.FloatingHeight = 350;
        source.FloatingLeft = 40;
        source.FloatingTop = 60;
        source.IsMaximized = true;
        source.ToolTip = "Tooltip";
        source.IsSelected = true;
        LayoutContent target = tool ? new LayoutAnchorable() : new LayoutDocument();
        object applicationContent = new();
        target.Content = applicationContent;
        object native = target.DockModel;

        ReadFragment((IXmlSerializable)target, WriteFragment((IXmlSerializable)source));

        Assert.Same(native, target.DockModel);
        Assert.Same(applicationContent, target.Content);
        Assert.Equal(source.Title, target.Title);
        Assert.Equal(source.ContentId, target.ContentId);
        Assert.Equal(source.ToolTip, target.ToolTip);
        Assert.False(target.CanClose);
        Assert.False(target.CanFloat);
        Assert.True(target.IsSelected);
        Assert.True(target.IsMaximized);
        Assert.Equal(500, target.FloatingWidth);
        Assert.Equal(350, target.FloatingHeight);
        Assert.Equal(40, target.FloatingLeft);
        Assert.Equal(60, target.FloatingTop);
        if (target is LayoutAnchorable anchorable)
        {
            Assert.False(anchorable.CanHide);
            Assert.False(anchorable.CanAutoHide);
            Assert.Equal(220, anchorable.AutoHideWidth);
        }
        else
        {
            Assert.Equal("Description", ((LayoutDocument)target).Description);
            Assert.False(((LayoutDocument)target).CanMove);
        }
    }

    [AvaloniaFact]
    public void Reading_Partial_Content_Metadata_Preserves_Unspecified_Values_And_Clears_Explicit_Selection()
    {
        LayoutDocument document = new()
        {
            Title = "Existing", ContentId = "stable", Description = "Keep", ToolTip = "Keep tooltip",
            CanFloat = false, IsMaximized = true, IsSelected = true
        };
        using XmlReader reader = XmlReader.Create(new StringReader("<Document IsSelected='false' CanClose='false' />"));
        document.ReadXml(reader);
        Assert.Equal("Existing", document.Title);
        Assert.Equal("stable", document.ContentId);
        Assert.Equal("Keep", document.Description);
        Assert.Equal("Keep tooltip", document.ToolTip);
        Assert.False(document.CanClose);
        Assert.False(document.CanFloat);
        Assert.False(document.IsSelected);
        Assert.True(document.IsMaximized);
        Assert.True(reader.EOF);
    }

    [AvaloniaTheory]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    [InlineData("panel")]
    [InlineData("anchor-group")]
    [InlineData("anchor-side")]
    public void Group_Xml_Appends_Children_To_The_Existing_Native_Container(string kind)
    {
        LayoutElement source = CreateGroup(kind, "new");
        LayoutElement target = CreateGroup(kind, "existing");
        object native = target.DockModel;
        if (source is ILayoutOrientableGroup orientable) orientable.Orientation = Orientation.Vertical;
        if (source is LayoutAnchorablePane toolPane) toolPane.Name = "Properties";
        if (source is LayoutDocumentPane documentPane) documentPane.ShowHeader = false;

        ReadFragment((IXmlSerializable)target, WriteFragment((IXmlSerializable)source));

        Assert.Same(native, target.DockModel);
        Assert.Equal(new[] { "existing", "new" }, target.Descendents().OfType<LayoutContent>().Select(x => x.ContentId));
        Assert.Equal(2, ((ILayoutContainer)target).ChildrenCount);
        foreach (ILayoutElement child in ((ILayoutContainer)target).Children) Assert.Same(target, child.Parent);
        if (target is ILayoutOrientableGroup direction) Assert.Equal(Orientation.Vertical, direction.Orientation);
        if (target is LayoutAnchorablePane tools) Assert.Equal("Properties", tools.Name);
        if (target is LayoutDocumentPane documents) Assert.False(documents.ShowHeader);
    }

    [AvaloniaFact]
    public void Positionable_Group_Xml_Preserves_Dimensions_And_Options()
    {
        LayoutPanel source = new(new LayoutDocumentPane())
        {
            DockWidth = new GridLength(245), DockHeight = new GridLength(3, GridUnitType.Star),
            DockMinWidth = 150, FloatingWidth = 600, FloatingHeight = 400,
            AllowDuplicateContent = false, CanRepositionItems = false
        };
        LayoutPanel target = new();
        ReadFragment(target, WriteFragment(source));
        Assert.Equal(source.DockWidth, target.DockWidth);
        Assert.Equal(source.DockHeight, target.DockHeight);
        Assert.Equal(source.DockMinWidth, target.DockMinWidth);
        Assert.Equal(source.FloatingWidth, target.FloatingWidth);
        Assert.Equal(source.FloatingHeight, target.FloatingHeight);
        Assert.False(target.AllowDuplicateContent);
        Assert.False(target.CanRepositionItems);
    }

    [AvaloniaFact]
    public void Empty_Elements_Are_Consumed_Without_Clearing_Existing_Content()
    {
        LayoutDocument document = new() { ContentId = "existing" };
        LayoutDocumentPane pane = new(document);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        foreach (IXmlSerializable target in new IXmlSerializable[] { document, pane, root })
        {
            using XmlReader reader = XmlReader.Create(new StringReader("<Empty />"));
            target.ReadXml(reader);
            Assert.True(reader.EOF);
        }
        Assert.Equal("existing", document.ContentId);
        Assert.Same(document, Assert.Single(pane.Children));
        Assert.Same(pane, Assert.Single(root.RootPanel.Children));
    }

    [AvaloniaFact]
    public void Standalone_Pane_Xml_Restores_The_Selected_Tab()
    {
        LayoutDocumentPane pane = new(new LayoutDocument { ContentId = "first" });
        pane.Children.Add(new LayoutDocument { ContentId = "second", IsSelected = true });
        LayoutDocumentPane restored = new();
        ReadFragment(restored, WriteFragment(pane));
        Assert.Equal(1, restored.SelectedContentIndex);
        Assert.Equal("second", restored.SelectedContent!.ContentId);
        Assert.False(restored.Children[0].IsSelected);
        Assert.True(restored.Children[1].IsSelected);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(restored) };
        Assert.Equal(1, restored.SelectedContentIndex);
        Assert.Same(root, restored.SelectedContent.Root);
    }

    [AvaloniaFact]
    public void Root_Xml_Replaces_The_Hierarchy_While_Keeping_The_Manager_Native_Root_And_Factory()
    {
        LayoutDocument document = new() { ContentId = "document" };
        LayoutRoot source = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
        LayoutAnchorGroup pinned = new();
        pinned.Children.Add(new LayoutAnchorable { ContentId = "pinned" });
        source.LeftSide!.Children.Add(pinned);
        source.Hidden.Add(new LayoutAnchorable { ContentId = "hidden" });
        source.FloatingWindows.Add(new LayoutDocumentFloatingWindow
        {
            RootDocument = new LayoutDocument { ContentId = "floating", FloatingWidth = 500, FloatingHeight = 350 }
        });
        source.ActiveContent = document;
        DockingManager manager = new();
        LayoutRoot target = manager.Layout;
        LayoutPanel previous = target.RootPanel;
        object native = target.DockModel;
        object factory = target.DockFactory;

        ReadFragment(target, WriteFragment(source));

        Assert.Same(target, manager.Layout);
        Assert.Same(native, target.DockModel);
        Assert.Same(factory, target.DockFactory);
        Assert.Null(previous.Parent);
        // The persisted last-focused flag is metadata; upstream ReadXml does
        // not translate it into root activation or a new activation timestamp.
        Assert.Null(target.ActiveContent);
        Assert.Null(target.LastFocusedDocument);
        Assert.True(target.Descendents().OfType<LayoutDocument>().Single(item => item.ContentId == "document").IsLastFocusedDocument);
        Assert.Equal("pinned", Assert.Single(Assert.Single(target.LeftSide!.Children).Children).ContentId);
        Assert.Equal("hidden", Assert.Single(target.Hidden).ContentId);
        Assert.Equal(500, Assert.Single(target.FloatingWindows).DockWindow.Width);
        Assert.All(target.Descendents().OfType<LayoutContent>(), item =>
        {
            Assert.Same(target, item.Root);
            Assert.Same(factory, item.DockModel.Factory);
        });
    }

    [AvaloniaFact]
    public void A_Later_Root_Xml_Error_Retains_The_Completed_Panel_Replacement()
    {
        LayoutRoot root = new();
        LayoutPanel previous = root.RootPanel;
        using XmlReader reader = XmlReader.Create(new StringReader("<Layout><RootPanel /><Unknown /></Layout>"));
        Assert.Throws<ArgumentException>(() => root.ReadXml(reader));
        Assert.NotSame(previous, root.RootPanel);
        Assert.Null(previous.Parent);
    }

    [AvaloniaFact]
    public void Reading_An_Empty_Document_Layout_Clears_Previous_Focus()
    {
        LayoutDocument document = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
        root.ActiveContent = document;
        Assert.Same(document, root.LastFocusedDocument);
        using XmlReader reader = XmlReader.Create(new StringReader("<Layout><RootPanel><LayoutDocumentPane /></RootPanel></Layout>"));
        root.ReadXml(reader);
        Assert.Null(root.ActiveContent);
        Assert.Null(root.LastFocusedDocument);
        Assert.Null(document.Root);
    }

    private static LayoutElement CreateGroup(string kind, string id) => kind switch
    {
        "document-pane" => new LayoutDocumentPane(new LayoutDocument { ContentId = id }),
        "tool-pane" => new LayoutAnchorablePane(new LayoutAnchorable { ContentId = id }),
        "document-group" => new LayoutDocumentPaneGroup(new LayoutDocumentPane(new LayoutDocument { ContentId = id })),
        "tool-group" => new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable { ContentId = id })),
        "panel" => new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { ContentId = id })),
        "anchor-group" => new LayoutAnchorGroup { Children = { new LayoutAnchorable { ContentId = id } } },
        "anchor-side" => new LayoutAnchorSide { Children = { new LayoutAnchorGroup { Children = { new LayoutAnchorable { ContentId = id } } } } },
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static string WriteFragment(IXmlSerializable source)
    {
        Assert.Null(source.GetSchema());
        using StringWriter text = new();
        using (XmlWriter writer = XmlWriter.Create(text, new XmlWriterSettings { OmitXmlDeclaration = true }))
        {
            writer.WriteStartElement("Container");
            writer.WriteStartElement("Layout");
            source.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteElementString("After", "Still readable");
            writer.WriteEndElement();
        }
        return text.ToString();
    }

    private static void ReadFragment(IXmlSerializable target, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        reader.ReadStartElement("Container");
        target.ReadXml(reader);
        Assert.Equal("Still readable", reader.ReadElementContentAsString("After", ""));
        Assert.Null(target.GetSchema());
    }
}
