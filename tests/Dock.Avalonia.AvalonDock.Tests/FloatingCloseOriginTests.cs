using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingCloseOriginTests
{
    [AvaloniaFact]
    public void New_Unshown_And_Manually_Closed_Windows_Start_With_User_Origin()
    {
        ProbeWindow host = new(new LayoutDocumentFloatingWindow()) { AllowClose = true };
        Assert.True(host.UserOrigin);
        host.IsTracked = true;
        Assert.True(host.UserOrigin);
        host.IsTracked = false;
        Assert.True(host.UserOrigin);
        host.Show();
        host.Hide();
        Assert.True(host.UserOrigin);
        host.Close();
        Assert.True(host.UserOrigin);
        Assert.True(host.ClosedOrigin);
    }

    [AvaloniaFact]
    public void Canceled_User_Requests_And_Hiding_Do_Not_Mark_An_Internal_Close()
    {
        using Workspace workspace = new(tool: true);
        ProbeWindow host = workspace.Float();
        EventHandler<Dock.Model.Core.Events.WindowClosingEventArgs> veto = (_, args) => args.Cancel = true;
        workspace.Manager.DockFactory.WindowClosing += veto;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.True(host.UserOrigin);
        Assert.Equal(new[] { true }, host.ClosingOrigins);
        workspace.Manager.DockFactory.WindowClosing -= veto;

        workspace.Tool.Hide();
        Assert.False(host.IsVisible);
        Assert.True(host.UserOrigin);
        workspace.Tool.Show();
        Assert.True(host.IsVisible);
        Assert.True(host.UserOrigin);

        // A close rejected by the protected capability hook remains a user request.
        host.Close();
        Assert.True(host.IsVisible);
        Assert.True(host.UserOrigin);
        Assert.Equal(new[] { true, true }, host.ClosingOrigins);
        Assert.Null(host.ClosedOrigin);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Model_Cleanup_And_Native_Exit_Are_Identified_Before_Closing(int operation)
    {
        using Workspace workspace = new();
        ProbeWindow host = workspace.Float();
        LayoutFloatingWindow model = (LayoutFloatingWindow)host.Model;
        if (operation == 0) workspace.Document.Dock();
        else if (operation == 1) workspace.Manager.Layout.FloatingWindows.Remove(model);
        else if (operation == 2) workspace.Document.Close();
        else model.DockWindow.Exit();
        Dispatcher.UIThread.RunJobs();
        Assert.False(host.UserOrigin);
        Assert.False(host.ClosedOrigin);
        Assert.NotEmpty(host.ClosingOrigins);
        Assert.All(host.ClosingOrigins, Assert.False);
        Assert.False(host.IsVisible);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Layout_Replacement_Is_Internal_But_View_Unload_Preserves_User_Origin(bool replaceLayout)
    {
        using Workspace workspace = new();
        ProbeWindow host = workspace.Float();
        LayoutRoot previousLayout = workspace.Manager.Layout;
        LayoutFloatingWindow model = (LayoutFloatingWindow)host.Model;
        int contentClosed = 0;
        workspace.Document.Closed += (_, _) => contentClosed++;
        if (replaceLayout) workspace.Manager.Layout = new();
        else workspace.View.Content = null;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(!replaceLayout, host.UserOrigin);
        Assert.Equal(!replaceLayout, host.ClosedOrigin);
        Assert.Equal(new[] { !replaceLayout }, host.ClosingOrigins);
        Assert.Equal(0, contentClosed);
        if (replaceLayout) Assert.Null(workspace.Document.Root);
        else Assert.Same(previousLayout, workspace.Document.Root);
        Assert.Same(model, workspace.Document.FindParent<LayoutFloatingWindow>());
        Assert.Empty(workspace.Manager.FloatingWindows);

        if (!replaceLayout)
        {
            workspace.View.Content = workspace.Manager;
            Dispatcher.UIThread.RunJobs();
            ProbeWindow replacement = Assert.IsType<ProbeWindow>(Assert.Single(workspace.Manager.FloatingWindows));
            Assert.NotSame(host, replacement);
            Assert.Same(model, replacement.Model);
            Assert.True(replacement.UserOrigin);
        }
    }

    [AvaloniaFact]
    public void An_Internal_Request_Stays_Internal_After_Cancellation()
    {
        using Workspace workspace = new();
        ProbeWindow host = workspace.Float();
        LayoutFloatingWindow model = (LayoutFloatingWindow)host.Model;
        EventHandler<Dock.Model.Core.Events.WindowClosingEventArgs> veto = (_, args) => args.Cancel = true;
        workspace.Manager.DockFactory.WindowClosing += veto;
        workspace.Document.Dock();
        Assert.True(host.IsVisible);
        Assert.False(host.UserOrigin);
        Assert.Null(host.ClosedOrigin);
        Assert.All(host.ClosingOrigins, Assert.False);
        Assert.True(host.IsTracked);
        Assert.Same(host, model.DockWindow.Host);
        Assert.Same(model, Assert.Single(workspace.Manager.Layout.FloatingWindows));
        Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Same(host, Assert.Single(workspace.Manager.DockFactory.HostWindows));
        workspace.Manager.DockFactory.WindowClosing -= veto;
        host.Close();
        Assert.False(host.IsVisible);
        Assert.False(host.UserOrigin);
        Assert.False(host.ClosedOrigin);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
    }

    private sealed class ProbeWindow : LayoutFloatingWindowControl
    {
        public ProbeWindow(ILayoutElement model) : base(model) => Model = model;
        public override ILayoutElement Model { get; }
        public bool UserOrigin => CloseInitiatedByUser;
        public bool AllowClose { get; init; }
        public bool? ClosedOrigin { get; private set; }
        public List<bool> ClosingOrigins { get; } = new();

        protected override bool CanClose(object? parameter = null) => AllowClose;

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            ClosingOrigins.Add(CloseInitiatedByUser);
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            ClosedOrigin = CloseInitiatedByUser;
            base.OnClosed(e);
        }
    }

    private sealed class Workspace : IDisposable
    {
        private readonly bool _tool;
        internal DockingManager Manager { get; } = new();
        internal LayoutDocument Document { get; } = new() { Title = "Document", Content = "Document content" };
        internal LayoutAnchorable Tool { get; } = new() { Title = "Tool", Content = "Tool content" };
        internal Window View { get; }

        internal Workspace(bool tool = false)
        {
            _tool = tool;
            LayoutPanel panel = new(new LayoutDocumentPane(Document));
            panel.Children.Add(new LayoutAnchorablePane(Tool));
            Manager.Layout = new() { RootPanel = panel };
            View = new() { Width = 800, Height = 500, Content = Manager };
            View.Show();
            Dispatcher.UIThread.RunJobs();
            Manager.DockFactory.HostWindowLocator![nameof(IDockWindow)] = () =>
                new ProbeWindow(Manager.Layout.FloatingWindows.Single(window => window.DockWindow.Host is null));
        }

        internal ProbeWindow Float()
        {
            if (_tool) Tool.Float();
            else Document.Float();
            Dispatcher.UIThread.RunJobs();
            return Assert.IsType<ProbeWindow>(Assert.Single(Manager.FloatingWindows));
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
