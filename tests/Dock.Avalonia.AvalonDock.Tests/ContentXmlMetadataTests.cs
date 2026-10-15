using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContentXmlMetadataTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Metadata_Notifies_In_Inheritance_Order_Regardless_Of_Attribute_Order(bool tool)
    {
        LayoutContent content = Create(tool);
        List<string> changes = new();
        content.PropertyChanged += (_, e) => changes.Add(e.PropertyName!);
        Read(content, "<Content LastActivationTimeStamp='2001-02-03T04:05:06' CanFloat='false' CanClose='" + tool + "' IsMaximized='true' IsFloating='true' FloatingHeight='404' FloatingWidth='303' FloatingTop='22' FloatingLeft='11' InitialContainerIndex='4' InitialContainerId='initial' PreviousContainerIndex='3' PreviousContainerId='previous' IsLastFocusedDocument='true' ContentId='restored' IsSelected='true' Title='Restored' CanDockAsTabbedDocument='false' AutoHideMinHeight='140' AutoHideMinWidth='130' AutoHideHeight='202' AutoHideWidth='201' CanAutoHide='false' CanHide='false' CanMove='false' Description='Description' />");
        List<string> expected = tool
            ? new() { "CanHide", "CanAutoHide", "AutoHideWidth", "AutoHideHeight", "AutoHideMinWidth", "AutoHideMinHeight", "CanDockAsTabbedDocument" }
            : new() { "Description", "CanMove" };
        expected.AddRange(new[] { "Title", "IsSelected", "ContentId", "IsLastFocusedDocument", "PreviousContainerIndex", "InitialContainerIndex", "FloatingLeft", "FloatingTop", "FloatingWidth", "FloatingHeight", "IsFloating", "IsMaximized", "CanClose", "CanFloat", "LastActivationTimeStamp" });
        Assert.Equal(expected, changes);
        Assert.Equal(new DateTime(2001, 2, 3, 4, 5, 6), content.LastActivationTimeStamp);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selection_Callback_Sees_Derived_Metadata_And_Title_Before_The_New_Identifier(bool tool)
    {
        LayoutContent content = Create(tool);
        content.ContentId = "before";
        int callbacks = 0;
        content.IsSelectedChanged += (_, _) =>
        {
            callbacks++;
            Assert.True(content.IsSelected);
            Assert.Equal("Restored", content.Title);
            Assert.Equal("before", content.ContentId);
            if (content is LayoutDocument document) Assert.Equal("Details", document.Description);
            else Assert.False(((LayoutAnchorable)content).CanHide);
        };
        Read(content, "<Content Description='Details' CanHide='false' Title='Restored' IsSelected='true' ContentId='after' />");
        Assert.Equal(1, callbacks);
        Assert.Equal("after", content.ContentId);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Later_Floating_Error_Retains_Preceding_Metadata_And_The_Failing_Reader_Attribute(bool tool)
    {
        LayoutContent content = Create(tool);
        object application = new();
        content.Content = application;
        object native = content.DockModel;
        using XmlReader reader = Reader("<Content Title='Restored' IsSelected='true' ContentId='id' IsLastFocusedDocument='true' PreviousContainerId='pane' PreviousContainerIndex='2' InitialContainerId='first' InitialContainerIndex='3' FloatingLeft='11' FloatingTop='22' FloatingWidth='303' FloatingHeight='invalid' CanFloat='false' />");
        Assert.Throws<FormatException>(() => content.ReadXml(reader));
        Assert.Equal("Restored", content.Title);
        Assert.True(content.IsSelected);
        Assert.True(content.IsLastFocusedDocument);
        Assert.Equal("id", content.ContentId);
        Assert.Equal("pane", content.PreviousContainerId);
        Assert.Equal(2, content.PreviousContainerIndex);
        Assert.Equal(11, content.FloatingLeft);
        Assert.Equal(22, content.FloatingTop);
        Assert.Equal(303, content.FloatingWidth);
        Assert.Equal(0, content.FloatingHeight);
        Assert.True(content.CanFloat);
        Assert.Null(content.LastActivationTimeStamp);
        Assert.Same(native, content.DockModel);
        Assert.Same(application, content.Content);
        Assert.Equal(XmlNodeType.Attribute, reader.NodeType);
        Assert.Equal("FloatingHeight", reader.LocalName);
        Assert.Equal("invalid", reader.Value);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Invalid_Selection_Does_Not_Read_The_Later_Content_Identifier(bool tool)
    {
        LayoutContent content = Create(tool);
        Assert.Throws<FormatException>(() => Read(content, "<Content Description='Details' CanHide='false' Title='Restored' IsSelected='invalid' ContentId='later' />"));
        Assert.Equal("Restored", content.Title);
        Assert.Null(content.ContentId);
        if (content is LayoutDocument document) Assert.Equal("Details", document.Description);
        else Assert.False(((LayoutAnchorable)content).CanHide);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Invalid_Derived_Metadata_Stops_Before_Base_Content_Metadata(bool tool)
    {
        LayoutContent content = Create(tool);
        Assert.Throws<FormatException>(() => Read(content, tool
            ? "<Content CanHide='false' AutoHideWidth='240' AutoHideMinWidth='invalid' Title='later' />"
            : "<Content Description='Details' CanMove='invalid' Title='later' />"));
        Assert.Null(content.Title);
        if (content is LayoutDocument document) Assert.Equal("Details", document.Description);
        else
        {
            Assert.False(((LayoutAnchorable)content).CanHide);
            Assert.Equal(240, ((LayoutAnchorable)content).AutoHideWidth);
        }
    }

    [AvaloniaTheory]
    [InlineData(false, "0")]
    [InlineData(false, "1")]
    [InlineData(false, "invalid")]
    [InlineData(true, "0")]
    [InlineData(true, "1")]
    [InlineData(true, "invalid")]
    public void Floating_Flag_Uses_Boolean_Parsing_And_Preserves_Earlier_Dimensions_On_Error(bool tool, string value)
    {
        LayoutContent content = Create(tool);
        Assert.Throws<FormatException>(() => Read(content, "<Content FloatingHeight='404' IsFloating='" + value + "' IsMaximized='true' />"));
        Assert.Equal(404, content.FloatingHeight);
        Assert.False(content.IsFloating);
        Assert.False(content.IsMaximized);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Throwing_Selection_Callback_Stops_Reading_At_The_Selection_Attribute(bool tool)
    {
        LayoutContent content = Create(tool);
        InvalidOperationException failure = new("Application callback");
        content.IsSelectedChanged += (_, _) => throw failure;
        using XmlReader reader = Reader("<Content Title='Restored' IsSelected='true' ContentId='later' />");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => content.ReadXml(reader)));
        Assert.Equal("Restored", content.Title);
        Assert.True(content.IsSelected);
        Assert.Null(content.ContentId);
        Assert.Equal(XmlNodeType.Attribute, reader.NodeType);
        Assert.Equal("IsSelected", reader.LocalName);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Last_Focused_Flag_Restores_And_Clears_Without_Activation(bool tool)
    {
        LayoutContent content = Create(tool);
        List<string> events = new();
        content.PropertyChanging += (_, e) => events.Add("changing:" + e.PropertyName);
        content.PropertyChanged += (_, e) => events.Add("changed:" + e.PropertyName);
        Read(content, "<Content IsLastFocusedDocument='true' />");
        Assert.True(content.IsLastFocusedDocument);
        Assert.False(content.IsActive);
        Read(content, "<Content IsLastFocusedDocument='true' />");
        Read(content, "<Content IsLastFocusedDocument='false' />");
        Assert.False(content.IsLastFocusedDocument);
        Assert.Equal(new[] { "changing:IsLastFocusedDocument", "changed:IsLastFocusedDocument", "changing:IsLastFocusedDocument", "changed:IsLastFocusedDocument" }, events);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Reading_Flags_On_Attached_Content_Does_Not_Change_Focus_Or_Create_A_Window(bool tool, bool value)
    {
        LayoutContent target = Create(tool);
        LayoutDocument active = new();
        LayoutDocumentPane pane = new(active);
        pane.Children.Add(target);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        manager.Layout.ActiveContent = active;
        DockingWindowState nativeState = target.DockModel.DockingState;
        Read(target, "<Content IsLastFocusedDocument='" + value + "' IsFloating='" + value + "' />");
        Assert.Equal(value, target.IsLastFocusedDocument);
        Assert.Equal(value, target.IsFloating);
        Assert.False(target.IsActive);
        Assert.Same(active, manager.Layout.ActiveContent);
        Assert.Same(active, manager.Layout.LastFocusedDocument);
        Assert.Same(pane, target.Parent);
        Assert.Equal(nativeState, target.DockModel.DockingState);
        Assert.Empty(manager.Layout.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Metadata_Round_Trips_And_Subsequent_Native_State_Remains_Authoritative(bool tool)
    {
        LayoutContent content = Create(tool);
        List<string> notifications = new();
        content.PropertyChanging += (_, e) => notifications.Add("changing:" + e.PropertyName);
        content.PropertyChanged += (_, e) => notifications.Add("changed:" + e.PropertyName);
        Read(content, "<Content IsFloating='true' />");
        Read(content, "<Content IsFloating='true' />");
        Assert.Equal(new[] { "changed:IsFloating" }, notifications);
        Assert.Equal("True", Write(content).Attribute("IsFloating")?.Value);
        content.DockModel.DockingState = DockingWindowState.Document;
        Assert.False(content.IsFloating);
        Assert.Null(Write(content).Attribute("IsFloating"));
        content.DockModel.DockingState = DockingWindowState.Document | DockingWindowState.Floating;
        Assert.True(content.IsFloating);
        Read(content, "<Content IsFloating='false' />");
        Assert.False(content.IsFloating);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, content.DockModel.DockingState);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_State_Observers_See_The_Current_Floating_Value_And_Can_Restore_Metadata(bool tool)
    {
        LayoutContent content = Create(tool);
        List<bool> observations = new();
        content.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != "DockingState") return;
            observations.Add(content.IsFloating);
            Read(content, "<Content IsFloating='false' />");
        };
        content.DockModel.DockingState = DockingWindowState.Floating;
        Assert.Equal(new[] { true }, observations);
        Assert.False(content.IsFloating);
        Assert.Equal(DockingWindowState.Floating, content.DockModel.DockingState);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reading_The_Focus_Flag_False_Does_Not_Clear_The_Roots_Existing_Focus_References(bool tool)
    {
        LayoutContent content = Create(tool);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) } };
        manager.Layout.ActiveContent = content;
        Assert.True(content.IsLastFocusedDocument);
        Read(content, "<Content IsLastFocusedDocument='false' />");
        Assert.False(content.IsLastFocusedDocument);
        Assert.True(content.IsActive);
        Assert.Same(content, manager.Layout.ActiveContent);
        Assert.Same(content, manager.Layout.LastFocusedDocument);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Content_Callback_Receives_Focus_Metadata_And_Stored_Time_Without_Activation(bool custom)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        if (custom) serializer.KnownTypes.Register("ApplicationDocument", static () => new ExtendedDocument());
        int callbacks = 0;
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            callbacks++;
            Assert.True(args.Model.IsSelected);
            Assert.True(args.Model.IsLastFocusedDocument);
            Assert.False(args.Model.IsActive);
            Assert.Null(args.Model.Root!.ActiveContent);
            Assert.Equal(new DateTime(2001, 2, 3, 4, 5, 6), args.Model.LastActivationTimeStamp);
            if (custom) Assert.Equal("extension", Assert.IsType<ExtendedDocument>(args.Model).Extra);
            args.Content = new object();
        };
        string node = custom
            ? "<ApplicationDocument ContentId='id' IsSelected='true' IsLastFocusedDocument='true' LastActivationTimeStamp='2001-02-03T04:05:06'><Extra>extension</Extra></ApplicationDocument>"
            : "<LayoutDocument ContentId='id' IsSelected='true' IsLastFocusedDocument='true' LastActivationTimeStamp='2001-02-03T04:05:06' />";
        serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutDocumentPane>" + node + "</LayoutDocumentPane></RootPanel><TopSide /><RightSide /><LeftSide /><BottomSide /><FloatingWindows /><Hidden /></LayoutRoot>"));
        Assert.Equal(1, callbacks);
        Assert.Null(manager.Layout.ActiveContent);
        Assert.Null(manager.Layout.LastFocusedDocument);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Auto_Hide_Size_Is_Clamped_Before_Reading_The_Persisted_Minimum(bool horizontal)
    {
        LayoutAnchorable tool = new();
        Read(tool, horizontal
            ? "<Content AutoHideMinWidth='10' AutoHideWidth='50' />"
            : "<Content AutoHideMinHeight='10' AutoHideHeight='50' />");
        Assert.Equal(100, horizontal ? tool.AutoHideWidth : tool.AutoHideHeight);
        Assert.Equal(10, horizontal ? tool.AutoHideMinWidth : tool.AutoHideMinHeight);
    }

    [AvaloniaTheory]
    [InlineData(false, "<Content Title='read' /><Next />", XmlNodeType.Element, "Next")]
    [InlineData(true, "<Content Title='read' /><Next />", XmlNodeType.Element, "Next")]
    [InlineData(false, "<Content Title='read'></Content><Next />", XmlNodeType.EndElement, "Content")]
    [InlineData(true, "<Content Title='read'></Content><Next />", XmlNodeType.EndElement, "Content")]
    [InlineData(false, "<Content Title='read'><Extra /></Content>", XmlNodeType.Element, "Extra")]
    [InlineData(true, "<Content Title='read'><Extra /></Content>", XmlNodeType.Element, "Extra")]
    [InlineData(false, "<Content Title='read'>text</Content>", XmlNodeType.Text, "")]
    [InlineData(true, "<Content Title='read'>text</Content>", XmlNodeType.Text, "")]
    [InlineData(false, "<Content Title='read'><!--comment--></Content>", XmlNodeType.Comment, "")]
    [InlineData(true, "<Content Title='read'><!--comment--></Content>", XmlNodeType.Comment, "")]
    [InlineData(false, "<Content Title='read'> </Content>", XmlNodeType.Whitespace, "")]
    [InlineData(true, "<Content Title='read'> </Content>", XmlNodeType.Whitespace, "")]
    public void Content_Read_Advances_Once_Without_Consuming_Application_Child_Content(bool tool, string xml, XmlNodeType type, string name)
    {
        LayoutContent content = Create(tool);
        using XmlReader reader = Reader(xml);
        content.ReadXml(reader);
        Assert.Equal("read", content.Title);
        Assert.Equal(type, reader.NodeType);
        Assert.Equal(name, reader.LocalName);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Later_Syntax_Error_Does_Not_Prevent_Metadata_Restoration(bool tool)
    {
        LayoutContent content = Create(tool);
        using XmlReader reader = Reader("<Content Title='read' IsSelected='true'><Unfinished>");
        content.ReadXml(reader);
        Assert.Equal("read", content.Title);
        Assert.True(content.IsSelected);
        Assert.Equal("Unfinished", reader.LocalName);
        Assert.Throws<XmlException>(() => reader.Read());
    }

    [AvaloniaFact]
    public void A_Document_Override_Can_Read_Its_Own_Child_Content_After_Base_Metadata()
    {
        ExtendedDocument document = new();
        using XmlReader reader = Reader("<Content Title='read' Description='details'><Extra>application</Extra></Content><Next />");
        document.ReadXml(reader);
        Assert.Equal("read", document.Title);
        Assert.Equal("details", document.Description);
        Assert.Equal("application", document.Extra);
        Assert.Equal(XmlNodeType.Element, reader.NodeType);
        Assert.Equal("Next", reader.LocalName);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Wrong_Thread_Read_Rejects_Metadata_Before_Any_Mutation(bool tool)
    {
        LayoutContent content = Create(tool);
        await Task.Run(() => Assert.Throws<InvalidOperationException>(() => Read(content, "<Content Description='later' CanHide='false' Title='later' IsFloating='true' />")));
        Assert.Null(content.Title);
        Assert.False(content.IsFloating);
        if (content is LayoutDocument document) Assert.Null(document.Description);
        else Assert.True(((LayoutAnchorable)content).CanHide);
    }

    private static LayoutContent Create(bool tool) => tool ? new LayoutAnchorable() : new LayoutDocument();

    private static XmlReader Reader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });
        reader.MoveToContent();
        return reader;
    }

    private static void Read(LayoutContent content, string xml)
    {
        using XmlReader reader = Reader(xml);
        content.ReadXml(reader);
    }

    private static XElement Write(LayoutContent content)
    {
        XDocument xml = new();
        using (XmlWriter writer = xml.CreateWriter())
        {
            writer.WriteStartElement("Content");
            content.WriteXml(writer);
            writer.WriteEndElement();
        }
        return xml.Root!;
    }

    private sealed class ExtendedDocument : LayoutDocument
    {
        internal string? Extra { get; private set; }
        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Extra = reader.ReadElementContentAsString("Extra", "");
            reader.ReadEndElement();
        }
    }
}
