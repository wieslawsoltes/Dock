using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class GroupXmlMetadataTests
{
    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    public void Metadata_Restores_Silently_Into_The_Existing_Native_Model(string kind)
    {
        LayoutElement group = Create(kind);
        object native = group.DockModel;
        List<string> notifications = new();
        group.PropertyChanging += (_, e) => notifications.Add("changing:" + e.PropertyName);
        group.PropertyChanged += (_, e) => notifications.Add("changed:" + e.PropertyName);
        Read(group, "<Group Id='pane-id' Name='Tools' ShowHeader='false' DockWidth='145' DockHeight='2*' DockMinWidth='125' DockMinHeight='175' FloatingWidth='501' FloatingHeight='401' FloatingLeft='31' FloatingTop='41' IsMaximized='true' />");
        Assert.All(notifications, value => Assert.EndsWith(":IsVisible", value));
        Assert.Same(native, group.DockModel);
        Assert.Equal(125, group.DockModel.MinWidth);
        Assert.Equal(175, group.DockModel.MinHeight);
        XElement result = Write(group);
        foreach ((string name, string value) in new[] { ("Id", "pane-id"), ("DockWidth", "145"), ("DockHeight", "2*"), ("DockMinWidth", "125"), ("DockMinHeight", "175"), ("FloatingWidth", "501"), ("FloatingHeight", "401"), ("FloatingLeft", "31"), ("FloatingTop", "41"), ("IsMaximized", "true") })
            Assert.Equal(value, result.Attribute(name)?.Value);
        if (group is LayoutDocumentPane documents) Assert.False(documents.ShowHeader);
        if (group is LayoutAnchorablePane tools) Assert.Equal("Tools", tools.Name);
    }

    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    public void Xml_Minima_Use_Stored_Values_Without_Public_Setter_Validation(string kind)
    {
        LayoutElement group = Create(kind);
        Read(group, "<Group DockMinWidth='-12' DockMinHeight='-34' />");
        Assert.Equal(-12, group.DockModel.MinWidth);
        Assert.Equal(-34, group.DockModel.MinHeight);
    }

    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    public void A_Later_Length_Error_Preserves_Earlier_Metadata_In_Inheritance_Order(string kind)
    {
        LayoutElement group = Create(kind);
        Assert.NotNull(Record.Exception(() => Read(group,
            "<Group Id='restored-id' Name='Restored tools' ShowHeader='false' Orientation='Vertical' DockWidth='146' DockHeight='invalid' DockMinWidth='200' />")));
        XElement result = Write(group);
        Assert.Equal("146", result.Attribute("DockWidth")?.Value);
        Assert.Equal("1*", result.Attribute("DockHeight")?.Value);
        Assert.Equal(25, group.DockModel.MinWidth);
        if (group is LayoutDocumentPane documents) Assert.False(documents.ShowHeader);
        if (group is LayoutAnchorablePane tools) Assert.Equal("Restored tools", tools.Name);
        if (group is ILayoutOrientableGroup orientable) Assert.Equal(Orientation.Vertical, orientable.Orientation);
    }

    [AvaloniaTheory]
    [InlineData("panel")]
    [InlineData("document-pane")]
    [InlineData("tool-pane")]
    [InlineData("document-group")]
    [InlineData("tool-group")]
    public void A_Later_Floating_Error_Preserves_Earlier_Silent_Values(string kind)
    {
        LayoutElement group = Create(kind);
        List<string> notifications = new();
        group.PropertyChanged += (_, e) => notifications.Add(e.PropertyName!);
        Assert.Throws<FormatException>(() => Read(group,
            "<Group DockMinWidth='130' DockMinHeight='140' FloatingWidth='502' FloatingHeight='invalid' FloatingLeft='99' />"));
        Assert.Empty(notifications);
        XElement result = Write(group);
        Assert.Equal("502", result.Attribute("FloatingWidth")?.Value);
        Assert.Equal("0", result.Attribute("FloatingHeight")?.Value);
        Assert.Equal("0", result.Attribute("FloatingLeft")?.Value);
        Assert.Equal(130, group.DockModel.MinWidth);
        Assert.Equal(140, group.DockModel.MinHeight);
    }

    [AvaloniaFact]
    public void Native_Observers_See_Restored_Minima_And_Reentrant_Public_Changes_Still_Notify()
    {
        LayoutPanel group = new();
        List<double> native = new();
        List<string> publicChanges = new();
        group.PropertyChanging += (_, e) => publicChanges.Add("changing:" + e.PropertyName);
        group.PropertyChanged += (_, e) => publicChanges.Add("changed:" + e.PropertyName);
        group.DockModel.PropertyChanged += (_, e) =>
        {
            if (e.Property != DockableBase.MinWidthProperty) return;
            native.Add(group.DockModel.MinWidth);
            if (group.DockModel.MinWidth == 80) group.DockMinWidth = 90;
        };
        Read(group, "<Group DockMinWidth='80'></Group>");
        Assert.Equal(new[] { 80d, 90d }, native);
        Assert.Equal(90, group.DockMinWidth);
        Assert.Single(publicChanges, value => value == "changing:DockMinWidth");
        Assert.Single(publicChanges, value => value == "changed:DockMinWidth");
        publicChanges.Clear();
        group.DockMinWidth = 100;
        Assert.Contains("changing:DockMinWidth", publicChanges);
        Assert.Contains("changed:DockMinWidth", publicChanges);
        Assert.Throws<ArgumentException>(() => group.DockMinWidth = -1);
    }

    [AvaloniaFact]
    public void A_Throwing_Native_Metadata_Observer_Does_Not_Suppress_Later_Public_Changes()
    {
        LayoutPanel group = new();
        group.DockModel.PropertyChanged += (_, e) =>
        {
            if (e.Property == DockableBase.MinWidthProperty && group.DockModel.MinWidth == 80) throw new ObserverException();
        };
        Assert.Throws<ObserverException>(() => Read(group, "<Group DockMinWidth='80' />"));
        List<string> changes = new();
        group.PropertyChanged += (_, e) => changes.Add(e.PropertyName!);
        group.DockMinWidth = 100;
        Assert.Contains(nameof(group.DockMinWidth), changes);
    }

    [AvaloniaFact]
    public void Silent_Pane_Metadata_Does_Not_Disable_Ordinary_Header_Name_Or_Size_Notifications()
    {
        LayoutDocumentPane document = new();
        LayoutAnchorablePane tool = new();
        Read(document, "<Pane ShowHeader='false' IsMaximized='true' />");
        Read(tool, "<Pane Name='Restored' />");
        List<string> changes = new();
        document.PropertyChanged += (_, e) => changes.Add(e.PropertyName!);
        tool.PropertyChanged += (_, e) => changes.Add(e.PropertyName!);
        document.ShowHeader = true; document.IsMaximized = false; document.FloatingWidth = 250;
        tool.Name = "Updated";
        Assert.Equal(new[] { "ShowHeader", "IsMaximized", "FloatingWidth", "Name" }, changes);
    }

    [AvaloniaFact]
    public void A_Styled_Native_Model_Does_Not_Freeze_The_Public_Pane_Name()
    {
        LayoutAnchorablePane tool = new();
        tool.DockModel.Name = "NativeStyleName";
        tool.DockModel.ApplyStyling();
        List<string?> changes = new();
        tool.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(tool.Name)) changes.Add(tool.Name); };
        tool.Name = "First";
        tool.Name = new string("First".ToCharArray());
        Read(tool, "<Pane Name='Restored' />");
        Assert.Equal("Restored", tool.Name);
        tool.Name = "Second";
        tool.Name = null!;
        Assert.Equal(new string?[] { "First", "Second", null }, changes);
        Assert.Equal("NativeStyleName", tool.DockModel.Name);
    }

    [AvaloniaFact]
    public void Auto_Hide_Group_Identifiers_Restore_Before_Children_Without_Public_Notifications()
    {
        LayoutAnchorGroup group = new();
        List<string> changes = new();
        group.PropertyChanged += (_, e) => changes.Add(e.PropertyName!);
        Assert.Throws<XmlException>(() => Read(group, "<Group Id='restored-id' PreviousContainerId='return-pane'><Unknown /></Group>"));
        Assert.Equal("restored-id", group.DockModel.Id);
        Assert.Empty(changes);
    }

    [AvaloniaFact]
    public async Task Rejected_Thread_Access_Does_Not_Alter_Identifier_Or_Dimension_Metadata()
    {
        LayoutPanel group = new();
        string initialId = group.DockModel.Id;
        Exception? error = await Task.Run(() => Record.Exception(() => Read(group, "<Group Id='changed' DockMinWidth='130' />")));
        Assert.IsType<InvalidOperationException>(error);
        Assert.Equal(initialId, group.DockModel.Id);
        Assert.Equal(25, group.DockMinWidth);
        Assert.False(string.IsNullOrEmpty(Write(group).Attribute("Id")?.Value));
    }

    public static IEnumerable<object[]> Orientations()
    {
        foreach (string kind in new[] { "panel", "document-group", "tool-group" })
        foreach ((string text, int expected) in new[] { ("vertical", 1), ("VERTICAL", 1), (" Horizontal ", 0), ("1", 1), ("2", 2), ("Horizontal, Vertical", 1) })
            yield return new object[] { kind, text, expected };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Orientations))]
    public void Orientation_Accepts_The_Upstream_Enum_Spellings(string kind, string text, int expected)
    {
        LayoutElement group = Create(kind);
        Read(group, "<Group Orientation='" + text + "' />");
        Assert.Equal(expected, (int)((ILayoutOrientableGroup)group).Orientation);
    }

    public static IEnumerable<object[]> EmptyGroups()
    {
        foreach (string kind in new[] { "panel", "document-pane", "tool-pane", "document-group", "tool-group", "anchor-group", "anchor-side" })
        foreach (bool selfClosing in new[] { false, true }) yield return new object[] { kind, selfClosing };
    }

    [AvaloniaTheory]
    [MemberData(nameof(EmptyGroups))]
    public void Only_Self_Closing_Empty_Groups_Recompute_Visibility(string kind, bool selfClosing)
    {
        LayoutElement group = Create(kind);
        Assert.True(((ILayoutPanelElement)group).IsVisible);
        Read(group, selfClosing ? "<Group />" : "<Group></Group>");
        Assert.Equal(!selfClosing || kind is "document-pane" or "document-group", ((ILayoutPanelElement)group).IsVisible);
    }

    private static LayoutElement Create(string kind) => kind switch
    {
        "document-pane" => new LayoutDocumentPane(),
        "tool-pane" => new LayoutAnchorablePane(),
        "document-group" => new LayoutDocumentPaneGroup(),
        "tool-group" => new LayoutAnchorablePaneGroup(),
        "anchor-group" => new LayoutAnchorGroup(),
        "anchor-side" => new LayoutAnchorSide(),
        _ => new LayoutPanel()
    };

    private static void Read(LayoutElement element, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        ((IXmlSerializable)element).ReadXml(reader);
        Assert.True(reader.EOF);
    }

    private static XElement Write(LayoutElement element)
    {
        XDocument result = new();
        using (XmlWriter writer = result.CreateWriter())
        {
            writer.WriteStartElement("Group");
            ((IXmlSerializable)element).WriteXml(writer);
            writer.WriteEndElement();
        }
        return result.Root!;
    }

    private sealed class ObserverException : Exception { }
}
