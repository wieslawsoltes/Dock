using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DockingEventTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Model_And_Container_Operations_Notify_Once_Around_Native_Movement(bool commands)
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        LayoutDocument document = workspace.First;
        if (commands) workspace.Manager.GetLayoutItemFromModel(document)!.FloatCommand!.Execute(null);
        else document.Float();
        Assert.Equal(new[] { "PreviewFloat", "Floated" }, events.Select(item => item.Name));
        Assert.All(events, item => Assert.Same(document, item.Content));
        Assert.Same(workspace.Documents, events[0].Parent);
        Assert.False(events[0].Floating);
        Assert.True(events[1].Floating);
        Assert.Same(document.Parent, events[1].Parent);
        Assert.Same(document, Assert.IsType<LayoutDocumentFloatingWindow>(events[1].Parent).RootDocument);

        events.Clear();
        document.Dock();
        Assert.Equal(new[] { "PreviewDock", "Docked" }, events.Select(item => item.Name));
        Assert.True(events[0].Floating);
        Assert.False(events[1].Floating);
        Assert.Same(workspace.Documents, events[1].Parent);
        Assert.Equal(new[] { "PreviewFloat", "Floated", "PreviewDock", "Docked" }, workspace.Manager.Hooks);
    }

    [AvaloniaFact]
    public void Handled_Notifications_Bubble_From_The_Manager_Without_Canceling_Operations()
    {
        using Workspace workspace = new();
        int normal = 0;
        int handled = 0;
        EventHandler<RoutedEventArgs> markHandled = (_, e) => e.Handled = true;
        workspace.Manager.PreviewFloat += markHandled;
        workspace.Window.AddHandler(DockingManager.PreviewFloatEvent, (_, _) => normal++);
        workspace.Window.AddHandler(DockingManager.PreviewFloatEvent, (sender, e) =>
        {
            Assert.Same(workspace.Window, sender);
            Assert.IsAssignableFrom<LayoutContent>(e.Source);
            Assert.Equal(ReferenceEquals(e.Source, workspace.First), e.Handled);
            handled++;
        }, RoutingStrategies.Bubble, handledEventsToo: true);
        workspace.First.Float();
        Assert.True(workspace.First.IsFloating);
        Assert.Equal(0, normal);
        Assert.Equal(1, handled);
        workspace.Manager.PreviewFloat -= markHandled;
        workspace.Second.Float();
        Assert.Equal(1, normal);
        Assert.Equal(2, handled);
    }

    [AvaloniaFact]
    public void Dock_Events_Bubble_And_Can_Be_Unsubscribed()
    {
        using Workspace workspace = new();
        int managerCalls = 0;
        int parentCalls = 0;
        EventHandler<RoutedEventArgs> handler = (_, _) => managerCalls++;
        workspace.Manager.PreviewDock += handler;
        workspace.Manager.Docked += handler;
        workspace.Window.AddHandler(DockingManager.DockedEvent, (_, e) =>
        {
            Assert.IsAssignableFrom<LayoutContent>(e.Source);
            parentCalls++;
        });
        workspace.First.Float();
        workspace.First.Dock();
        Assert.Equal(2, managerCalls);
        workspace.Manager.PreviewDock -= handler;
        workspace.Manager.Docked -= handler;
        workspace.First.Float();
        workspace.First.Dock();
        Assert.Equal(2, managerCalls);
        Assert.Equal(2, parentCalls);
    }

    [AvaloniaFact]
    public void Dock_As_Document_Notifies_At_The_Public_Operation_Boundary()
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        workspace.Tool.DockAsDocument();
        Assert.Equal(new[] { "PreviewDock", "Docked" }, events.Select(item => item.Name));
        Assert.Same(workspace.Tools, events[0].Parent);
        Assert.Same(workspace.Documents, events[1].Parent);
        Assert.All(events, item => Assert.Same(workspace.Tool, item.Content));
        events.Clear();
        workspace.Tool.DockAsDocument();
        Assert.Empty(events);
    }

    [AvaloniaFact]
    public void Floating_An_Auto_Hidden_Tool_Notifies_Before_Unpinning_It()
    {
        using Workspace workspace = new();
        workspace.Tool.ToggleAutoHide();
        LayoutAnchorGroup group = Assert.IsType<LayoutAnchorGroup>(workspace.Tool.Parent);
        List<Notification> events = Observe(workspace.Manager);
        workspace.Tool.Float();
        Assert.Equal(new[] { "PreviewFloat", "Floated" }, events.Select(item => item.Name));
        Assert.Same(group, events[0].Parent);
        Assert.False(workspace.Tool.IsAutoHidden);
        Assert.True(workspace.Tool.IsFloating);
    }

    [AvaloniaFact]
    public void Native_Drag_Validation_Is_Silent_And_Execution_Uses_The_Same_Float_Events()
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        DockManager native = new(new DockService());
        Assert.True(native.ValidateDockable(workspace.First.DockModel, workspace.Documents.DockModel,
            DragAction.Move, DockOperation.Window, false));
        Assert.Empty(events);
        Assert.True(native.ValidateDockable(workspace.First.DockModel, workspace.Documents.DockModel,
            DragAction.Move, DockOperation.Window, true));
        Assert.Equal(new[] { "PreviewFloat", "Floated" }, events.Select(item => item.Name));
        Assert.Same(workspace.Documents, events[0].Parent);
        Assert.True(events[1].Floating);
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Fill)]
    [InlineData(DockOperation.Right)]
    public void Native_Redocking_Notifies_Before_Any_Interim_Collection_Moves(DockOperation operation)
    {
        using Workspace workspace = new();
        workspace.First.Float();
        ILayoutContainer? floatingParent = workspace.First.Parent;
        List<Notification> events = Observe(workspace.Manager);
        DockManager native = new(new DockService());
        Assert.True(native.ValidateDockable(workspace.First.DockModel, workspace.Documents.DockModel,
            DragAction.Move, operation, true));
        Assert.Equal(new[] { "PreviewDock", "Docked" }, events.Select(item => item.Name));
        Assert.Same(floatingParent, events[0].Parent);
        Assert.True(events[0].Floating);
        Assert.False(events[1].Floating);
        Assert.Same(workspace.Manager.Layout, workspace.First.Root);
        Assert.IsType<LayoutDocumentPane>(workspace.First.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_A_Pane_Notifies_Once_For_Its_Selected_Content(bool all)
    {
        using Workspace workspace = new();
        workspace.Tools.SelectedContentIndex = 1;
        workspace.OtherTool.IsActive = true;
        List<Notification> events = Observe(workspace.Manager);
        if (all) workspace.Manager.DockFactory.FloatAllDockables(workspace.OtherTool.DockModel);
        else workspace.Manager.DockFactory.FloatDockable(workspace.Tools.DockModel);
        Assert.Equal(new[] { "PreviewFloat", "Floated" }, events.Select(item => item.Name));
        Assert.All(events, item => Assert.Same(workspace.OtherTool, item.Content));
        Assert.True(workspace.Tool.IsFloating);
        Assert.True(workspace.OtherTool.IsFloating);
        Assert.Same(workspace.OtherTool, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Native_Group_Redocking_Reports_One_Pair_For_The_First_Content()
    {
        using Workspace workspace = new();
        LayoutAnchorablePane target = new(new LayoutAnchorable { Title = "Target" });
        workspace.Manager.Layout.RootPanel.Children.Add(target);
        workspace.Manager.DockFactory.FloatAllDockables(workspace.Tool.DockModel);
        LayoutAnchorablePane floatingPane = (LayoutAnchorablePane)workspace.Tool.Parent!;
        List<Notification> events = Observe(workspace.Manager);
        DockManager native = new(new DockService());
        Assert.True(native.ValidateDockable(floatingPane.DockModel, target.DockModel,
            DragAction.Move, DockOperation.Fill, true));
        Assert.Equal(new[] { "PreviewDock", "Docked" }, events.Select(item => item.Name));
        Assert.All(events, item => Assert.Same(workspace.Tool, item.Content));
        Assert.Same(floatingPane, events[0].Parent);
        Assert.Same(target, events[1].Parent);
        Assert.Same(target, workspace.OtherTool.Parent);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Null(workspace.Tool.PreviousContainer);
        Assert.Null(workspace.OtherTool.PreviousContainer);
    }

    [AvaloniaFact]
    public void A_Layout_Replacement_During_Preview_Does_Not_Send_A_Stale_Completion()
    {
        using Workspace workspace = new();
        LayoutRoot previous = workspace.Manager.Layout;
        List<Notification> events = Observe(workspace.Manager);
        workspace.Manager.PreviewFloat += (_, _) => workspace.Manager.Layout = new LayoutRoot();
        workspace.First.Float();
        Assert.Equal("PreviewFloat", Assert.Single(events).Name);
        previous.FloatingWindows.Clear();
    }

    [AvaloniaFact]
    public void Reusing_A_Previous_Floating_Pane_Does_Not_Report_A_New_Window()
    {
        using Workspace workspace = new();
        workspace.Manager.DockFactory.FloatAllDockables(workspace.Tool.DockModel);
        LayoutFloatingWindow window = workspace.Tool.FindParent<LayoutFloatingWindow>()!;
        workspace.Tool.Dock();
        List<Notification> events = Observe(workspace.Manager);
        workspace.Tool.Float();
        Assert.Same(window, workspace.Tool.FindParent<LayoutFloatingWindow>());
        Assert.Empty(events);
    }

    [AvaloniaFact]
    public void Reordering_Tabs_Does_Not_Raise_Docking_Notifications()
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        workspace.Documents.MoveChild(0, 1);
        workspace.Manager.DockFactory.MoveDockable((IDock)workspace.Documents.DockModel,
            workspace.First.DockModel, workspace.Second.DockModel);
        DockManager native = new(new DockService());
        Assert.True(native.ValidateDockable(workspace.First.DockModel, workspace.Second.DockModel,
            DragAction.Move, DockOperation.Fill, true));
        Assert.Empty(events);
    }

    [AvaloniaFact]
    public void Document_Tab_Group_Commands_Remain_Silent()
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        var item = workspace.Manager.GetLayoutItemFromModel(workspace.First)!;
        Assert.True(item.NewHorizontalTabGroupCommand!.CanExecute(null));
        item.NewHorizontalTabGroupCommand.Execute(null);
        Assert.NotSame(workspace.Documents, workspace.First.Parent);
        Assert.True(item.MoveToPreviousTabGroupCommand!.CanExecute(null));
        item.MoveToPreviousTabGroupCommand.Execute(null);
        Assert.Same(workspace.Documents, workspace.First.Parent);
        Assert.Empty(events);
    }

    [AvaloniaFact]
    public void Capability_Rejection_And_Unexecuted_Operations_Do_Not_Report_Completion()
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        workspace.First.CanFloat = false;
        workspace.First.Float();
        DockManager native = new(new DockService());
        Assert.False(native.ValidateDockable(workspace.First.DockModel, workspace.Documents.DockModel,
            DragAction.Move, DockOperation.Window, true));
        Assert.Empty(events);
        workspace.OtherTool.CanFloat = false;
        workspace.Manager.DockFactory.FloatAllDockables(workspace.Tool.DockModel);
        Assert.Equal("PreviewFloat", Assert.Single(events).Name);
        Assert.False(workspace.Tool.IsFloating);
        workspace.OtherTool.CanFloat = true;
        events.Clear();
        workspace.Manager.DockFactory.FloatAllDockables(workspace.Tool.DockModel);
        Assert.Equal(new[] { "PreviewFloat", "Floated" }, events.Select(item => item.Name));
    }

    [AvaloniaFact]
    public void A_Handler_Can_Float_Another_Document_Without_Losing_Its_Notifications()
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        workspace.Manager.PreviewFloat += (_, e) =>
        {
            if (ReferenceEquals(e.Source, workspace.First)) workspace.Second.Float();
        };
        workspace.First.Float();
        Assert.Equal(new[] { "PreviewFloat", "PreviewFloat", "Floated", "Floated" }, events.Select(item => item.Name));
        Assert.Equal(new LayoutContent[] { workspace.First, workspace.Second, workspace.Second, workspace.First },
            events.Select(item => item.Content));
    }

    [AvaloniaFact]
    public void Throwing_Preview_Handlers_Release_The_Operation_Scope()
    {
        using Workspace workspace = new();
        List<Notification> events = Observe(workspace.Manager);
        EventHandler<RoutedEventArgs> handler = (_, _) => throw new InvalidOperationException("Application preview failed.");
        workspace.Manager.PreviewFloat += handler;
        Assert.Throws<InvalidOperationException>(() => workspace.First.Float());
        Assert.Same(workspace.Documents, workspace.First.Parent);
        Assert.Equal("PreviewFloat", Assert.Single(events).Name);
        workspace.Manager.PreviewFloat -= handler;
        events.Clear();
        workspace.First.Float();
        Assert.Equal(new[] { "PreviewFloat", "Floated" }, events.Select(item => item.Name));
    }

    [AvaloniaFact]
    public void Replaced_Layouts_Do_Not_Send_Events_Through_The_Old_Manager()
    {
        using Workspace workspace = new();
        LayoutRoot previous = workspace.Manager.Layout;
        List<Notification> events = Observe(workspace.Manager);
        workspace.Manager.Layout = new LayoutRoot();
        Assert.Throws<NullReferenceException>(workspace.First.Float);
        Assert.Empty(events);
        previous.FloatingWindows.Clear();
    }

    [AvaloniaFact]
    public void Disposed_Observer_Scopes_Do_Not_Retain_The_Manager_Or_Content()
    {
        (IDockOperationScope scope, WeakReference manager, WeakReference content) = CreateDisposedScope();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(manager.IsAlive);
        Assert.False(content.IsAlive);
        scope.Complete();
        scope.Dispose();
        GC.KeepAlive(scope);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (IDockOperationScope, WeakReference, WeakReference) CreateDisposedScope()
    {
        DockingManager manager = new();
        LayoutDocument document = new() { Title = "Retained scope" };
        LayoutDocumentPane pane = manager.Layout.Descendents().OfType<LayoutDocumentPane>().Single();
        pane.Children.Add(document);
        IDockOperationScope scope = ((IDockOperationObserver)manager.DockFactory)
            .BeginDockOperation(document.DockModel, pane.DockModel, DockOperation.Fill)!;
        scope.Dispose();
        return (scope, new(manager), new(document));
    }

    private static List<Notification> Observe(DockingManager manager)
    {
        List<Notification> result = new();
        void Record(object? sender, RoutedEventArgs e)
        {
            Assert.Same(manager, sender);
            LayoutContent content = Assert.IsAssignableFrom<LayoutContent>(e.Source);
            result.Add(new(e.RoutedEvent!.Name, content, content.Parent, content.IsFloating));
        }
        manager.PreviewFloat += Record;
        manager.Floated += Record;
        manager.PreviewDock += Record;
        manager.Docked += Record;
        return result;
    }

    private sealed record Notification(string Name, LayoutContent Content, ILayoutContainer? Parent, bool Floating);

    private sealed class TrackingManager : DockingManager
    {
        public List<string> Hooks { get; } = new();
        protected override void RaisePreviewFloatEvent(LayoutContent content)
        { Hooks.Add("PreviewFloat"); base.RaisePreviewFloatEvent(content); }
        protected override void RaiseFloatedEvent(LayoutContent content)
        { Hooks.Add("Floated"); base.RaiseFloatedEvent(content); }
        protected override void RaisePreviewDockEvent(LayoutContent content)
        { Hooks.Add("PreviewDock"); base.RaisePreviewDockEvent(content); }
        protected override void RaiseDockedEvent(LayoutContent content)
        { Hooks.Add("Docked"); base.RaiseDockedEvent(content); }
    }

    private sealed class Workspace : IDisposable
    {
        public TrackingManager Manager { get; } = new();
        public Window Window { get; }
        public LayoutDocument First { get; } = new() { Title = "First", ContentId = "first" };
        public LayoutDocument Second { get; } = new() { Title = "Second", ContentId = "second" };
        public LayoutAnchorable Tool { get; } = new() { Title = "Tool", ContentId = "tool" };
        public LayoutAnchorable OtherTool { get; } = new() { Title = "Other", ContentId = "other" };
        public LayoutDocumentPane Documents { get; }
        public LayoutAnchorablePane Tools { get; }

        public Workspace()
        {
            Documents = new(First);
            Documents.Children.Add(Second);
            Tools = new(Tool);
            Tools.Children.Add(OtherTool);
            Manager.Layout.RootPanel = new LayoutPanel(Documents);
            Manager.Layout.RootPanel.Children.Add(Tools);
            Window = new() { Content = Manager, Width = 900, Height = 600 };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
