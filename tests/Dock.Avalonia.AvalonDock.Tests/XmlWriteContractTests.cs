using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class XmlWriteContractTests
{
    [AvaloniaTheory]
    [InlineData("document", "CanClose=True")]
    [InlineData("tool", "AutoHideMinWidth=100;AutoHideMinHeight=100;CanClose=False")]
    [InlineData("panel", "Orientation=Horizontal")]
    [InlineData("document-group", "Orientation=Horizontal")]
    [InlineData("tool-group", "Orientation=Horizontal")]
    [InlineData("document-pane", "")]
    [InlineData("tool-pane", "")]
    [InlineData("anchor-group", "")]
    [InlineData("anchor-side", "")]
    public void Default_Models_Write_Only_The_Upstream_Required_Attributes(string kind, string expected)
    {
        LayoutElement element = Create(kind);
        string nativeId = element.DockModel.Id;
        Assert.Equal(expected, string.Join(";", Write(element).Attributes().Select(attribute => attribute.Name + "=" + attribute.Value)));
        Assert.Equal(nativeId, element.DockModel.Id);
    }

    [AvaloniaTheory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, " \t\r\n")]
    [InlineData(true, " \t\r\n")]
    public void Blank_Content_Strings_Are_Omitted_Without_Changing_The_Model(bool tool, string? value)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument { Description = value };
        content.Title = value!;
        content.ContentId = value;
        content.ToolTip = value;
        XElement result = Write(content);
        Assert.Equal(tool ? new[] { "AutoHideMinWidth", "AutoHideMinHeight", "CanClose" } : new[] { "CanClose" }, result.Attributes().Select(attribute => attribute.Name.LocalName));
        Assert.Equal(value, content.Title);
        Assert.Equal(value, content.ContentId);
        Assert.Equal(value, content.ToolTip);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Content_Attributes_Follow_Derived_And_Base_Write_Order(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        Read(content, "<Item CanHide='false' CanAutoHide='false' AutoHideWidth='101' AutoHideHeight='102' AutoHideMinWidth='103' AutoHideMinHeight='104' CanDockAsTabbedDocument='false' Title='title' IsSelected='true' IsLastFocusedDocument='true' ContentId='content' ToolTip='tip' FloatingLeft='11' FloatingTop='12' FloatingWidth='201' FloatingHeight='202' IsFloating='true' IsMaximized='true' CanClose='false' CanFloat='false' LastActivationTimeStamp='2001-02-03T04:05:06' Description='description' CanMove='false' />");
        string common = "Title IsSelected IsLastFocusedDocument ContentId ToolTip FloatingLeft FloatingTop FloatingWidth FloatingHeight IsFloating IsMaximized CanClose CanFloat LastActivationTimeStamp";
        string expected = tool ? "CanHide CanAutoHide AutoHideWidth AutoHideHeight AutoHideMinWidth AutoHideMinHeight CanDockAsTabbedDocument " + common : common + " Description CanMove";
        XElement result = Write(content);
        Assert.Equal(expected.Split(' '), result.Attributes().Select(attribute => attribute.Name.LocalName));
        Assert.Equal("True", result.Attribute("IsSelected")?.Value);
        Assert.Equal("False", result.Attribute("CanFloat")?.Value);
        Assert.Equal("False", result.Attribute("CanClose")?.Value);
    }

    [AvaloniaTheory]
    [InlineData("panel", "Orientation")]
    [InlineData("document-group", "Orientation")]
    [InlineData("tool-group", "Orientation")]
    [InlineData("document-pane", "Id ShowHeader")]
    [InlineData("tool-pane", "Id Name")]
    public void Concrete_Group_Metadata_Precedes_Positionable_Attributes(string kind, string prefix)
    {
        LayoutElement group = Create(kind);
        Read(group, "<Group Id='stored' Name='Tools' ShowHeader='false' Orientation='Vertical' DockWidth='101' DockHeight='2*' DockMinWidth='102' DockMinHeight='103' FloatingWidth='501' FloatingHeight='502' FloatingLeft='21' FloatingTop='22' IsMaximized='true' AllowDuplicateContent='false' CanRepositionItems='false' />");
        string expected = prefix + " DockWidth DockHeight DockMinWidth DockMinHeight FloatingWidth FloatingHeight FloatingLeft FloatingTop IsMaximized";
        if (kind is not ("document-pane" or "tool-pane")) expected += " Id"; // Native group return-location extension.
        expected += " AllowDuplicateContent CanRepositionItems";
        XElement result = Write(group);
        Assert.Equal(expected.Split(' '), result.Attributes().Select(attribute => attribute.Name.LocalName));
        Assert.Equal("True", result.Attribute("IsMaximized")?.Value);
        Assert.Equal("False", result.Attribute("AllowDuplicateContent")?.Value);
    }

    [AvaloniaTheory]
    [InlineData("0", false)]
    [InlineData("-0", false)]
    [InlineData("NaN", false)]
    [InlineData("Infinity", true)]
    [InlineData("125.5", true)]
    public void Auto_Hide_Size_Is_Written_Only_When_Positive(string value, bool written)
    {
        LayoutAnchorable tool = new();
        Read(tool, "<Tool AutoHideWidth='" + value + "' AutoHideHeight='" + value + "' />");
        XElement result = Write(tool);
        Assert.Equal(written, result.Attribute("AutoHideWidth") is not null);
        Assert.Equal(written, result.Attribute("AutoHideHeight") is not null);
        if (written) Assert.Equal(value, result.Attribute("AutoHideWidth")?.Value);
    }

    [AvaloniaFact]
    public void Auto_Hide_Minimum_25_Is_Omitted_Even_Though_New_Tools_Start_At_100()
    {
        LayoutAnchorable tool = new() { AutoHideMinWidth = 25, AutoHideMinHeight = 25 };
        XElement result = Write(tool);
        Assert.Null(result.Attribute("AutoHideMinWidth"));
        Assert.Null(result.Attribute("AutoHideMinHeight"));
        Assert.Equal(100, new LayoutAnchorable().AutoHideMinWidth);
    }

    [AvaloniaTheory]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("anchor-group")]
    public void Explicit_Empty_Identifiers_And_Tool_Pane_Names_Are_Preserved(string kind)
    {
        LayoutElement element = Create(kind);
        Read(element, "<Group Id='' Name='' />");
        XElement result = Write(element);
        Assert.Equal(string.Empty, result.Attribute("Id")?.Value);
        if (element is LayoutAnchorablePane) Assert.Equal(string.Empty, result.Attribute("Name")?.Value);
    }

    [AvaloniaFact]
    public void Saving_A_Layout_Does_Not_Assign_Unreferenced_Container_Identifiers()
    {
        LayoutPanel panel = new(new LayoutDocumentPane());
        panel.Children.Add(new LayoutAnchorablePane());
        DockingManager manager = new() { Layout = new() { RootPanel = panel } };
        using StringWriter text = new();
        new XmlLayoutSerializer(manager).Serialize(text);
        Assert.Empty(XElement.Parse(text.ToString()).Descendants().Attributes("Id"));
        Assert.All(manager.Layout.Descendents().OfType<LayoutElement>(), element => Assert.True(string.IsNullOrEmpty(element.DockModel.Id)));
    }

    [AvaloniaFact]
    public void A_Referenced_Native_Group_Gets_An_Id_Without_Changing_Unrelated_Groups()
    {
        LayoutDocument document = new() { ContentId = "document" };
        LayoutPanel previous = new();
        LayoutPanel unrelated = new();
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        manager.Layout.RootPanel.Children.Add(previous);
        manager.Layout.RootPanel.Children.Add(unrelated);
        document.PreviousContainer = previous;
        using StringWriter text = new();
        new XmlLayoutSerializer(manager).Serialize(text);
        XElement saved = XElement.Parse(text.ToString());
        Assert.False(string.IsNullOrEmpty(previous.DockModel.Id));
        Assert.True(string.IsNullOrEmpty(unrelated.DockModel.Id));
        Assert.Equal(previous.DockModel.Id, saved.Descendants("LayoutDocument").Single().Attribute("PreviousContainerId")?.Value);
        Assert.Single(saved.Descendants().Attributes("Id"), attribute => attribute.Value == previous.DockModel.Id);
    }

    [AvaloniaTheory]
    [InlineData(false, "root")]
    [InlineData(true, "root")]
    [InlineData(false, "floating")]
    [InlineData(true, "floating")]
    [InlineData(false, "side")]
    [InlineData(true, "side")]
    public void Nonserializable_Return_Containers_Are_Omitted_Without_Assigning_Identifiers(bool tool, string kind)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutElement target = kind switch { "root" => new LayoutRoot(), "floating" => new LayoutDocumentFloatingWindow(), _ => new LayoutAnchorSide() };
        string id = target.DockModel.Id;
        content.PreviousContainer = (ILayoutContainer)target;
        XElement result = Write(content);
        Assert.Null(result.Attribute("PreviousContainerId"));
        Assert.Null(result.Attribute("PreviousContainerIndex"));
        Assert.Same(target, content.PreviousContainer);
        Assert.Equal(id, target.DockModel.Id);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Inherited_Group_Writers_Do_Not_Infer_Concrete_Metadata_From_Extra_Interfaces(bool positionable)
    {
        LayoutElement group = positionable ? new OrientablePositionableGroup() : new OrientableGroup();
        Assert.Empty(Write(group).Attributes());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Return_Index_Is_Read_After_The_Writer_Receives_The_Container_Id(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        content.PreviousContainer = new LayoutDocumentPane();
        content.PreviousContainerIndex = 2;
        using StringWriter text = new();
        using (ObservingWriter writer = new(text, name =>
        {
            if (name == "PreviousContainerId") content.PreviousContainerIndex = 7;
        }))
        {
            writer.WriteStartElement("Content");
            content.WriteXml(writer);
            writer.WriteEndElement();
        }
        Assert.Equal("7", XElement.Parse(text.ToString()).Attribute("PreviousContainerIndex")?.Value);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Writer_Failure_Stops_At_The_Correct_Inheritance_Boundary(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable { CanHide = false } : new LayoutDocument { Description = "later" };
        content.Title = "title";
        content.CanFloat = false;
        ApplicationException failure = new("writer failure");
        List<string> attributes = new();
        using StringWriter text = new();
        using ObservingWriter writer = new(text, name =>
        {
            attributes.Add(name);
            if (name == "CanClose") throw failure;
        });
        writer.WriteStartElement("Content");
        Assert.Same(failure, Assert.Throws<ApplicationException>(() => content.WriteXml(writer)));
        Assert.Equal(tool ? new[] { "CanHide", "AutoHideMinWidth", "AutoHideMinHeight", "Title", "CanClose" } : new[] { "Title", "CanClose" }, attributes);
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData(" \t ")]
    public void Saved_Blank_Content_Identifiers_Are_Not_Automatically_Recovered(string id)
    {
        LayoutDocument document = new() { ContentId = id, Content = new object() };
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        XmlLayoutSerializer serializer = new(manager);
        using StringWriter saved = new();
        serializer.Serialize(saved);
        Assert.Null(XElement.Parse(saved.ToString()).Descendants("LayoutDocument").Single().Attribute("ContentId"));
        serializer.Deserialize(new StringReader(saved.ToString()));
        Assert.Empty(manager.Layout.Descendents().OfType<LayoutDocument>());
    }

    private static LayoutElement Create(string kind) => kind switch
    {
        "document" => new LayoutDocument(), "tool" => new LayoutAnchorable(), "panel" => new LayoutPanel(),
        "document-group" => new LayoutDocumentPaneGroup(), "tool-group" => new LayoutAnchorablePaneGroup(),
        "document-pane" => new LayoutDocumentPane(), "tool-pane" => new LayoutAnchorablePane(),
        "anchor-group" => new LayoutAnchorGroup(), _ => new LayoutAnchorSide()
    };

    private static XElement Write(LayoutElement element)
    {
        XDocument document = new();
        using (XmlWriter writer = document.CreateWriter())
        {
            writer.WriteStartElement("Item");
            ((IXmlSerializable)element).WriteXml(writer);
            writer.WriteEndElement();
        }
        return document.Root!;
    }

    private static void Read(LayoutElement element, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        ((IXmlSerializable)element).ReadXml(reader);
    }

    private sealed class OrientableGroup : LayoutAnchorSide, ILayoutOrientableGroup
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
    }
    private sealed class OrientablePositionableGroup : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutOrientableGroup
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
    }
    private sealed class ObservingWriter(TextWriter text, Action<string> afterAttribute) : XmlTextWriter(text)
    {
        private string _attribute = string.Empty;
        public override void WriteStartAttribute(string? prefix, string localName, string? ns)
        {
            _attribute = localName;
            base.WriteStartAttribute(prefix, localName, ns);
        }
        public override void WriteEndAttribute()
        {
            base.WriteEndAttribute();
            afterAttribute(_attribute);
        }
    }
}
