using System;
using System.Collections.Specialized;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingClosedHookTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Typed_Model_Observation_Remains_Active_During_Closed(bool tools)
    {
        (IProbe probe, LayoutFloatingWindow model) = Standalone(tools);
        probe.Window.Closed += (_, _) =>
        {
            Assert.True(probe.UserOrigin);
            Empty(model);
            Assert.False(probe.UserOrigin);
        };
        probe.Window.Close();
        Assert.False(probe.UserOrigin);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Successful_Closure_Removes_The_Typed_Model_Subscription(bool tools)
    {
        (IProbe probe, LayoutFloatingWindow model) = Standalone(tools);
        probe.Window.Close();
        Empty(model);
        Assert.True(probe.UserOrigin);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Throwing_Closed_Callback_Stops_The_Derived_Unsubscription(bool tools)
    {
        (IProbe probe, LayoutFloatingWindow model) = Standalone(tools);
        InvalidOperationException failure = new("closed callback");
        bool fail = true;
        probe.Window.Closed += (_, _) => { if (fail) throw failure; };
        try
        {
            Assert.Same(failure, Assert.Throws<InvalidOperationException>(probe.Window.Close));
            Assert.True(probe.UserOrigin);
            Empty(model);
            Assert.False(probe.UserOrigin);
        }
        finally { fail = false; probe.Replay(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Suppressing_The_Closed_Base_Call_Retains_Model_Observation(bool tools)
    {
        (IProbe probe, LayoutFloatingWindow model) = Standalone(tools);
        probe.Suppress = true;
        int closed = 0;
        probe.Window.Closed += (_, _) => closed++;
        probe.Window.Close();
        Assert.Equal(0, closed);
        Empty(model);
        Assert.False(probe.UserOrigin);
        probe.Suppress = false;
        probe.Replay();
        Assert.Equal(1, closed);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_Panes_Are_Collected_Before_Closed_Callbacks(bool tools)
    {
        using Workspace workspace = new(tools);
        LayoutAnchorablePane empty = new();
        workspace.Root.RootPanel.Children.Add(empty);
        Assert.Same(workspace.Root.RootPanel, empty.Parent);
        workspace.Probe.Window.Closed += (_, _) =>
        {
            Assert.Null(empty.Parent);
            Assert.DoesNotContain(workspace.Probe.Window, workspace.Manager.FloatingWindows);
        };
        workspace.Probe.Window.Close();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_Panes_Added_By_Closed_Callbacks_Are_Not_Collected_Again(bool tools)
    {
        using Workspace workspace = new(tools);
        LayoutAnchorablePane empty = new();
        workspace.Probe.Window.Closed += (_, _) => workspace.Root.RootPanel.Children.Add(empty);
        workspace.Probe.Window.Close();
        Assert.Same(workspace.Root.RootPanel, empty.Parent);
        Assert.Contains(empty, workspace.Root.RootPanel.Children);
    }

    [AvaloniaFact]
    public void An_Empty_Document_Window_Is_Removed_Before_Closed_Without_Losing_Native_Notification()
    {
        using Workspace workspace = new(tools: false);
        int nativeClosed = 0;
        workspace.Manager.DockFactory.WindowClosed += (_, e) =>
        {
            if (ReferenceEquals(workspace.Model.DockWindow, e.Window)) nativeClosed++;
        };
        workspace.Probe.Window.Closed += (_, _) =>
        {
            Assert.Null(workspace.Model.Root);
            Assert.DoesNotContain(workspace.Model, workspace.Root.FloatingWindows);
            Assert.Equal(0, nativeClosed);
        };
        Empty(workspace.Model);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, nativeClosed);
        Assert.Null(workspace.Model.DockWindow.Factory);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Collection_Callbacks_Cannot_Redirect_The_Captured_Root_Cleanup(bool tools)
    {
        using Workspace workspace = new(tools);
        LayoutAnchorablePane originalEmpty = new();
        LayoutAnchorablePane redirectedEmpty = new();
        workspace.Root.RootPanel.Children.Add(originalEmpty);
        LayoutRoot redirected = new() { RootPanel = new(redirectedEmpty) };
        workspace.Manager.LayoutFloatingWindowControlCollectionChanged += (_, e) =>
        {
            if (e.CollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
                workspace.Probe.CurrentModel = redirected;
        };
        workspace.Probe.Window.Close();
        Assert.Null(originalEmpty.Parent);
        Assert.Same(redirected.RootPanel, redirectedEmpty.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Failing_Garbage_Collection_Stops_Before_Closed_And_Can_Be_Replayed(bool tools)
    {
        using Workspace workspace = new(tools);
        LayoutAnchorablePane empty = new();
        workspace.Root.RootPanel.Children.Add(empty);
        InvalidOperationException failure = new("collection callback");
        bool fail = true;
        int closed = 0;
        workspace.Root.RootPanel.Children.CollectionChanged += (_, e) =>
        {
            if (fail && e.Action == NotifyCollectionChangedAction.Remove && e.OldItems?.Contains(empty) == true) throw failure;
        };
        workspace.Probe.Window.Closed += (_, _) => closed++;
        try
        {
            Assert.Same(failure, Assert.Throws<InvalidOperationException>(workspace.Probe.Window.Close));
            Assert.Equal(0, closed);
        }
        finally { fail = false; workspace.Probe.Replay(); }
        Assert.Equal(1, closed);
        Assert.Null(empty.Parent);
    }

    [AvaloniaFact]
    public void Tool_Window_Commands_Remain_Available_In_Closed_And_Are_Released_Afterward()
    {
        using Workspace workspace = new(tools: true);
        ToolProbe host = (ToolProbe)workspace.Probe;
        host.Closed += (_, _) =>
        {
            Assert.True(host.CloseWindowCommand.CanExecute(null));
            Assert.True(host.HideWindowCommand.CanExecute(null));
        };
        host.Close();
        Assert.False(host.CloseWindowCommand.CanExecute(null));
        Assert.False(host.HideWindowCommand.CanExecute(null));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Tool_Local_Bindings_Are_Cleared_After_The_Base_Closed_Event(bool fail)
    {
        (IProbe probe, _) = Standalone(tools: true);
        ToolProbe host = (ToolProbe)probe;
        using BehaviorSubject<LayoutItem?> item = new(new LayoutDocumentItem());
        using BehaviorSubject<bool> visible = new(true);
        using IDisposable itemBinding = host.Bind(LayoutAnchorableFloatingWindowControl.SingleContentLayoutItemProperty, item.ToBinding());
        using IDisposable visibilityBinding = host.Bind(Visual.IsVisibleProperty, visible.ToBinding());
        bool throwNow = fail;
        InvalidOperationException failure = new("closed binding callback");
        host.Closed += (_, _) =>
        {
            Assert.True(item.HasObservers);
            Assert.True(visible.HasObservers);
            if (throwNow) throw failure;
        };
        try
        {
            if (fail)
            {
                Assert.Same(failure, Assert.Throws<InvalidOperationException>(host.Close));
                Assert.True(item.HasObservers);
                Assert.True(visible.HasObservers);
                throwNow = false;
                probe.Replay();
            }
            else host.Close();
            Assert.False(item.HasObservers);
            Assert.False(visible.HasObservers);
        }
        finally { throwNow = false; host.Close(); }
    }

    private static (IProbe, LayoutFloatingWindow) Standalone(bool tools)
    {
        LayoutFloatingWindow model = Model(tools);
        IProbe host = Probe(model);
        host.Window.Show();
        return (host, model);
    }
    private static LayoutFloatingWindow Model(bool tools) => tools
        ? new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable { CanClose = true })) }
        : new LayoutDocumentFloatingWindow { RootDocument = new() };
    private static IProbe Probe(LayoutFloatingWindow model) => model is LayoutDocumentFloatingWindow document
        ? new DocumentProbe(document) : new ToolProbe((LayoutAnchorableFloatingWindow)model);
    private static void Empty(LayoutFloatingWindow model)
    {
        if (model is LayoutDocumentFloatingWindow document) document.RootDocument = null;
        else ((LayoutAnchorableFloatingWindow)model).RootPanel = null;
    }
    private interface IProbe
    {
        LayoutFloatingWindowControl Window { get; }
        bool UserOrigin { get; }
        bool Suppress { get; set; }
        ILayoutElement? CurrentModel { get; set; }
        void Replay();
    }
    private sealed class DocumentProbe(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model), IProbe
    {
        LayoutFloatingWindowControl IProbe.Window => this;
        public bool UserOrigin => CloseInitiatedByUser;
        public bool Suppress { get; set; }
        public ILayoutElement? CurrentModel { get; set; }
        public override ILayoutElement Model => CurrentModel ?? base.Model;
        public void Replay() => OnClosed(EventArgs.Empty);
        protected override bool CanClose(object? parameter = null) => true;
        protected override void OnClosed(EventArgs e) { if (!Suppress) base.OnClosed(e); }
    }
    private sealed class ToolProbe(LayoutAnchorableFloatingWindow model) : LayoutAnchorableFloatingWindowControl(model), IProbe
    {
        LayoutFloatingWindowControl IProbe.Window => this;
        public bool UserOrigin => CloseInitiatedByUser;
        public bool Suppress { get; set; }
        public ILayoutElement? CurrentModel { get; set; }
        public override ILayoutElement Model => CurrentModel ?? base.Model;
        public void Replay() => OnClosed(EventArgs.Empty);
        protected override bool CanClose(object? parameter = null) => true;
        protected override void OnClosed(EventArgs e) { if (!Suppress) base.OnClosed(e); }
    }
    private sealed class Workspace : IDisposable
    {
        internal LayoutRoot Root { get; } = new() { RootPanel = new(new LayoutDocumentPane(new LayoutDocument())) };
        internal LayoutFloatingWindow Model { get; }
        internal DockingManager Manager { get; }
        internal IProbe Probe { get; private set; } = null!;
        private readonly Window _view;
        internal Workspace(bool tools)
        {
            Model = FloatingClosedHookTests.Model(tools);
            Manager = new() { Layout = Root };
            _view = new() { Content = Manager, Width = 640, Height = 400 };
            _view.Show();
            Dispatcher.UIThread.RunJobs();
            Manager.DockFactory.HostWindowLocator![nameof(IDockWindow)] = () =>
            {
                Probe = FloatingClosedHookTests.Probe(Model);
                return Probe.Window;
            };
            Root.FloatingWindows.Add(Model);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(Probe.Window, Assert.Single(Manager.FloatingWindows));
        }
        public void Dispose()
        {
            Probe.Suppress = false;
            Probe.CurrentModel = null;
            Probe.Window.Close();
            Root.FloatingWindows.Clear();
            _view.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
