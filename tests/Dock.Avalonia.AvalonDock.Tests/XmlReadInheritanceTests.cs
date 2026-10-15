using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class XmlReadInheritanceTests
{
    [AvaloniaTheory]
    [InlineData(false, "Horizontal")]
    [InlineData(true, "Horizontal")]
    [InlineData(false, "invalid")]
    [InlineData(true, "invalid")]
    [InlineData(false, "2147483648")]
    [InlineData(true, "2147483648")]
    public void An_Additional_Orientation_Interface_Does_Not_Change_The_Inherited_Reader(bool positionable, string orientation)
    {
        LayoutElement group = positionable ? new ApplicationGroup() : new ApplicationSide();
        string child = positionable ? "<LayoutDocumentPane />" : "<LayoutAnchorGroup />";
        Read((IXmlSerializable)group, "<Group Orientation='" + orientation + "' DockMinWidth='81'>" + child + "</Group>");
        Assert.Equal(Orientation.Vertical, ((ILayoutOrientableGroup)group).Orientation);
        Assert.Equal(1, ((ILayoutContainer)group).ChildrenCount);
        if (group is ApplicationGroup sized) { Assert.Equal(81, sized.DockMinWidth); Assert.Equal(0, sized.OrientationSets); }
        else { Assert.True(double.IsNaN(group.DockModel.MinWidth)); Assert.Equal(0, ((ApplicationSide)group).OrientationSets); }
    }

    [AvaloniaFact]
    public void Registered_Positionable_Readers_Keep_Custom_Interface_State_And_Read_Their_Base_Metadata()
    {
        XmlLayoutSerializer serializer = new(new DockingManager());
        ApplicationGroup? group = null;
        serializer.KnownTypes.Register("ApplicationGroup", () => group = new ApplicationGroup());
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><ApplicationGroup Orientation='invalid' DockMinWidth='82'><LayoutDocumentPane><LayoutDocument ContentId='item' /></LayoutDocumentPane></ApplicationGroup></RootPanel></LayoutRoot>"));
        Assert.NotNull(group);
        Assert.Equal(Orientation.Vertical, group.Orientation);
        Assert.Equal(0, group.OrientationSets);
        Assert.Equal(82, group.DockMinWidth);
        Assert.Equal(1, group.Before);
        Assert.Equal(1, group.After);
        Assert.Equal(2, group.Depth);
    }

    [AvaloniaFact]
    public void A_Failing_Base_Attribute_Is_Reported_Without_Reading_Additional_Interface_Metadata()
    {
        ApplicationGroup group = new();
        using XmlReader reader = XmlReader.Create(new StringReader("<Group Orientation='invalid' DockWidth='125' DockMinWidth='invalid' />"));
        Assert.Throws<FormatException>(() => group.ReadXml(reader));
        Assert.Equal(0, group.OrientationSets);
        Assert.Equal(125, group.DockWidth.Value);
        Assert.Equal("DockMinWidth", reader.LocalName);
        Assert.Equal(XmlNodeType.Attribute, reader.NodeType);
        Assert.Equal(1, group.Before);
        Assert.Equal(0, group.After);
    }

    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    public void Concrete_Orientable_Readers_Apply_Orientation_Before_Positionable_Metadata(string kind)
    {
        LayoutElement group = kind switch { "panel" => new LayoutPanel(), "document-group" => new LayoutDocumentPaneGroup(), _ => new LayoutAnchorablePaneGroup() };
        double minimumDuringOrientation = -1;
        group.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "Orientation") minimumDuringOrientation = group.DockModel.MinWidth;
        };
        Read((IXmlSerializable)group, "<Group Orientation='Vertical' DockMinWidth='83' />");
        Assert.Equal(25, minimumDuringOrientation);
        Assert.Equal(83, group.DockModel.MinWidth);
        Assert.Equal(Orientation.Vertical, ((ILayoutOrientableGroup)group).Orientation);
    }

    [AvaloniaFact]
    public void A_Derived_Pane_Can_Suppress_Metadata_And_Children_Without_Changing_Selection()
    {
        SuppressedPane pane = new();
        LayoutAnchorable tool = new();
        pane.Children.Add(tool);
        Read(pane, "<Pane Name='ignored' DockMinWidth='invalid'><Unknown /></Pane>");
        Assert.Null(pane.Name);
        Assert.Equal(25, pane.DockMinWidth);
        Assert.Same(tool, pane.SelectedContent);
        Assert.Same(tool, Assert.Single(pane.Children));
        Assert.Equal(1, pane.Reads);
    }

    private static void Read(IXmlSerializable group, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        group.ReadXml(reader);
        Assert.True(reader.EOF);
    }

    private sealed class ApplicationSide : LayoutAnchorSide, ILayoutOrientableGroup
    {
        private Orientation _orientation = Orientation.Vertical;
        public int OrientationSets { get; private set; }
        public Orientation Orientation { get => _orientation; set { OrientationSets++; _orientation = value; } }
    }

    private sealed class ApplicationGroup : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutOrientableGroup, ILayoutPanelElement
    {
        private Orientation _orientation = Orientation.Vertical;
        public int OrientationSets { get; private set; }
        public int Before { get; private set; }
        public int After { get; private set; }
        public int Depth { get; private set; }
        public Orientation Orientation { get => _orientation; set { OrientationSets++; _orientation = value; } }
        public override void ReadXml(XmlReader reader)
        {
            Before++;
            Depth = reader.Depth;
            base.ReadXml(reader);
            After++;
        }
    }

    private sealed class SuppressedPane : LayoutAnchorablePane
    {
        public int Reads { get; private set; }
        public override void ReadXml(XmlReader reader) { Reads++; reader.Skip(); }
    }
}
