using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class XmlNumericContractTests
{
    public static IEnumerable<object[]> FloatingNumbers()
    {
        foreach (string kind in new[] { "document", "tool", "panel", "document-pane", "tool-pane", "document-group", "tool-group" })
            foreach (string value in new[] { "Infinity", "-Infinity", "NaN", "-1234.5" })
                yield return new object[] { kind, value };
    }

    [AvaloniaTheory]
    [MemberData(nameof(FloatingNumbers))]
    public void Floating_Numbers_Use_The_Upstream_Invariant_Lexical_Form_And_Round_Trip(string kind, string value)
    {
        LayoutElement source = Create(kind);
        Read(source, $"<Item FloatingWidth='{value}' FloatingHeight='{value}' FloatingLeft='{value}' FloatingTop='{value}' />");
        XElement written = Write(source);
        foreach (string attribute in new[] { "FloatingWidth", "FloatingHeight", "FloatingLeft", "FloatingTop" })
            Assert.Equal(value, written.Attribute(attribute)?.Value);
        LayoutElement restored = Create(kind);
        Read(restored, written.ToString());
        XElement roundTrip = Write(restored);
        foreach (string attribute in new[] { "FloatingWidth", "FloatingHeight", "FloatingLeft", "FloatingTop" })
            Assert.Equal(written.Attribute(attribute)?.Value, roundTrip.Attribute(attribute)?.Value);
    }

    public static IEnumerable<object[]> GroupNumbers()
    {
        foreach (string kind in new[] { "panel", "document-pane", "tool-pane", "document-group", "tool-group" })
            foreach (string value in new[] { "Infinity", "-Infinity", "NaN", "-1234.5" })
                yield return new object[] { kind, value };
    }

    [AvaloniaTheory]
    [MemberData(nameof(GroupNumbers))]
    public void Stored_Group_Minima_Round_Trip_Without_Changing_Their_Native_Values(string kind, string value)
    {
        LayoutElement source = Create(kind);
        Read(source, $"<Group DockMinWidth='{value}' DockMinHeight='{value}' />");
        XElement written = Write(source);
        Assert.Equal(value, written.Attribute("DockMinWidth")?.Value);
        Assert.Equal(value, written.Attribute("DockMinHeight")?.Value);
        LayoutElement restored = Create(kind);
        Read(restored, written.ToString());
        Assert.Equal(source.DockModel.MinWidth, restored.DockModel.MinWidth);
        Assert.Equal(source.DockModel.MinHeight, restored.DockModel.MinHeight);
    }

    [AvaloniaTheory]
    [InlineData("Infinity")]
    [InlineData("NaN")]
    public void Auto_Hide_Infinite_Dimensions_And_Nonfinite_Minima_Round_Trip(string minimum)
    {
        LayoutAnchorable tool = new();
        Read(tool, $"<Tool AutoHideWidth='Infinity' AutoHideHeight='Infinity' AutoHideMinWidth='{minimum}' AutoHideMinHeight='{minimum}' />");
        XElement written = Write(tool);
        Assert.Equal("Infinity", written.Attribute("AutoHideWidth")?.Value);
        Assert.Equal("Infinity", written.Attribute("AutoHideHeight")?.Value);
        Assert.Equal(minimum, written.Attribute("AutoHideMinWidth")?.Value);
        Assert.Equal(minimum, written.Attribute("AutoHideMinHeight")?.Value);
        LayoutAnchorable restored = new();
        Read(restored, written.ToString());
        Assert.Equal(tool.AutoHideWidth, restored.AutoHideWidth);
        Assert.Equal(tool.AutoHideHeight, restored.AutoHideHeight);
        Assert.Equal(tool.AutoHideMinWidth, restored.AutoHideMinWidth);
        Assert.Equal(tool.AutoHideMinHeight, restored.AutoHideMinHeight);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Return_Indices_Use_Current_Culture_While_Dimensions_Remain_Invariant(bool tool)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "minus";
        culture.NumberFormat.NumberDecimalSeparator = ",";
        CultureInfo.CurrentCulture = culture;
        try
        {
            LayoutContent source = tool ? new LayoutAnchorable() : new LayoutDocument();
            source.PreviousContainer = new LayoutDocumentPane();
            source.PreviousContainerIndex = -3;
            source.FloatingLeft = -1234.5;
            XElement written = Write(source);
            Assert.Equal("minus3", written.Attribute("PreviousContainerIndex")?.Value);
            Assert.Equal("-1234.5", written.Attribute("FloatingLeft")?.Value);
            LayoutContent restored = tool ? new LayoutAnchorable() : new LayoutDocument();
            Read(restored, written.ToString());
            Assert.Equal(-3, restored.PreviousContainerIndex);
            Assert.Equal(-1234.5, restored.FloatingLeft);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Complete_Layout_Round_Trips_Previous_And_Initial_Indices_With_A_Custom_Negative_Sign(bool tool)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "minus";
        CultureInfo.CurrentCulture = culture;
        try
        {
            DockingManager manager = new();
            XmlLayoutSerializer serializer = new(manager);
            serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
            string node = tool ? "LayoutAnchorable" : "LayoutDocument";
            serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutDocumentPane Id='pane'><" + node + " ContentId='item' PreviousContainerId='pane' PreviousContainerIndex='minus3' InitialContainerId='pane' InitialContainerIndex='minus4' /></LayoutDocumentPane></RootPanel><TopSide /><RightSide /><LeftSide /><BottomSide /><FloatingWindows /><Hidden /></LayoutRoot>"));
            using StringWriter output = new();
            serializer.Serialize(output);
            XElement item = Assert.Single(XElement.Parse(output.ToString()).Descendants(node));
            Assert.Equal("minus3", item.Attribute("PreviousContainerIndex")?.Value);
            Assert.Equal("minus4", item.Attribute("InitialContainerIndex")?.Value);
            serializer.Deserialize(new StringReader(output.ToString()));
            LayoutContent restored = Assert.Single(manager.Layout.Descendents().OfType<LayoutContent>());
            Assert.Equal(-3, restored.PreviousContainerIndex);
            using StringWriter repeated = new();
            serializer.Serialize(repeated);
            Assert.Equal("minus4", Assert.Single(XElement.Parse(repeated.ToString()).Descendants(node)).Attribute("InitialContainerIndex")?.Value);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [AvaloniaTheory]
    [InlineData("panel", "")]
    [InlineData("panel", "Diagonal")]
    [InlineData("panel", "2147483648")]
    [InlineData("document-group", "")]
    [InlineData("document-group", "Diagonal")]
    [InlineData("document-group", "2147483648")]
    [InlineData("tool-group", "")]
    [InlineData("tool-group", "Diagonal")]
    [InlineData("tool-group", "2147483648")]
    public void Invalid_Orientations_Expose_Enum_Parsing_Errors_Before_Later_Metadata(string kind, string value)
    {
        LayoutElement group = Create(kind);
        Exception? error = Record.Exception(() => Read(group, $"<Group Orientation='{value}' DockMinWidth='200' />"));
        if (value == "2147483648") Assert.IsType<OverflowException>(error);
        else Assert.IsType<ArgumentException>(error);
        Assert.Equal(25, group.DockModel.MinWidth);
    }

    private static LayoutElement Create(string kind) => kind switch
    {
        "document" => new LayoutDocument(),
        "tool" => new LayoutAnchorable(),
        "panel" => new LayoutPanel(),
        "document-pane" => new LayoutDocumentPane(),
        "tool-pane" => new LayoutAnchorablePane(),
        "document-group" => new LayoutDocumentPaneGroup(),
        "tool-group" => new LayoutAnchorablePaneGroup(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static void Read(LayoutElement element, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        ((IXmlSerializable)element).ReadXml(reader);
    }

    private static XElement Write(LayoutElement element)
    {
        XDocument xml = new();
        using (XmlWriter writer = xml.CreateWriter())
        {
            writer.WriteStartElement("Item");
            ((IXmlSerializable)element).WriteXml(writer);
            writer.WriteEndElement();
        }
        return xml.Root!;
    }
}
