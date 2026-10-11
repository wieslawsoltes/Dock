using System;
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

public class FloatingCleanupCancellationTests
{
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Canceled_Cleanup_Retains_The_Host_And_Ownership_Until_Retry(int operation)
    {
        using Workspace workspace = new();
        LayoutFloatingWindow model = workspace.Float();
        LayoutFloatingWindowControl host = (LayoutFloatingWindowControl)model.DockWindow.Host!;
        IDockable? owner = model.DockWindow.Owner;
        object? nativeLayout = model.DockWindow.Layout;
        LayoutDocumentFloatingWindow replacement = new() { RootDocument = new() { Title = "Replacement" } };
        int closed = 0;
        int removed = 0;
        host.Closed += (_, _) => closed++;
        workspace.Manager.DockFactory.WindowRemoved += (_, _) => removed++;
        workspace.CancelClose = true;
        if (operation == 0) workspace.Document.Dock();
        else if (operation == 1) workspace.Document.Close();
        else if (operation == 2) workspace.Manager.Layout.FloatingWindows.Remove(model);
        else if (operation == 3) workspace.Manager.Layout.FloatingWindows.Clear();
        else workspace.Manager.Layout.FloatingWindows[0] = replacement;

        Assert.True(host.IsVisible);
        Assert.True(host.IsTracked);
        Assert.False(((IHostWindowCloseState)host).IsClosed);
        Assert.Same(host, model.DockWindow.Host);
        Assert.Same(workspace.Manager.Layout, model.Parent);
        Assert.Same(owner, model.DockWindow.Owner);
        Assert.Same(nativeLayout, model.DockWindow.Layout);
        Assert.Same(workspace.Manager.DockFactory, model.DockWindow.Factory);
        Assert.Same(model, Assert.Single(workspace.Manager.Layout.FloatingWindows));
        Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Same(host, Assert.Single(workspace.Manager.DockFactory.HostWindows));
        Assert.Null(replacement.Parent);
        Assert.Equal(0, closed);
        Assert.Equal(0, removed);
        // Empty floating models can remain while their native close is vetoed.
        // Collection must return, and subsequent collection can retry later.
        workspace.Manager.Layout.CollectGarbage();
        Assert.Same(model, Assert.Single(workspace.Manager.Layout.FloatingWindows));
        workspace.CancelClose = false;
        if (operation < 2) workspace.Manager.Layout.CollectGarbage();
        else workspace.Manager.Layout.FloatingWindows.Remove(model);
        Assert.False(host.IsVisible);
        Assert.True(((IHostWindowCloseState)host).IsClosed);
        Assert.Null(model.DockWindow.Host);
        Assert.Null(model.Parent);
        Assert.Null(model.DockWindow.Owner);
        Assert.Null(model.DockWindow.Factory);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
        Assert.Equal(1, closed);
        Assert.Equal(1, removed);
    }

    [AvaloniaFact]
    public void A_Canceled_Transfer_Leaves_The_Window_In_Its_Original_Manager()
    {
        using Workspace source = new();
        using Workspace target = new();
        LayoutFloatingWindow model = source.Float();
        IHostWindow? host = model.DockWindow.Host;
        source.CancelClose = true;
        Assert.Throws<InvalidOperationException>(() => target.Manager.Layout.FloatingWindows.Add(model));
        Assert.Same(model, Assert.Single(source.Manager.Layout.FloatingWindows));
        Assert.Same(source.Manager.Layout, source.Document.Root);
        Assert.Same(host, model.DockWindow.Host);
        Assert.Same(source.Manager.DockFactory, model.DockWindow.Factory);
        Assert.Empty(target.Manager.Layout.FloatingWindows);
        Assert.Empty(target.Manager.FloatingWindows);
        source.CancelClose = false;
        target.Manager.Layout.FloatingWindows.Add(model);
        Assert.Empty(source.Manager.Layout.FloatingWindows);
        Assert.Empty(source.Manager.FloatingWindows);
        Assert.Empty(source.Manager.DockFactory.HostWindows);
        Assert.Same(model, Assert.Single(target.Manager.Layout.FloatingWindows));
        Assert.Same(target.Manager.Layout, source.Document.Root);
        Assert.NotSame(host, model.DockWindow.Host);
        Assert.Same(target.Manager.DockFactory, model.DockWindow.Factory);
    }

    [AvaloniaFact]
    public void Clearing_Multiple_Previously_Canceled_Empty_Hosts_Tolerates_Closed_Callback_Cleanup()
    {
        using Workspace workspace = new();
        LayoutDocumentPane pane = (LayoutDocumentPane)workspace.Document.Parent!;
        LayoutDocument second = new() { Title = "Second" };
        pane.Children.Add(second);
        workspace.Float();
        second.Float();
        LayoutFloatingWindowControl[] hosts = workspace.Manager.FloatingWindows.ToArray();
        Assert.Equal(2, hosts.Length);
        workspace.CancelClose = true;
        workspace.Document.Close();
        second.Close();
        Assert.Equal(2, workspace.Manager.Layout.FloatingWindows.Count);
        workspace.CancelClose = false;
        workspace.Manager.Layout.FloatingWindows.Clear();
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
        Assert.All(hosts, host => Assert.True(((IHostWindowCloseState)host).IsClosed));
    }

    [AvaloniaFact]
    public void Native_Closing_Cancellation_Precedes_Content_Commands_And_Retains_Registration()
    {
        using Workspace workspace = new();
        LayoutFloatingWindow model = workspace.Float();
        LayoutFloatingWindowControl host = (LayoutFloatingWindowControl)model.DockWindow.Host!;
        bool cancel = true;
        int contentClosing = 0;
        int closed = 0;
        workspace.Document.Closing += (_, _) => contentClosing++;
        workspace.Document.Closed += (_, _) => closed++;
        host.Closing += (_, args) => args.Cancel = cancel;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Same(host, model.DockWindow.Host);
        Assert.Same(model, workspace.Document.FindParent<LayoutFloatingWindow>());
        Assert.Equal(0, contentClosing);
        Assert.Equal(0, closed);
        cancel = false;
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Null(workspace.Document.Root);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
        Assert.Equal(1, contentClosing);
        Assert.Equal(1, closed);
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        internal LayoutDocument Document { get; } = new() { Title = "Document", Content = "Content" };
        internal bool CancelClose { get; set; }
        private readonly Window _view;

        internal Workspace()
        {
            Manager.Layout = new() { RootPanel = new(new LayoutDocumentPane(Document)) };
            Manager.DockFactory.WindowClosing += (_, args) => args.Cancel = CancelClose;
            _view = new() { Width = 800, Height = 500, Content = Manager };
            _view.Show();
            Dispatcher.UIThread.RunJobs();
        }

        internal LayoutFloatingWindow Float()
        {
            Document.Float();
            Dispatcher.UIThread.RunJobs();
            return Manager.Layout.FloatingWindows.Single();
        }

        public void Dispose()
        {
            CancelClose = false;
            Manager.Layout.FloatingWindows.Clear();
            _view.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
