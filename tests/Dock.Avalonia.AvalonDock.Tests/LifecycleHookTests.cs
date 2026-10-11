using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LifecycleHookTests
{
    [AvaloniaFact]
    public void Manager_Initialization_And_Template_Hooks_Follow_The_Real_Avalonia_Lifecycle()
    {
        ProbeManager manager = new();
        manager.Initialized += (_, _) => manager.Events.Add("event");
        manager.BeginInit(); manager.EndInit();
        Assert.Empty(manager.Events);
        Window view = new() { Width = 800, Height = 500, Content = manager };
        try
        {
            view.Show(); Update(view);
            Assert.Equal(new[] { "initializing", "initialized", "event" }, manager.Events);
            Assert.Equal(1, manager.Initializations);
            Assert.True(manager.Templates > 0);
            LayoutPanelControl? panel = manager.LayoutRootPanel;
            LayoutAnchorSideControl? side = manager.LeftSidePanel;
            LayoutAutoHideWindowControl? preview = manager.AutoHideWindow;
            Assert.NotNull(panel); Assert.NotNull(side); Assert.NotNull(preview);
            int count = manager.Templates;
            manager.Template = new FuncControlTemplate((_, _) => new Border());
            Update(view);
            Assert.Equal(count + 1, manager.Templates);
            manager.OnApplyTemplate();
            Assert.Equal(count + 2, manager.Templates);
            Assert.Same(panel, manager.LayoutRootPanel);
            Assert.Same(side, manager.LeftSidePanel);
            Assert.Same(preview, manager.AutoHideWindow);
            view.Content = null;
            view.Content = manager;
            Update(view);
            Assert.Equal(1, manager.Initializations);
        }
        finally { view.Close(); Update(view); }
    }

    [AvaloniaFact]
    public void Floating_Initialization_Sees_The_Model_And_Runs_Once_Before_Its_Event()
    {
        LayoutDocumentFloatingWindow model = new();
        ProbeWindow host = new(model);
        try
        {
            host.Show(); Update(host);
            Assert.Same(model, host.InitializedModel);
            Assert.Equal(new[] { "initializing", "initialized", "event" }, host.Events);
            host.Hide(); host.Show(); Update(host);
            Assert.Equal(1, host.Initializations);
        }
        finally { host.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Both_Closing_Overloads_Share_The_Native_Arguments_And_Event_Order(bool cancel)
    {
        ProbeWindow host = new(new LayoutDocumentFloatingWindow()) { CancelClose = cancel };
        WindowClosingEventArgs? nativeArgs = null;
        host.Closing += (_, args) => { nativeArgs = args; host.Events.Add("event"); };
        host.Show(); host.Events.Clear();
        try
        {
            host.Close();
            Assert.Equal(new[] { "native-before", "hook-before", "event", "hook-after", "native-after" }, host.Events);
            Assert.Same(nativeArgs, host.ClosingArguments);
            Assert.Equal(WindowCloseReason.WindowClosing, nativeArgs!.CloseReason);
            Assert.True(nativeArgs.IsProgrammatic);
            Assert.Equal(cancel, host.IsVisible);
            Assert.Equal(cancel, host.CanceledAfterBase);
        }
        finally { host.CancelClose = false; host.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Hook_Event_Factory_And_Content_Vetoes_Preserve_The_Floating_Document(int stage)
    {
        using Workspace workspace = new();
        ProbeWindow host = workspace.Float();
        LayoutFloatingWindow model = (LayoutFloatingWindow)host.Model;
        bool veto = true;
        int windowEvents = 0, factoryEvents = 0, contentEvents = 0, closed = 0;
        host.CancelClose = stage == 0;
        host.Closing += (_, args) => { windowEvents++; if (stage == 1 && veto) args.Cancel = true; };
        workspace.Manager.DockFactory.WindowClosing += (_, args) => { factoryEvents++; if (stage == 2 && veto) args.Cancel = true; };
        workspace.Document.Closing += (_, args) => { contentEvents++; if (stage == 3 && veto) args.Cancel = true; };
        workspace.Document.Closed += (_, _) => closed++;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.True(host.CanceledAfterBase);
        Assert.Same(host, model.DockWindow.Host);
        Assert.Same(model, workspace.Document.FindParent<LayoutFloatingWindow>());
        Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Equal(1, windowEvents);
        Assert.Equal(stage < 2 ? 0 : 1, factoryEvents);
        Assert.Equal(stage < 3 ? 0 : 1, contentEvents);
        Assert.Equal(0, closed);
        veto = false; host.CancelClose = false;
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Null(workspace.Document.Root);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Equal(1, closed);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Base_Calls_Invoke_An_Intermediate_Bcl_Override_Exactly_Once(bool cancel)
    {
        NativeDerivedWindow host = new() { CancelClose = cancel };
        int events = 0;
        host.Closing += (_, _) => events++;
        try
        {
            host.Show(); host.Close();
            Assert.Equal(1, host.NativeCalls);
            Assert.Equal(1, host.BclCalls);
            Assert.Equal(1, events);
            Assert.Equal(cancel, host.IsVisible);
        }
        finally { host.CancelClose = false; host.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Bcl_Close_Hook_Can_Veto_And_Retry_Internal_Cleanup(bool removeWindow)
    {
        using Workspace workspace = new();
        ProbeWindow host = workspace.Float();
        LayoutFloatingWindow model = (LayoutFloatingWindow)host.Model;
        host.CancelClose = true;
        if (removeWindow) workspace.Manager.Layout.FloatingWindows.Remove(model);
        else workspace.Document.Dock();
        Assert.True(host.IsVisible);
        Assert.True(host.CanceledAfterBase);
        Assert.False(host.UserCloseOrigin);
        Assert.Same(model, Assert.Single(workspace.Manager.Layout.FloatingWindows));
        host.CancelClose = false;
        workspace.Manager.Layout.FloatingWindows.Remove(model);
        Assert.False(host.IsVisible);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Null(model.Parent);
    }

    [AvaloniaFact]
    public void A_Throwing_Close_Hook_Propagates_Through_Native_Close_Cleanup()
    {
        using Workspace workspace = new();
        ProbeWindow host = workspace.Float();
        host.ThrowOnClose = true;
        Assert.Throws<InvalidOperationException>(() => host.Close());
        Assert.False(host.IsVisible);
        Assert.Null(workspace.Document.Root);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Empty(workspace.Manager.FloatingWindows);
        host.ThrowOnClose = false;
    }

    private static void Update(Window window) { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }

    private sealed class ProbeManager : DockingManager
    {
        public List<string> Events { get; } = new();
        public int Initializations { get; private set; }
        public int Templates { get; private set; }
        protected override void OnInitialized(EventArgs e)
        {
            Initializations++; Events.Add("initializing"); base.OnInitialized(e); Events.Add("initialized");
        }
        public override void OnApplyTemplate() { base.OnApplyTemplate(); Templates++; }
    }

    private class BclDerivedWindow(ILayoutElement model) : LayoutFloatingWindowControl(model)
    {
        public override ILayoutElement Model { get; } = model;
        public int BclCalls { get; private set; }
        public bool CancelClose { get; set; }
        protected override void OnClosing(CancelEventArgs e)
        {
            BclCalls++; e.Cancel |= CancelClose; base.OnClosing(e);
        }
    }

    private sealed class NativeDerivedWindow() : BclDerivedWindow(new LayoutDocumentFloatingWindow())
    {
        public int NativeCalls { get; private set; }
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            NativeCalls++; base.OnClosing(e);
        }
    }

    private sealed class ProbeWindow(ILayoutElement model) : LayoutFloatingWindowControl(model)
    {
        public override ILayoutElement Model { get; } = model;
        public List<string> Events { get; } = new();
        public ILayoutElement? InitializedModel { get; private set; }
        public int Initializations { get; private set; }
        public bool CancelClose { get; set; }
        public bool ThrowOnClose { get; set; }
        public bool CanceledAfterBase { get; private set; }
        public CancelEventArgs? ClosingArguments { get; private set; }
        public bool UserCloseOrigin => CloseInitiatedByUser;
        protected override bool CanClose(object? parameter = null) => true;
        protected override void OnInitialized(EventArgs e)
        {
            InitializedModel = Model; Initializations++;
            Initialized += (_, _) => Events.Add("event");
            Events.Add("initializing"); base.OnInitialized(e); Events.Add("initialized");
        }
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            Events.Add("native-before"); base.OnClosing(e); Events.Add("native-after");
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Events.Add("hook-before"); ClosingArguments = e;
            if (ThrowOnClose) throw new InvalidOperationException("Close hook failed.");
            e.Cancel |= CancelClose;
            base.OnClosing(e);
            CanceledAfterBase = e.Cancel; Events.Add("hook-after");
        }
    }

    private sealed class Workspace : IDisposable
    {
        public DockingManager Manager { get; } = new();
        public LayoutDocument Document { get; } = new() { Title = "Editor" };
        private readonly Window _view;
        public Workspace()
        {
            Manager.Layout = new() { RootPanel = new(new LayoutDocumentPane(Document)) };
            _view = new() { Width = 800, Height = 500, Content = Manager };
            _view.Show(); Update(_view);
            Manager.DockFactory.HostWindowLocator![nameof(IDockWindow)] = () =>
                new ProbeWindow(Manager.Layout.FloatingWindows.Single(model => model.DockWindow.Host is null));
        }
        public ProbeWindow Float()
        {
            Document.Float(); Update(_view);
            return Assert.IsType<ProbeWindow>(Assert.Single(Manager.FloatingWindows));
        }
        public void Dispose()
        {
            foreach (ProbeWindow host in Manager.FloatingWindows.OfType<ProbeWindow>())
            { host.ThrowOnClose = false; host.CancelClose = false; }
            Manager.Layout.FloatingWindows.Clear(); _view.Close(); Dispatcher.UIThread.RunJobs();
        }
    }
}
