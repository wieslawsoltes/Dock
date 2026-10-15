using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutMetadataContractTests
{
    [AvaloniaTheory]
    [InlineData("Description")]
    [InlineData("ToolTip")]
    [InlineData("IconSource")]
    [InlineData("IsEnabled")]
    [InlineData("LastActivationTimeStamp")]
    [InlineData("CanHide")]
    [InlineData("CanMove")]
    [InlineData("CanAutoHide")]
    [InlineData("CanDockAsTabbedDocument")]
    [InlineData("ShowHeader")]
    [InlineData("IsMaximized")]
    public void Changed_Only_Metadata_Reports_The_Public_Name_Once(string property)
    {
        LayoutDocument document = new();
        LayoutAnchorable tool = new();
        LayoutDocumentPane pane = new();
        object tip = new();
        DrawingImage icon = new();
        DateTime time = new(2020, 1, 1);
        (LayoutElement Element, Action Assign) operation = property switch
        {
            "Description" => (document, () => document.Description = "description"),
            "ToolTip" => (document, () => document.ToolTip = tip),
            "IconSource" => (document, () => document.IconSource = icon),
            "IsEnabled" => (document, () => document.IsEnabled = false),
            "LastActivationTimeStamp" => (document, () => document.LastActivationTimeStamp = time),
            "CanHide" => (tool, () => tool.CanHide = false),
            "CanMove" => (document, () => document.CanMove = false),
            "CanAutoHide" => (tool, () => tool.CanAutoHide = false),
            "CanDockAsTabbedDocument" => (tool, () => tool.CanDockAsTabbedDocument = false),
            "ShowHeader" => (pane, () => pane.ShowHeader = false),
            _ => (pane, () => pane.IsMaximized = true)
        };
        List<string> events = new();
        operation.Element.PropertyChanging += (_, args) => events.Add("changing:" + args.PropertyName);
        operation.Element.PropertyChanged += (_, args) => events.Add("changed:" + args.PropertyName);

        operation.Assign();
        operation.Assign();

        Assert.Equal(new[] { "changed:" + property }, events);
    }

    [AvaloniaFact]
    public void Native_Capability_Changes_Notify_Their_Public_Properties()
    {
        LayoutDocument document = new();
        LayoutAnchorable tool = new();
        List<string> events = new();
        document.PropertyChanged += (_, args) => events.Add(args.PropertyName!);
        tool.PropertyChanged += (_, args) => events.Add(args.PropertyName!);

        document.DockModel.CanDrag = false;
        tool.DockModel.CanPin = false;
        tool.DockModel.CanDockAsDocument = false;

        Assert.Equal(new[] { "CanMove", "CanAutoHide", "CanDockAsTabbedDocument" }, events);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Title_Default_And_Notifications_Preserve_Before_And_After_Values(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        Assert.Null(content.Title);
        List<(string, string?)> events = new();
        content.PropertyChanging += (_, args) => events.Add(("changing:" + args.PropertyName, content.Title));
        content.PropertyChanged += (_, args) => events.Add(("changed:" + args.PropertyName, content.Title));
        content.Title = null!;
        content.Title = "title";
        content.Title = new string("title".ToCharArray());
        content.Title = null!;
        Assert.Equal(new (string, string?)[] { ("changing:Title", null), ("changed:Title", "title"), ("changing:Title", "title"), ("changed:Title", null) }, events);
        Assert.Null(new LayoutAnchorablePane().Name);
    }

    [AvaloniaFact]
    public void Tooltip_Changes_Use_Reference_Identity_Even_For_Equal_Objects()
    {
        LayoutDocument document = new();
        EqualTooltip first = new("tip"), second = new("tip");
        List<object?> observed = new();
        document.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(document.ToolTip)) observed.Add(document.ToolTip); };
        document.ToolTip = first;
        document.ToolTip = first;
        document.ToolTip = second;
        Assert.Equal(2, observed.Count);
        Assert.Same(first, observed[0]);
        Assert.Same(second, observed[1]);
    }

    [AvaloniaTheory]
    [InlineData("document")]
    [InlineData("tool")]
    [InlineData("pane")]
    public void Floating_Dimensions_Default_To_Zero_And_Repeat_NaN_Assignments_Notify(string kind)
    {
        LayoutElement element = kind switch { "document" => new LayoutDocument(), "tool" => new LayoutAnchorable(), _ => new LayoutDocumentPane() };
        LayoutContent? content = element as LayoutContent;
        LayoutDocumentPane? pane = element as LayoutDocumentPane;
        Assert.Equal(0, content?.FloatingWidth ?? pane!.FloatingWidth);
        Assert.Equal(0, content?.FloatingHeight ?? pane!.FloatingHeight);
        List<string> events = new();
        element.PropertyChanging += (_, args) => events.Add("changing:" + args.PropertyName);
        element.PropertyChanged += (_, args) => events.Add("changed:" + args.PropertyName);
        for (int i = 0; i < 2; i++)
            if (content is not null) content.FloatingWidth = double.NaN;
            else pane!.FloatingWidth = double.NaN;
        Assert.Equal(new[] { "changing:FloatingWidth", "changed:FloatingWidth", "changing:FloatingWidth", "changed:FloatingWidth" }, events);
        Assert.True(double.IsNaN(content?.FloatingWidth ?? pane!.FloatingWidth));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unspecified_Floating_Dimensions_Use_Valid_Native_Window_Sizes(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutPanel panel = new();
        panel.Children.Add(tool ? new LayoutAnchorablePane((LayoutAnchorable)content) : new LayoutDocumentPane(content));
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = panel } };
        Assert.Equal(0, content.FloatingWidth);
        Assert.Equal(0, content.FloatingHeight);
        content.Float();
        LayoutFloatingWindow window = Assert.Single(manager.Layout.FloatingWindows);
        Assert.True(window.DockWindow.Width > 0 && double.IsFinite(window.DockWindow.Width));
        Assert.True(window.DockWindow.Height > 0 && double.IsFinite(window.DockWindow.Height));
        Assert.Equal(window.DockWindow.Width, content.FloatingWidth);
        Assert.Equal(window.DockWindow.Height, content.FloatingHeight);
    }

    [AvaloniaFact]
    public void Sorted_Children_Use_Application_Comparison_Before_Titles()
    {
        LayoutDocument higher = new() { Title = "A", Content = 20 };
        LayoutDocument lower = new() { Title = "Z", Content = 10 };
        LayoutDocumentPane pane = new() { Children = { higher, lower } };
        Assert.Equal(new[] { lower, higher }, pane.ChildrenSorted.ToArray());
        higher.Content = lower.Content = new object();
        Assert.Equal(new[] { higher, lower }, pane.ChildrenSorted.ToArray());
    }

    [AvaloniaFact]
    public void Content_Comparison_Passes_The_Other_Content_And_Preserves_Application_Exceptions()
    {
        RecordingComparable comparable = new();
        LayoutDocument first = new() { Content = comparable };
        LayoutDocument second = new() { Content = new object() };
        Assert.Equal(17, first.CompareTo(second));
        Assert.Same(second.Content, comparable.Other);
        comparable.Throw = true;
        Assert.Throws<InvalidOperationException>(() => first.CompareTo(second));
        Assert.Throws<NullReferenceException>(() => first.CompareTo(null));
        first.Content = null;
        Assert.Throws<NullReferenceException>(() => first.CompareTo(null));
    }

    private sealed record EqualTooltip(string Text);
    private sealed class RecordingComparable : IComparable
    {
        public object? Other { get; private set; }
        public bool Throw { get; set; }
        public int CompareTo(object? other)
        {
            Other = other;
            if (Throw) throw new InvalidOperationException("application comparison");
            return 17;
        }
    }
}
