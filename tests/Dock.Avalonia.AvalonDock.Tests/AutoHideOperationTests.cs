using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AutoHideOperationTests
{
    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Each_Explicit_Pin_Creates_A_Group_And_Hide_Show_Retains_Its_Position(AnchorSide side)
    {
        DockingManager manager = new();
        LayoutAnchorable first = new() { ContentId = "first", CanAutoHide = false };
        first.AddToLayout(manager, Strategy(side));
        LayoutAnchorablePane pane = (LayoutAnchorablePane)first.Parent!;
        LayoutAnchorable second = new() { ContentId = "second", CanAutoHide = false };
        pane.Children.Add(second);

        first.ToggleAutoHide();
        second.ToggleAutoHide();

        LayoutAnchorSide strip = Side(manager.Layout, side);
        LayoutAnchorGroup firstGroup = Assert.IsType<LayoutAnchorGroup>(first.Parent);
        LayoutAnchorGroup secondGroup = Assert.IsType<LayoutAnchorGroup>(second.Parent);
        Assert.NotSame(firstGroup, secondGroup);
        Assert.Equal(new[] { firstGroup, secondGroup }, strip.Children);
        Assert.Same(first, Assert.Single(firstGroup.Children));
        Assert.Same(second, Assert.Single(secondGroup.Children));
        Assert.Equal(new[] { first.DockModel, second.DockModel }, Pinned(manager.Layout, side));

        first.Hide();
        Assert.True(first.IsHidden);
        Assert.Equal(new[] { firstGroup, secondGroup }, strip.Children);
        Assert.Same(firstGroup, first.PreviousContainer);
        Assert.False(manager.DockFactory.IsDockablePinned(first.DockModel));
        first.Show();
        Assert.Same(firstGroup, first.Parent);
        Assert.Equal(new[] { firstGroup, secondGroup }, strip.Children);
        Assert.Equal(new[] { first.DockModel, second.DockModel }, Pinned(manager.Layout, side));
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Floating_Tool_Uses_The_Main_Strip_Through_Preview_And_Xml_Then_Returns_To_Its_Floating_Pane(bool roundTrip, bool preview)
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool;
        tool.Float();
        LayoutAnchorablePane pane = (LayoutAnchorablePane)tool.Parent!;
        LayoutAnchorableFloatingWindow floating = tool.FindParent<LayoutAnchorableFloatingWindow>()!;
        tool.CanAutoHide = false;
        tool.ToggleAutoHide();
        Assert.True(tool.IsAutoHidden);
        // Upstream retains the floating flag while the tool is in an anchor
        // group. It still remembers a floating pane, but belongs to the main strip.
        Assert.True(tool.IsFloating);
        Assert.Same(workspace.Manager.Layout.RightSide, tool.Parent!.Parent);
        Assert.Same(workspace.Manager.DockLayout, workspace.Manager.DockFactory.FindRoot(tool.DockModel));
        Assert.Contains(tool.DockModel, workspace.Manager.DockLayout.RightPinnedDockables!);
        Assert.DoesNotContain(tool.DockModel, ((IRootDock)floating.DockModel).RightPinnedDockables!);
        Assert.Same(pane, tool.PreviousContainer);
        Assert.Empty(pane.Children);

        if (roundTrip)
        {
            XmlLayoutSerializer serializer = new(workspace.Manager);
            using StringWriter text = new();
            serializer.Serialize(text);
            serializer.Deserialize(new StringReader(text.ToString()));
            tool = workspace.Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
            pane = (LayoutAnchorablePane)tool.PreviousContainer!;
            floating = pane.FindParent<LayoutAnchorableFloatingWindow>()!;
        }
        if (preview)
        {
            for (int cycle = 0; cycle < 2; cycle++)
            {
                workspace.Manager.DockFactory.PreviewPinnedDockable(tool.DockModel);
                Dispatcher.UIThread.RunJobs();
                Assert.Same(tool.DockModel, workspace.Manager.DockLayout.PinnedDock!.ActiveDockable);
                workspace.Manager.DockFactory.HidePreviewingDockables(workspace.Manager.DockLayout);
                Dispatcher.UIThread.RunJobs();
                Assert.Same(workspace.Manager.DockLayout, workspace.Manager.DockFactory.FindRoot(tool.DockModel));
                Assert.True(workspace.Manager.DockFactory.IsDockablePinned(tool.DockModel));
                Assert.Same(pane.DockModel, tool.DockModel.OriginalOwner);
            }
        }
        LayoutDocument active = workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Single();
        active.IsActive = true;
        tool.ToggleAutoHide();
        Dispatcher.UIThread.RunJobs();

        Assert.Same(pane, tool.Parent);
        Assert.Same(floating, tool.FindParent<LayoutAnchorableFloatingWindow>());
        Assert.True(tool.IsFloating);
        Assert.False(tool.IsAutoHidden);
        Assert.Same(tool, pane.SelectedContent);
        // Presenting an existing host restores that native root's focus history.
        // An empty root reconstructed from XML has no focused tool to restore.
        Assert.Same(roundTrip ? (LayoutContent)active : tool, workspace.Manager.Layout.ActiveContent);
        Assert.Empty(workspace.Manager.Layout.RightSide.Children);
        Assert.False(workspace.Manager.DockFactory.IsDockablePinned(tool.DockModel));
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left, Orientation.Horizontal)]
    [InlineData(AnchorSide.Left, Orientation.Vertical)]
    [InlineData(AnchorSide.Top, Orientation.Horizontal)]
    [InlineData(AnchorSide.Top, Orientation.Vertical)]
    [InlineData(AnchorSide.Right, Orientation.Horizontal)]
    [InlineData(AnchorSide.Right, Orientation.Vertical)]
    [InlineData(AnchorSide.Bottom, Orientation.Horizontal)]
    [InlineData(AnchorSide.Bottom, Orientation.Vertical)]
    public void A_Declared_Group_Creates_An_Edge_Pane_With_Upstream_Orientation_And_Minimums(AnchorSide side, Orientation orientation)
    {
        LayoutDocument document = new();
        LayoutAnchorablePane existing = new(new LayoutAnchorable());
        LayoutPanel original = new() { Orientation = orientation, Children = { existing, new LayoutDocumentPane(document) } };
        DockingManager manager = new() { Layout = new() { RootPanel = original } };
        LayoutAnchorable tool = new() { CanAutoHide = false, AutoHideMinWidth = 247, AutoHideMinHeight = 139 };
        LayoutAnchorGroup group = new();
        group.Children.Add(tool);
        Side(manager.Layout, side).Children.Add(group);
        document.IsActive = true;

        tool.ToggleAutoHide();

        LayoutAnchorablePane destination = Assert.IsType<LayoutAnchorablePane>(tool.Parent);
        Orientation expected = side is AnchorSide.Left or AnchorSide.Right ? Orientation.Horizontal : Orientation.Vertical;
        Assert.NotSame(existing, destination);
        Assert.Equal(expected, manager.Layout.RootPanel.Orientation);
        Assert.Same(destination, side is AnchorSide.Left or AnchorSide.Top
            ? manager.Layout.RootPanel.Children[0] : manager.Layout.RootPanel.Children[^1]);
        if (orientation == expected)
        {
            Assert.Same(original, manager.Layout.RootPanel);
            if (expected == Orientation.Horizontal) Assert.Equal(tool.AutoHideMinWidth, destination.DockMinWidth);
            else Assert.Equal(tool.AutoHideMinHeight, destination.DockMinHeight);
        }
        else
        {
            Assert.NotSame(original, manager.Layout.RootPanel);
            Assert.Contains(original, manager.Layout.RootPanel.Children);
            LayoutAnchorablePane defaults = new();
            Assert.Equal(defaults.DockMinWidth, destination.DockMinWidth);
            Assert.Equal(defaults.DockMinHeight, destination.DockMinHeight);
        }
        Assert.Same(tool, destination.SelectedContent);
        if (orientation == expected) Assert.Same(document, manager.Layout.ActiveContent);
        else Assert.Null(manager.Layout.ActiveContent); // Replacing the root panel clears upstream activation.
        Assert.Empty(Side(manager.Layout, side).Children);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Restoring_One_Tool_Redirects_Hidden_Returns_Even_When_The_Group_Still_Has_Children(bool keepSibling)
    {
        DockingManager manager = new();
        LayoutAnchorable hidden = new() { ContentId = "hidden" };
        LayoutAnchorable restored = new() { ContentId = "restored", CanAutoHide = false };
        LayoutAnchorGroup group = new() { Children = { hidden, restored } };
        if (keepSibling) group.Children.Add(new LayoutAnchorable());
        manager.Layout.LeftSide.Children.Add(group);
        hidden.Hide();
        Assert.Same(group, hidden.PreviousContainer);

        restored.ToggleAutoHide();

        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(restored.Parent);
        Assert.Same(pane, hidden.PreviousContainer);
        Assert.Same(pane.DockModel, hidden.DockModel.OriginalOwner);
        Assert.Equal(keepSibling, manager.Layout.LeftSide.Children.Contains(group));
        manager.Layout.CollectGarbage();
        hidden.Show();
        Assert.Same(pane, hidden.Parent);
        Assert.Equal(new[] { hidden, restored }, pane.Children);
        Assert.False(hidden.IsAutoHidden);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Closing_A_Pinned_Tool_Redirects_Hidden_Returns_Only_After_All_Cancellation(bool managed, bool nativeVeto)
    {
        LayoutRoot root = new();
        DockingManager? manager = managed ? new() { Layout = root } : null;
        LayoutAnchorable hidden = new();
        LayoutAnchorable closing = new() { CanClose = false, CanAutoHide = false };
        LayoutAnchorGroup group = new() { Children = { hidden, closing } };
        root.BottomSide.Children.Add(group);
        hidden.Hide();
        bool cancel = true;
        closing.Closing += (_, args) => args.Cancel = cancel && !nativeVeto;
        root.DockFactory.DockableClosing += (_, args) => args.Cancel |= cancel && nativeVeto;

        closing.Close();
        Assert.Same(group, closing.Parent);
        Assert.Same(group, hidden.PreviousContainer);
        Assert.True(root.DockFactory.IsDockablePinned(closing.DockModel));
        cancel = false;
        closing.Close();

        Assert.Null(closing.Parent);
        Assert.Empty(root.BottomSide.Children);
        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(hidden.PreviousContainer);
        Assert.Same(root, pane.Root);
        hidden.Show();
        Assert.Same(pane, hidden.Parent);
        GC.KeepAlive(manager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Hiding_A_Pin_Disabled_Tool_Cleans_Up_The_Preview_And_Restores_Its_Group(bool preview)
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool;
        tool.ToggleAutoHide();
        LayoutAnchorGroup group = (LayoutAnchorGroup)tool.Parent!;
        tool.CanAutoHide = false;
        if (preview) workspace.Manager.DockFactory.PreviewPinnedDockable(tool.DockModel);

        tool.Hide();
        Dispatcher.UIThread.RunJobs();

        Assert.True(tool.IsHidden);
        Assert.False(workspace.Manager.DockFactory.IsDockablePinned(tool.DockModel));
        Assert.Null(workspace.Manager.DockLayout.PinnedDock);
        Assert.Same(tool, Assert.Single(workspace.Manager.Layout.Hidden));
        tool.Show();
        Assert.True(tool.IsAutoHidden);
        Assert.Same(group, tool.Parent);
    }

    [AvaloniaFact]
    public void Nested_Pin_Callbacks_Keep_Their_Own_Public_Groups()
    {
        DockingManager manager = new();
        LayoutAnchorable first = new();
        first.AddToLayout(manager, AnchorableShowStrategy.Left);
        LayoutAnchorable second = new();
        ((LayoutAnchorablePane)first.Parent!).Children.Add(second);
        manager.DockFactory.DockablePinned += (_, args) =>
        {
            if (ReferenceEquals(args.Dockable, first.DockModel)) second.ToggleAutoHide();
        };

        first.ToggleAutoHide();

        Assert.NotSame(first.Parent, second.Parent);
        Assert.Equal(2, manager.Layout.LeftSide.Children.Count);
        Assert.Same(first, Assert.Single(((LayoutAnchorGroup)first.Parent!).Children));
        Assert.Same(second, Assert.Single(((LayoutAnchorGroup)second.Parent!).Children));
    }

    [AvaloniaFact]
    public void Commands_Use_Current_Floating_Ancestors_Rather_Than_The_Retained_Floating_Flag()
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool;
        LayoutAnchorableItem item = (LayoutAnchorableItem)workspace.Manager.GetLayoutItemFromModel(tool)!;
        tool.Float();
        Assert.False(item.FloatCommand!.CanExecute(null));
        Assert.False(item.AutoHideCommand!.CanExecute(null));

        tool.ToggleAutoHide();

        Assert.True(tool.IsFloating);
        Assert.Null(tool.FindParent<LayoutFloatingWindow>());
        Assert.True(item.FloatCommand.CanExecute(null));
        Assert.True(item.AutoHideCommand.CanExecute(null));
        item.AutoHideCommand.Execute(null);
        Assert.NotNull(tool.FindParent<LayoutFloatingWindow>());
        Assert.False(item.FloatCommand.CanExecute(null));
        Assert.False(item.AutoHideCommand.CanExecute(null));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Returning_To_A_Floating_Pane_Is_Passive_Until_A_Host_Activates(bool preview)
    {
        LayoutDocument document = new();
        LayoutRoot root = new() { RootPanel = new(new LayoutDocumentPane(document)) };
        LayoutAnchorable tool = new();
        LayoutAnchorablePane pane = new(tool);
        LayoutAnchorableFloatingWindow floating = new() { RootPanel = new() { Children = { pane } } };
        root.FloatingWindows.Add(floating);
        tool.ToggleAutoHide();
        if (preview)
        {
            root.DockFactory.PreviewPinnedDockable(tool.DockModel);
            root.DockFactory.HidePreviewingDockables((IRootDock)root.DockModel);
        }
        document.IsActive = true;

        tool.ToggleAutoHide();

        Assert.Same(pane, tool.Parent);
        Assert.Same(tool, pane.SelectedContent);
        Assert.Same(document, root.ActiveContent);
        Assert.False(tool.IsActive);
        Assert.Null(floating.DockWindow.Host);
    }

    private static LayoutAnchorSide Side(LayoutRoot root, AnchorSide side) => side switch
    {
        AnchorSide.Left => root.LeftSide,
        AnchorSide.Top => root.TopSide,
        AnchorSide.Right => root.RightSide,
        _ => root.BottomSide
    };

    private static System.Collections.Generic.IList<IDockable> Pinned(LayoutRoot root, AnchorSide side)
    {
        IRootDock model = (IRootDock)root.DockModel;
        return side switch
        {
            AnchorSide.Left => model.LeftPinnedDockables!,
            AnchorSide.Top => model.TopPinnedDockables!,
            AnchorSide.Right => model.RightPinnedDockables!,
            _ => model.BottomPinnedDockables!
        };
    }

    private static AnchorableShowStrategy Strategy(AnchorSide side) => side switch
    {
        AnchorSide.Left => AnchorableShowStrategy.Left,
        AnchorSide.Top => AnchorableShowStrategy.Top,
        AnchorSide.Right => AnchorableShowStrategy.Right,
        _ => AnchorableShowStrategy.Bottom
    };

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; }
        internal LayoutAnchorable Tool { get; }
        private readonly Window _window;

        internal Workspace()
        {
            Tool = new() { ContentId = "tool", Content = new object() };
            Manager = new() { AutoHideWindowClosingTimer = int.MaxValue, Layout = new() { RootPanel = new() { Children =
            { new LayoutDocumentPane(new LayoutDocument { ContentId = "document", Content = new object() }), new LayoutAnchorablePane(Tool) } } } };
            _window = new() { Content = Manager, Width = 900, Height = 700 };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
