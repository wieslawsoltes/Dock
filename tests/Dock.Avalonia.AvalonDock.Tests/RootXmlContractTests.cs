using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class RootXmlContractTests
{
    [AvaloniaTheory]
    [InlineData(null, Orientation.Horizontal)]
    [InlineData("Vertical", Orientation.Vertical)]
    [InlineData("vertical", Orientation.Horizontal)]
    [InlineData("VERTICAL", Orientation.Horizontal)]
    [InlineData("Horizontal", Orientation.Horizontal)]
    [InlineData("1", Orientation.Horizontal)]
    [InlineData("2147483648", Orientation.Horizontal)]
    [InlineData("Sideways", Orientation.Horizontal)]
    [InlineData(" Vertical ", Orientation.Horizontal)]
    [InlineData("", Orientation.Horizontal)]
    public void Root_Orientation_Uses_The_Exact_Vertical_Spelling(string? value, Orientation expected)
    {
        LayoutRoot root = new();
        string attribute = value is null ? "" : " Orientation='" + value + "'";
        Read(root, Canonical("<RootPanel" + attribute + "><LayoutDocumentPane /></RootPanel>"));
        Assert.Equal(expected, root.RootPanel.Orientation);
        Assert.IsType<LayoutDocumentPane>(Assert.Single(root.RootPanel.Children));
    }

    [AvaloniaTheory]
    [InlineData("Vertical", Orientation.Vertical)]
    [InlineData("vertical", Orientation.Horizontal)]
    [InlineData("1", Orientation.Horizontal)]
    [InlineData("Sideways", Orientation.Horizontal)]
    public void Complete_Restoration_Uses_The_Root_Section_Contract(string value, Orientation expected)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        serializer.Deserialize(new StringReader(Canonical("<RootPanel Orientation='" + value + "'><LayoutDocumentPane><LayoutDocument ContentId='retained' /></LayoutDocumentPane></RootPanel>")));
        Assert.Equal(expected, manager.Layout.RootPanel.Orientation);
        Assert.Equal("retained", Assert.Single(Assert.IsType<LayoutDocumentPane>(Assert.Single(manager.Layout.RootPanel.Children)).Children).ContentId);
    }

    [AvaloniaTheory]
    [InlineData("<LayoutDocument ContentId='ignored' />")]
    [InlineData("<LayoutAnchorable ContentId='ignored' />")]
    [InlineData("<LayoutAnchorGroup><LayoutAnchorable /></LayoutAnchorGroup>")]
    [InlineData("<LayoutAnchorSide />")]
    [InlineData("<LayoutDocumentFloatingWindow><LayoutDocument /></LayoutDocumentFloatingWindow>")]
    public void A_Root_Panel_Consumes_Recognized_Children_That_Are_Not_Panel_Elements(string ignored)
    {
        LayoutRoot root = new();
        Read(root, Canonical("<RootPanel>" + ignored + "<LayoutDocumentPane /></RootPanel>"));
        Assert.IsType<LayoutDocumentPane>(Assert.Single(root.RootPanel.Children));
        Assert.Empty(root.FloatingWindows);
        Assert.Empty(root.Hidden);
    }

    [AvaloniaTheory]
    [InlineData("TopSide")]
    [InlineData("RightSide")]
    [InlineData("LeftSide")]
    [InlineData("BottomSide")]
    public void Root_Sides_Consume_Recognized_Non_Group_Children_Before_Attaching_Their_Groups(string name)
    {
        LayoutRoot root = new();
        int countDuringReplacement = -1;
        root.PropertyChanged += (_, args) => { if (args.PropertyName == name) countDuringReplacement = Side(root, name).ChildrenCount; };
        Read(root, Canonical("<RootPanel />", name, "<LayoutDocument ContentId='ignored' /><LayoutAnchorGroup><LayoutAnchorable ContentId='retained' /></LayoutAnchorGroup>"));
        Assert.Equal(0, countDuringReplacement);
        LayoutAnchorGroup group = Assert.Single(Side(root, name).Children);
        Assert.Equal("retained", Assert.Single(group.Children).ContentId);
        Assert.Same(Side(root, name), group.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Ignored_Registered_Children_Run_Their_Read_Hooks_But_Do_Not_Enter_Content_Recovery(bool side)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        IgnoredDocument? ignored = null;
        serializer.KnownTypes.Register("IgnoredDocument", () => ignored = new IgnoredDocument());
        List<string?> recovered = new();
        serializer.LayoutSerializationCallback += (_, args) => { recovered.Add(args.Model.ContentId); args.Content = new object(); };
        const string discarded = "<IgnoredDocument ContentId='discarded' />";
        const string kept = "<LayoutDocumentPane><LayoutDocument ContentId='retained' /></LayoutDocumentPane>";
        serializer.Deserialize(new StringReader(Canonical("<RootPanel>" + (side ? "" : discarded) + kept + "</RootPanel>", "TopSide", side ? discarded : "")));
        Assert.NotNull(ignored);
        Assert.Equal(1, ignored.Reads);
        Assert.Equal(2, ignored.Depth);
        Assert.Equal("discarded", ignored.ContentId);
        Assert.Null(ignored.Parent);
        Assert.Null(ignored.Root);
        Assert.Equal(new[] { "retained" }, recovered);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Later_Unknown_Child_Retains_The_Root_Section_Staging_Boundary(bool side)
    {
        LayoutRoot root = new();
        LayoutPanel previousPanel = root.RootPanel;
        LayoutAnchorSide previousSide = root.TopSide;
        string children = "<LayoutDocument />" + (side ? "<LayoutAnchorGroup />" : "<LayoutDocumentPane />") + "<Unknown />";
        string xml = side ? Canonical("<RootPanel />", "TopSide", children) : Canonical("<RootPanel>" + children + "</RootPanel>");
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        Assert.Throws<ArgumentException>(() => root.ReadXml(reader));
        Assert.Equal("Unknown", reader.LocalName);
        if (side) { Assert.NotSame(previousSide, root.TopSide); Assert.Empty(root.TopSide.Children); }
        else Assert.Same(previousPanel, root.RootPanel);
    }

    [AvaloniaFact]
    public void Native_Root_Metadata_Remains_An_Extension_Alongside_The_Upstream_Orientation_Rule()
    {
        LayoutRoot root = new();
        Read(root, Canonical("<RootPanel Orientation='vertical' Id='native-root' DockMinWidth='83' CanRepositionItems='False' />"));
        Assert.Equal(Orientation.Horizontal, root.RootPanel.Orientation);
        Assert.Equal(83, root.RootPanel.DockMinWidth);
        Assert.Equal("native-root", root.RootPanel.DockModel.Id);
        Assert.False(root.RootPanel.CanRepositionItems);
    }

    [AvaloniaTheory]
    [InlineData("vertical", true)]
    [InlineData("1", true)]
    [InlineData("Sideways", false)]
    public void A_Standalone_Panel_Still_Uses_Its_Enum_Parser_Even_With_The_RootPanel_Element_Name(string value, bool valid)
    {
        LayoutPanel panel = new();
        using XmlReader reader = XmlReader.Create(new StringReader("<RootPanel Orientation='" + value + "' />"));
        reader.MoveToContent();
        if (valid) { panel.ReadXml(reader); Assert.Equal(Orientation.Vertical, panel.Orientation); }
        else Assert.Throws<ArgumentException>(() => panel.ReadXml(reader));
    }

    private static string Canonical(string panel, string? selectedSide = null, string children = "")
    {
        string xml = "<LayoutRoot>" + panel;
        foreach (string side in new[] { "TopSide", "RightSide", "LeftSide", "BottomSide" })
            xml += side == selectedSide && children.Length != 0 ? "<" + side + ">" + children + "</" + side + ">" : "<" + side + " />";
        return xml + "<FloatingWindows /><Hidden /></LayoutRoot>";
    }
    private static LayoutAnchorSide Side(LayoutRoot root, string name) => name switch
    {
        "TopSide" => root.TopSide, "RightSide" => root.RightSide, "LeftSide" => root.LeftSide, _ => root.BottomSide
    };
    private static void Read(LayoutRoot root, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        root.ReadXml(reader);
        Assert.True(reader.EOF);
    }
    private sealed class IgnoredDocument : LayoutDocument
    {
        internal int Reads { get; private set; }
        internal int Depth { get; private set; }
        public override void ReadXml(XmlReader reader) { Reads++; Depth = reader.Depth; base.ReadXml(reader); }
    }
}
