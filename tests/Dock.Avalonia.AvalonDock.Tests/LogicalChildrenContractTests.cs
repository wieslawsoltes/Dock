using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LogicalChildrenContractTests
{
    [AvaloniaFact]
    public void Manager_Registers_Application_Controls_And_Lazy_Views_Instead_Of_Containers()
    {
        TextBox documentContent = new();
        Border toolContent = new();
        LayoutDocument document = new() { Content = documentContent };
        LayoutAnchorable tool = new() { Content = toolContent };
        DockingManager manager = new()
        {
            Layout = new()
            {
                RootPanel = new()
                {
                    Children =
                    {
                        new LayoutAnchorablePane(tool),
                        new LayoutDocumentPane
                        {
                            Children = { document, new LayoutDocument { Content = "text" },
                                new LayoutDocument { Content = new object() }, new LayoutDocument() }
                        }
                    }
                }
            }
        };

        Assert.Equal(new object[] { documentContent, toolContent }, Read(manager.LogicalChildrenPublic));
        Assert.Equal(new ILogical[] { documentContent, toolContent }, manager.GetLogicalChildren());
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        Assert.DoesNotContain(item, manager.GetLogicalChildren());
        Control view = item.View;
        Assert.Same(view, item.View);
        Assert.Equal(new object[] { documentContent, toolContent, view }, Read(manager.LogicalChildrenPublic));
        Assert.Same(manager, view.Parent);
        Assert.Same(manager, documentContent.Parent);
    }

    [AvaloniaFact]
    public void Content_Replacement_Does_Not_Change_The_Initially_Registered_Logical_Control()
    {
        Border original = new();
        Border replacement = new();
        LayoutDocument document = new() { Content = original };
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };

        document.Content = replacement;

        Assert.Equal(new object[] { original }, Read(manager.LogicalChildrenPublic));
        Assert.Same(manager, original.Parent);
        Assert.Null(replacement.Parent);
        manager.Layout = new();
        Assert.Empty(Read(manager.LogicalChildrenPublic));
        Assert.Null(original.Parent);
    }

    [AvaloniaFact]
    public void Replaying_Layout_Attachment_Replaces_Presentation_And_Restores_Logical_Membership()
    {
        LogicalManager manager = new();
        Window window = new() { Content = manager };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Control previousPanel = manager.LayoutRootPanel!;
            Control previousSide = manager.LeftSidePanel!;

            manager.ReplayLayout();

            Assert.NotSame(previousPanel, manager.LayoutRootPanel);
            Assert.NotSame(previousSide, manager.LeftSidePanel);
            Assert.Null(previousPanel.Parent);
            Assert.Null(previousSide.Parent);
            Assert.Contains(manager.LayoutRootPanel, Read(manager.LogicalChildrenPublic));
            Assert.Contains(manager.LeftSidePanel, Read(manager.LogicalChildrenPublic));
            Assert.Same(manager, manager.LayoutRootPanel!.Parent);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData("root")]
    [InlineData("left")]
    [InlineData("top")]
    [InlineData("right")]
    [InlineData("bottom")]
    [InlineData("preview")]
    public void Presentation_Membership_Is_Owned_By_The_Base_Property_Hook(string kind)
    {
        LogicalManager manager = new();
        Control first = NewPresentation(kind);
        Control replacement = NewPresentation(kind);
        manager.SetPresentation(kind, first);
        Assert.Contains(first, Read(manager.LogicalChildrenPublic));
        Assert.Same(manager, first.Parent);
        manager.Suppress = true;

        manager.SetPresentation(kind, replacement);

        Assert.Contains(first, Read(manager.LogicalChildrenPublic));
        Assert.DoesNotContain(replacement, Read(manager.LogicalChildrenPublic));
        Assert.Null(replacement.Parent);
        manager.Replay(kind);
        Assert.DoesNotContain(first, Read(manager.LogicalChildrenPublic));
        Assert.Contains(replacement, Read(manager.LogicalChildrenPublic));
        Assert.Null(first.Parent);
        Assert.Same(manager, replacement.Parent);
        manager.Suppress = false;
        manager.SetPresentation(kind, null);
        Assert.Empty(Read(manager.LogicalChildrenPublic));
        Assert.Null(replacement.Parent);
    }

    [AvaloniaFact]
    public void Failed_Logical_Parent_Assignment_Leaves_The_Already_Registered_Child()
    {
        LogicalManager first = new();
        LogicalManager second = new();
        Control child = NewPresentation("left");
        first.SetPresentation("left", child);

        Assert.Throws<InvalidOperationException>(() => second.SetPresentation("left", child));

        Assert.Contains(child, Read(second.LogicalChildrenPublic));
        Assert.Same(first, child.Parent);
        second.SetPresentation("left", null);
        Assert.Empty(Read(second.LogicalChildrenPublic));
        Assert.Same(first, child.Parent);
    }

    [AvaloniaFact]
    public void A_Logical_Override_Supplies_Raw_Public_And_Filtered_Framework_Enumeration()
    {
        Border first = new();
        TextBox second = new();
        LogicalManager manager = new() { ChildrenOverride = new object?[] { null, "raw", first, 42, second } };
        Assert.Equal(manager.ChildrenOverride, Read(manager.LogicalChildrenPublic));
        var children = ((ILogical)manager).LogicalChildren;
        Assert.Equal(2, children.Count);
        Assert.Same(first, children[0]);
        Assert.Same(second, children[1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => children[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => children[2]);
        Assert.Equal(new ILogical[] { first, second }, children);
        Assert.Equal(new Control[] { first, second }, manager.FindLogicalChildren<Control>());
        manager.ChildrenOverride = new object?[] { second };
        Assert.Equal(new ILogical[] { second }, children);
    }

    [AvaloniaFact]
    public void A_Null_Logical_Override_Produces_No_Framework_Children()
    {
        LogicalManager manager = new() { NullEnumeration = true };
        Assert.Null(manager.LogicalChildrenPublic);
        Assert.Empty(manager.GetLogicalChildren());
    }

    [AvaloniaFact]
    public void Framework_Attachment_And_DataContext_Propagation_Use_The_Logical_Override()
    {
        Border child = new();
        LogicalManager manager = new() { ChildrenOverride = new object?[] { "raw", child }, DataContext = "before" };
        ((ISetLogicalParent)child).SetParent(manager);
        int attached = 0;
        child.AttachedToLogicalTree += (_, _) => attached++;
        Window window = new() { Content = manager };
        try
        {
            window.Show();
            Assert.Equal(1, attached);
            Assert.True(((ILogical)child).IsAttachedToLogicalTree);
            Assert.Equal("before", child.DataContext);
            manager.DataContext = "after";
            Assert.Equal("after", child.DataContext);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Failed_Content_Parent_Assignment_Retains_The_Published_Container(bool tool)
    {
        Border content = new();
        ContentControl owner = new() { Content = content };
        LayoutContent model = tool ? new LayoutAnchorable { Content = content } : new LayoutDocument { Content = content };
        ILayoutPanelElement pane = tool ? new LayoutAnchorablePane((LayoutAnchorable)model) : new LayoutDocumentPane((LayoutDocument)model);
        LayoutRoot layout = new() { RootPanel = new(pane) };
        DockingManager manager = new();

        Assert.Throws<InvalidOperationException>(() => manager.Layout = layout);

        LayoutItem item = Assert.IsAssignableFrom<LayoutItem>(manager.GetLayoutItemFromModel(model));
        Assert.Same(model, item.LayoutElement);
        Assert.Same(content, item.Model);
        Assert.Contains(content, Read(manager.LogicalChildrenPublic));
        Assert.Same(owner, content.Parent);
        manager.Layout = new();
        Assert.Null(item.LayoutElement);
        Assert.Same(owner, content.Parent);
    }

    [AvaloniaFact]
    public void Weak_Membership_Preserves_Expired_Public_Entries_Without_Exposing_Null_Framework_Children()
    {
        var (manager, weak) = CreateExpiredContent();
        for (int i = 0; i < 8 && weak.IsAlive; i++)
        {
            Dispatcher.UIThread.RunJobs();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        }
        Assert.False(weak.IsAlive);
        Assert.Null(Assert.Single(Read(manager.LogicalChildrenPublic)));
        Assert.Empty(manager.GetLogicalChildren());
        GC.KeepAlive(manager);
    }

    [AvaloniaFact]
    public void Document_Panes_Expose_An_Empty_Logical_Enumeration()
    {
        XamlFixture window = new();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            LayoutDocumentPaneControl pane = Assert.Single(manager.FindVisualChildren<LayoutDocumentPaneControl>());
            Assert.Empty(pane.GetLogicalChildren());
            Assert.NotEmpty(manager.FindLogicalChildren<Control>());
        }
        finally { window.Close(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DockingManager, WeakReference) CreateExpiredContent()
    {
        Border content = new();
        LayoutDocument document = new() { Content = content };
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        document.Content = new object();
        return (manager, new(content));
    }

    private static object?[] Read(IEnumerator children)
    {
        List<object?> result = new();
        try { while (children.MoveNext()) result.Add(children.Current); }
        finally { (children as IDisposable)?.Dispose(); }
        return result.ToArray();
    }

    private static Control NewPresentation(string kind)
    {
        LogicalManager source = new();
        source.OnApplyTemplate();
        Control result = source.GetPresentation(kind)!;
        source.SetPresentation(kind, null);
        return result;
    }

    private sealed class LogicalManager : DockingManager
    {
        private readonly Dictionary<string, AvaloniaPropertyChangedEventArgs> _changes = new();
        public bool Suppress { get; set; }
        public object?[]? ChildrenOverride { get; set; }
        public bool NullEnumeration { get; set; }
        protected override IEnumerator LogicalChildren => NullEnumeration ? null! : ChildrenOverride?.GetEnumerator() ?? base.LogicalChildren;

        public Control? GetPresentation(string kind) => kind switch
        {
            "root" => LayoutRootPanel, "left" => LeftSidePanel, "top" => TopSidePanel,
            "right" => RightSidePanel, "bottom" => BottomSidePanel, _ => AutoHideWindow
        };

        public void SetPresentation(string kind, Control? control)
        {
            switch (kind)
            {
                case "root": LayoutRootPanel = (LayoutPanelControl?)control; break;
                case "left": LeftSidePanel = (LayoutAnchorSideControl?)control; break;
                case "top": TopSidePanel = (LayoutAnchorSideControl?)control; break;
                case "right": RightSidePanel = (LayoutAnchorSideControl?)control; break;
                case "bottom": BottomSidePanel = (LayoutAnchorSideControl?)control; break;
                default: SetAutoHideWindow((LayoutAutoHideWindowControl?)control); break;
            }
        }

        public void ReplayLayout() => base.OnLayoutChanged(Layout, Layout);

        public void Replay(string kind)
        {
            AvaloniaPropertyChangedEventArgs change = _changes[kind];
            switch (kind)
            {
                case "root": base.OnLayoutRootPanelChanged(change); break;
                case "left": base.OnLeftSidePanelChanged(change); break;
                case "top": base.OnTopSidePanelChanged(change); break;
                case "right": base.OnRightSidePanelChanged(change); break;
                case "bottom": base.OnBottomSidePanelChanged(change); break;
                default: base.OnAutoHideWindowChanged(change); break;
            }
        }

        protected override void OnLayoutRootPanelChanged(AvaloniaPropertyChangedEventArgs e) { _changes["root"] = e; if (!Suppress) base.OnLayoutRootPanelChanged(e); }
        protected override void OnLeftSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { _changes["left"] = e; if (!Suppress) base.OnLeftSidePanelChanged(e); }
        protected override void OnTopSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { _changes["top"] = e; if (!Suppress) base.OnTopSidePanelChanged(e); }
        protected override void OnRightSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { _changes["right"] = e; if (!Suppress) base.OnRightSidePanelChanged(e); }
        protected override void OnBottomSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { _changes["bottom"] = e; if (!Suppress) base.OnBottomSidePanelChanged(e); }
        protected override void OnAutoHideWindowChanged(AvaloniaPropertyChangedEventArgs e) { _changes["preview"] = e; if (!Suppress) base.OnAutoHideWindowChanged(e); }
    }
}
