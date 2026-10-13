using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutOperationContractTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Explicit_And_Direct_Command_Close_Ignore_Capabilities_Without_Changing_Them_Or_Reentrant_Native_Calls(bool tools, bool directCommand)
    {
        DockingManager manager = CreateManager(out LayoutDocument document, out LayoutAnchorable tool);
        LayoutContent content = tools ? tool : document;
        LayoutContent other = tools ? document : tool;
        content.CanClose = other.CanClose = false;
        IDock owner = (IDock)content.DockModel.Owner!;
        owner.CanCloseLastDockable = false;
        owner.DockCapabilityPolicy = new DockCapabilityPolicy { CanClose = false };
        List<string> events = new();
        manager.DocumentClosing += (_, _) => events.Add("manager:closing");
        manager.DocumentClosed += (_, _) => events.Add("manager:closed");
        content.Closing += (_, _) =>
        {
            events.Add("content:closing");
            Assert.False(content.CanClose);
            Assert.False(owner.CanCloseLastDockable);
            manager.DockFactory.CloseDockable(other.DockModel);
            Assert.Same(manager.Layout, other.Root);
        };
        content.Closed += (_, _) =>
        {
            Assert.Null(content.Parent);
            events.Add("content:closed");
        };
        manager.DockFactory.DockableClosing += (_, _) => events.Add("native:closing");
        manager.DockFactory.DockableClosed += (_, _) => events.Add("native:closed");
        ICommand command = manager.GetLayoutItemFromModel(content)!.CloseCommand!;
        Assert.False(command.CanExecute(null));
        manager.DockFactory.CloseDockable(content.DockModel);
        Assert.Empty(events);

        if (directCommand) command.Execute(null);
        else content.Close();

        Assert.Null(content.Root);
        Assert.False(content.CanClose);
        Assert.False(owner.CanCloseLastDockable);
        Assert.False(owner.DockCapabilityPolicy.CanClose);
        Assert.Equal(tools
            ? new[] { "content:closing", "native:closing", "content:closed", "native:closed" }
            : new[] { "manager:closing", "content:closing", "native:closing", "content:closed", "manager:closed", "native:closed" }, events);
    }

    [AvaloniaTheory]
    [InlineData(false, "manager")]
    [InlineData(false, "content")]
    [InlineData(false, "native")]
    [InlineData(true, "content")]
    [InlineData(true, "native")]
    public void Explicit_Close_Still_Honors_All_Cancellation_Layers(bool tools, string vetoLayer)
    {
        DockingManager manager = CreateManager(out LayoutDocument document, out LayoutAnchorable tool);
        LayoutContent content = tools ? tool : document;
        content.CanClose = false;
        bool cancel = true;
        int closed = 0;
        manager.DocumentClosing += (_, args) => args.Cancel = cancel && vetoLayer == "manager";
        content.Closing += (_, args) => args.Cancel = cancel && vetoLayer == "content";
        manager.DockFactory.DockableClosing += (_, args) => args.Cancel |= cancel && vetoLayer == "native";
        content.Closed += (_, _) => closed++;

        content.Close();
        Assert.Same(manager.Layout, content.Root);
        Assert.Equal(0, closed);
        cancel = false;
        content.Close();
        Assert.Null(content.Root);
        Assert.Equal(1, closed);
        Assert.False(content.CanClose);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left, false, false)]
    [InlineData(AnchorSide.Top, false, false)]
    [InlineData(AnchorSide.Right, false, false)]
    [InlineData(AnchorSide.Bottom, false, false)]
    [InlineData(AnchorSide.Left, true, false)]
    [InlineData(AnchorSide.Top, true, false)]
    [InlineData(AnchorSide.Right, true, false)]
    [InlineData(AnchorSide.Bottom, true, false)]
    [InlineData(AnchorSide.Left, false, true)]
    [InlineData(AnchorSide.Top, false, true)]
    [InlineData(AnchorSide.Right, false, true)]
    [InlineData(AnchorSide.Bottom, false, true)]
    [InlineData(AnchorSide.Left, true, true)]
    [InlineData(AnchorSide.Top, true, true)]
    [InlineData(AnchorSide.Right, true, true)]
    [InlineData(AnchorSide.Bottom, true, true)]
    public void Explicit_And_Direct_Command_AutoHide_Preserve_Native_Identity_While_Command_Availability_Respects_Capabilities(AnchorSide side, bool policy, bool directCommand)
    {
        DockingManager manager = new();
        LayoutAnchorable tool = new() { CanAutoHide = policy };
        tool.AddToLayout(manager, side switch
        {
            AnchorSide.Left => AnchorableShowStrategy.Left,
            AnchorSide.Right => AnchorableShowStrategy.Right,
            AnchorSide.Top => AnchorableShowStrategy.Top,
            _ => AnchorableShowStrategy.Bottom
        });
        LayoutAnchorablePane pane = (LayoutAnchorablePane)tool.Parent!;
        LayoutAnchorable neighbor = new();
        pane.Children.Add(neighbor);
        IDock nativePane = (IDock)pane.DockModel;
        nativePane.DockCapabilityPolicy = new DockCapabilityPolicy { CanPin = policy ? false : null };
        LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
        List<string> events = new();
        manager.DockFactory.DockablePinned += (_, args) => { Assert.Same(tool.DockModel, args.Dockable); events.Add("pin"); };
        manager.DockFactory.DockableUnpinned += (_, args) => { Assert.Same(tool.DockModel, args.Dockable); events.Add("unpin"); };
        Assert.False(item.AutoHideCommand!.CanExecute(null));
        manager.DockFactory.PinDockable(tool.DockModel);
        Assert.Same(pane, tool.Parent);
        Assert.Empty(events);

        if (directCommand) item.AutoHideCommand.Execute(null);
        else tool.ToggleAutoHide();
        LayoutAnchorGroup group = Assert.IsType<LayoutAnchorGroup>(tool.Parent);
        Assert.Equal(side, Assert.IsType<LayoutAnchorSide>(group.Parent).Side);
        Assert.Same(pane, tool.PreviousContainer);
        Assert.Same(neighbor, Assert.Single(pane.Children));
        Assert.True(manager.DockFactory.IsDockablePinned(tool.DockModel));
        manager.DockFactory.UnpinDockable(tool.DockModel);
        Assert.Same(group, tool.Parent);
        if (directCommand) item.AutoHideCommand.Execute(null);
        else tool.ToggleAutoHide();

        Assert.Same(pane, tool.Parent);
        Assert.Equal(new[] { neighbor, tool }, pane.Children);
        Assert.Same(tool, pane.SelectedContent);
        Assert.False(manager.DockFactory.IsDockablePinned(tool.DockModel));
        Assert.Equal(new[] { "pin", "unpin" }, events);
        Assert.Equal(policy, tool.CanAutoHide);
        Assert.Equal(policy ? false : (bool?)null, nativePane.DockCapabilityPolicy.CanPin);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_AutoHide_Does_Not_Move_Hidden_Or_Document_Hosted_Tools(bool hidden)
    {
        DockingManager manager = CreateManager(out _, out LayoutAnchorable tool);
        if (hidden) tool.Hide();
        else tool.DockAsDocument();
        ILayoutContainer? parent = tool.Parent;
        ILayoutContainer? previous = tool.PreviousContainer;

        tool.ToggleAutoHide();

        Assert.Same(parent, tool.Parent);
        Assert.Same(previous, tool.PreviousContainer);
        Assert.False(tool.IsAutoHidden);
        Assert.False(manager.DockFactory.IsDockablePinned(tool.DockModel));
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Explicit_And_Direct_Command_DockAsDocument_Ignore_Command_Capability_And_Select_The_Native_Destination(bool policy, bool directCommand)
    {
        DockingManager manager = CreateManager(out LayoutDocument document, out LayoutAnchorable tool);
        LayoutDocumentPane destination = (LayoutDocumentPane)document.Parent!;
        document.IsActive = true;
        tool.CanDockAsTabbedDocument = policy;
        ((IDock)tool.DockModel.Owner!).DockCapabilityPolicy = new DockCapabilityPolicy { CanDockAsDocument = policy ? false : null };
        ICommand command = manager.GetLayoutItemFromModel(tool)!.DockAsDocumentCommand!;
        List<string> events = new();
        manager.PreviewDock += (_, _) => events.Add("preview");
        manager.Docked += (_, _) => events.Add("docked");
        Assert.False(command.CanExecute(null));
        manager.DockFactory.DockAsDocument(tool.DockModel);
        Assert.IsType<LayoutAnchorablePane>(tool.Parent);
        Assert.Empty(events);

        if (directCommand) command.Execute(null);
        else tool.DockAsDocument();

        Assert.Same(destination, tool.Parent);
        Assert.Same(destination.DockModel, tool.DockModel.Owner);
        Assert.Same(tool, destination.SelectedContent);
        Assert.Same(tool, manager.Layout.ActiveContent);
        Assert.Equal(new[] { "preview", "docked" }, events);
        Assert.Equal(policy, tool.CanDockAsTabbedDocument);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Denied_New_Float_Still_Selects_And_Activates_The_Requested_Content(bool tools)
    {
        DockingManager manager = CreateManager(out LayoutDocument document, out LayoutAnchorable tool);
        LayoutContent content = tools ? tool : document;
        (tools ? (LayoutContent)document : tool).IsActive = true;
        ILayoutContainer? parent = content.Parent;
        content.CanFloat = false;
        int windows = 0;
        manager.PreviewFloat += (_, _) => windows++;

        content.Float();

        Assert.Same(parent, content.Parent);
        Assert.False(content.IsFloating);
        Assert.True(content.IsSelected);
        Assert.Same(content, manager.Layout.ActiveContent);
        Assert.Empty(manager.Layout.FloatingWindows);
        Assert.Equal(0, windows);
        Assert.False(content.CanFloat);
    }

    [AvaloniaFact]
    public void Explicit_Float_Reuses_Its_Previous_Floating_Pane_When_CanFloat_Is_False()
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool;
        LayoutAnchorablePane original = (LayoutAnchorablePane)tool.Parent!;
        original.Children.Add(new LayoutAnchorable());
        workspace.Manager.DockFactory.FloatAllDockables(tool.DockModel);
        LayoutAnchorablePane floatingPane = (LayoutAnchorablePane)tool.Parent!;
        LayoutFloatingWindow window = tool.FindParent<LayoutFloatingWindow>()!;
        tool.Dock();
        tool.CanFloat = false;
        int notifications = 0;
        workspace.Manager.PreviewFloat += (_, _) => notifications++;
        workspace.Manager.PreviewDock += (_, _) => notifications++;
        Assert.False(workspace.Manager.GetLayoutItemFromModel(tool)!.FloatCommand!.CanExecute(null));
        workspace.Manager.DockFactory.FloatDockable(tool.DockModel);
        Assert.Same(original, tool.Parent);

        tool.Float();

        Assert.Same(floatingPane, tool.Parent);
        Assert.Same(window, tool.FindParent<LayoutFloatingWindow>());
        Assert.Same(tool, floatingPane.Children[0]);
        Assert.Same(original, tool.PreviousContainer);
        Assert.True(tool.IsSelected);
        Assert.True(tool.IsActive);
        Assert.False(tool.CanFloat);
        Assert.Equal(0, notifications);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Float_Of_Already_Floating_Content_Creates_A_New_Window_And_Keeps_The_Docked_Return_Pane(bool tools)
    {
        using Workspace workspace = new();
        LayoutContent content = tools ? workspace.Tool : workspace.Document;
        ILayoutContainer? dockedPane = content.Parent;
        content.Float();
        LayoutFloatingWindow first = content.FindParent<LayoutFloatingWindow>()!;
        content.Float();
        LayoutFloatingWindow second = content.FindParent<LayoutFloatingWindow>()!;
        Dispatcher.UIThread.RunJobs();

        Assert.NotSame(first, second);
        Assert.Same(dockedPane, content.PreviousContainer);
        Assert.Same(second, Assert.Single(workspace.Manager.Layout.FloatingWindows));
        content.Dock();
        Dispatcher.UIThread.RunJobs();
        Assert.Same(dockedPane, content.Parent);
        if (tools)
        {
            Assert.Same(second, Assert.Single(workspace.Manager.Layout.FloatingWindows));
            Assert.False(Assert.IsType<LayoutAnchorableFloatingWindow>(second).IsVisible);
            Assert.Same(second, content.PreviousContainer!.FindParent<LayoutFloatingWindow>());
        }
        else Assert.Empty(workspace.Manager.Layout.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData("hidden")]
    [InlineData("auto-hidden")]
    [InlineData("floating")]
    public void Explicit_Close_Removes_Restricted_Tools_From_All_Native_Locations(string location)
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool;
        tool.CanClose = false;
        if (location == "hidden") tool.Hide();
        else if (location == "auto-hidden") tool.ToggleAutoHide();
        else tool.Float();
        tool.CanAutoHide = false;

        tool.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.Null(tool.Root);
        Assert.DoesNotContain(tool, workspace.Manager.Layout.Descendents());
        Assert.False(workspace.Manager.DockFactory.IsDockablePinned(tool.DockModel));
        Assert.Empty(workspace.Manager.Layout.Hidden);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData("root")]
    [InlineData("pane")]
    [InlineData("content")]
    public void Default_Commands_Refresh_When_Native_Policy_Is_Replaced(string policyScope)
    {
        DockingManager manager = CreateManager(out _, out LayoutAnchorable tool);
        tool.CanClose = true;
        LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
        ICommand[] commands = { item.CloseCommand!, item.AutoHideCommand!, item.FloatCommand!, item.DockAsDocumentCommand! };
        Assert.All(commands, command => Assert.True(command.CanExecute(null)));
        DockCapabilityOverrides denied = new() { CanClose = false, CanPin = false, CanFloat = false, CanDockAsDocument = false };
        SetPolicy(denied);
        Assert.All(commands, command => Assert.False(command.CanExecute(null)));
        SetPolicy(null);
        Assert.All(commands, command => Assert.True(command.CanExecute(null)));

        void SetPolicy(DockCapabilityOverrides? policy)
        {
            if (policyScope == "root") ((IRootDock)manager.Layout.DockModel).RootDockCapabilityPolicy = policy;
            else if (policyScope == "pane") ((IDock)tool.DockModel.Owner!).DockCapabilityPolicy = policy;
            else tool.DockModel.DockCapabilityOverrides = policy;
        }
    }

    [AvaloniaFact]
    public void Close_Command_Rechecks_Last_Child_And_Policy_After_Earlier_Close_Callbacks()
    {
        DockingManager manager = CreateManager(out LayoutDocument document, out _);
        LayoutDocumentPane pane = (LayoutDocumentPane)document.Parent!;
        LayoutDocument next = new();
        pane.Children.Add(next);
        ((IDock)pane.DockModel).CanCloseLastDockable = false;
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        ICommand nextClose = manager.GetLayoutItemFromModel(next)!.CloseCommand!;
        Assert.True(nextClose.CanExecute(null));
        item.CloseAllCommand!.Execute(null);
        Assert.Same(next, Assert.Single(pane.Children));
        Assert.False(nextClose.CanExecute(null));
        ((IDock)pane.DockModel).CanCloseLastDockable = true;
        Assert.True(nextClose.CanExecute(null));
        nextClose.Execute(null);
        Assert.Null(next.Root);
    }

    private static DockingManager CreateManager(out LayoutDocument document, out LayoutAnchorable tool)
    {
        document = new LayoutDocument { Title = "Document", ContentId = "document" };
        tool = new LayoutAnchorable { Title = "Tool", ContentId = "tool" };
        return new DockingManager { Layout = new() { RootPanel = new() { Children =
        { new LayoutDocumentPane(document), new LayoutAnchorablePane(tool) } } } };
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; }
        internal LayoutDocument Document { get; }
        internal LayoutAnchorable Tool { get; }
        private readonly Window _window;

        internal Workspace()
        {
            Manager = CreateManager(out LayoutDocument document, out LayoutAnchorable tool);
            Document = document;
            Tool = tool;
            _window = new Window { Content = Manager, Width = 800, Height = 600 };
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
