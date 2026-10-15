using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ToolPaneXmlSelectionTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Automatic_Selection_Waits_For_Complete_Xml_While_Explicit_Selection_Is_Immediate(bool selected)
    {
        ObservedPane pane = new();
        Read(pane, "<Pane><LayoutAnchorable ContentId='first' /><LayoutAnchorable ContentId='second' IsSelected='" + selected + "' /></Pane>");
        Assert.Equal(new[] { -1, selected ? 1 : -1 }, pane.ObservedSelection);
        Assert.Equal(selected ? 1 : 0, pane.SelectedContentIndex);
        Assert.Equal(selected ? "second" : "first", pane.SelectedContent!.ContentId);
        Assert.Equal(selected, pane.Children[1].IsSelected);
        Assert.Equal(!selected, pane.Children[0].IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Child_Observers_Can_Select_Content_During_Xml_Restoration(bool secondSelected)
    {
        ObservedPane pane = new();
        pane.AfterChildrenChanged = () =>
        {
            if (pane.ChildrenCount == 1) pane.SelectedContentIndex = 0;
        };
        Read(pane, "<Pane><LayoutAnchorable ContentId='first' /><LayoutAnchorable ContentId='second' IsSelected='" + secondSelected + "' /></Pane>");
        Assert.Equal(new[] { -1, secondSelected ? 1 : 0 }, pane.ObservedSelection);
        Assert.Equal(secondSelected ? 1 : 0, pane.SelectedContentIndex);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Failed_Xml_Leaves_Automatic_Selection_Disabled_Until_A_Successful_Read(bool metadataFailure)
    {
        ObservedPane pane = new();
        Exception? error = Record.Exception(() => Read(pane, metadataFailure
            ? "<Pane Name='retained' DockMinWidth='invalid' />"
            : "<Pane Name='retained'><LayoutAnchorable ContentId='first' /><LayoutAnchorable CanHide='invalid' /></Pane>"));
        if (!metadataFailure) error = Assert.IsType<InvalidOperationException>(error).InnerException;
        Assert.IsType<FormatException>(error);
        Assert.Equal("retained", pane.Name);
        pane.Children.Add(new LayoutAnchorable { ContentId = "added" });
        Assert.Equal(-1, pane.SelectedContentIndex);
        Assert.All(pane.Children, child => Assert.False(child.IsSelected));
        Read(pane, "<Pane />");
        Assert.Equal(0, pane.SelectedContentIndex);
        Assert.True(pane.Children[0].IsSelected);
    }

    [AvaloniaFact]
    public void A_Throwing_Child_Observer_Leaves_Automatic_Selection_Disabled()
    {
        ObservedPane pane = new();
        InvalidOperationException failure = new("Application observer");
        pane.AfterChildrenChanged = () => throw failure;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => Read(pane, "<Pane><LayoutAnchorable /></Pane>")));
        pane.AfterChildrenChanged = null;
        pane.Children.Add(new LayoutAnchorable());
        Assert.Equal(-1, pane.SelectedContentIndex);
        Read(pane, "<Pane></Pane>");
        Assert.Equal(0, pane.SelectedContentIndex);
    }

    [AvaloniaFact]
    public void A_Successful_Nested_Read_Reenables_Selection_During_The_Outer_Read()
    {
        ObservedPane pane = new();
        pane.AfterChildrenChanged = () =>
        {
            if (pane.ChildrenCount == 1) Read(pane, "<Pane />");
        };
        Read(pane, "<Pane><LayoutAnchorable ContentId='first' /><LayoutAnchorable ContentId='second' /></Pane>");
        Assert.Equal(new[] { -1, 0 }, pane.ObservedSelection);
        Assert.Equal(0, pane.SelectedContentIndex);
    }

    [AvaloniaFact]
    public void A_Registered_Pane_Hook_Observes_No_Fallback_Before_All_Children_Arrive()
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        ObservedPane? restored = null;
        serializer.KnownTypes.Register("ApplicationPane", () => restored = new ObservedPane());
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><ApplicationPane><LayoutAnchorable ContentId='first' /><LayoutAnchorable ContentId='second' /></ApplicationPane></RootPanel><TopSide /><RightSide /><LeftSide /><BottomSide /><FloatingWindows /><Hidden /></LayoutRoot>"));
        Assert.NotNull(restored);
        Assert.Equal(new[] { -1, -1 }, restored.ObservedSelection);
        Assert.Equal(0, restored.SelectedContentIndex);
        Assert.Equal("first", restored.SelectedContent!.ContentId);
        Assert.False(restored.SelectedContent.IsActive);
    }

    [AvaloniaFact]
    public void A_Builtin_Pane_Does_Not_Select_Its_First_Child_Before_Creating_The_Second()
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        FirstTool? first = null;
        serializer.KnownTypes.Register("FirstTool", () => first = new FirstTool());
        serializer.KnownTypes.Register("SecondTool", () =>
        {
            Assert.NotNull(first);
            LayoutAnchorablePane parent = Assert.IsType<LayoutAnchorablePane>(first.Parent);
            Assert.False(first.IsSelected);
            Assert.Equal(-1, parent.SelectedContentIndex);
            return new SecondTool();
        });
        serializer.LayoutSerializationCallback += (_, args) => args.Content = new object();
        serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutAnchorablePane><FirstTool ContentId='first' /><SecondTool ContentId='second' /></LayoutAnchorablePane></RootPanel><TopSide /><RightSide /><LeftSide /><BottomSide /><FloatingWindows /><Hidden /></LayoutRoot>"));
        Assert.True(first!.IsSelected);
    }

    private static void Read(LayoutAnchorablePane pane, string xml)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        reader.MoveToContent();
        pane.ReadXml(reader);
    }

    private sealed class ObservedPane : LayoutAnchorablePane
    {
        internal List<int> ObservedSelection { get; } = new();
        internal Action? AfterChildrenChanged { get; set; }
        protected override void OnChildrenCollectionChanged()
        {
            base.OnChildrenCollectionChanged();
            ObservedSelection.Add(SelectedContentIndex);
            AfterChildrenChanged?.Invoke();
        }
    }

    private sealed class FirstTool : LayoutAnchorable;
    private sealed class SecondTool : LayoutAnchorable;
}
