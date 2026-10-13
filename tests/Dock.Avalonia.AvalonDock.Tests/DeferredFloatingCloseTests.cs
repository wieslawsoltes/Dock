using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DeferredFloatingCloseTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Deferred_Close_Releases_Native_Registration_After_Public_Content_Events(bool tools)
    {
        using Workspace workspace = new(tools);
        int closed = 0;
        int removed = 0;
        workspace.Manager.DockFactory.WindowClosed += (_, _) => closed++;
        workspace.Manager.DockFactory.WindowRemoved += (_, _) => removed++;
        workspace.Content.Closed += (_, _) =>
        {
            Assert.False(IsClosed(workspace.Host));
            Assert.Same(workspace.Host, Assert.Single(workspace.Manager.FloatingWindows));
            Assert.Same(workspace.Host, Assert.Single(workspace.Manager.DockFactory.HostWindows));
            Assert.Equal(!tools, workspace.OwnerRoot.FloatingWindows.Contains(workspace.Model));
        };

        workspace.Content.Close();
        Assert.False(IsClosed(workspace.Host));
        Assert.Equal(0, closed);
        Pump();

        Assert.True(IsClosed(workspace.Host));
        Assert.False(workspace.Host.IsTracked);
        Assert.Null(workspace.Model.DockWindow.Host);
        Assert.Null(workspace.Model.DockWindow.Owner);
        Assert.Null(workspace.Model.DockWindow.Factory);
        Assert.Null(workspace.Model.DockWindow.Layout);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
        Assert.Empty(((IRootDock)workspace.OwnerRoot.DockModel).Windows!);
        Assert.Equal(1, closed);
        Assert.Equal(1, removed);
        if (tools) Assert.Same(workspace.Host, Assert.Single(workspace.Manager.FloatingWindows));
        else Assert.Empty(workspace.Manager.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Canceled_Internal_Close_Is_Queued_Once_And_Explicit_Close_Can_Retry(bool tools)
    {
        using Workspace workspace = new(tools);
        int requests = 0;
        workspace.Host.Closing += (_, _) => requests++;
        workspace.CancelClose = true;
        workspace.Content.Close();
        workspace.OwnerRoot.CollectGarbage();
        Assert.Equal(0, requests);
        Pump();
        Assert.Equal(1, requests);
        workspace.OwnerRoot.CollectGarbage();
        Pump();
        Assert.Equal(1, requests);
        Assert.False(IsClosed(workspace.Host));
        Assert.Same(workspace.Host, Assert.Single(workspace.Manager.DockFactory.HostWindows));

        workspace.CancelClose = false;
        workspace.Host.Close();
        Pump();
        Assert.Equal(2, requests);
        Assert.True(IsClosed(workspace.Host));
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
        Assert.Null(workspace.Model.DockWindow.Factory);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Canceled_Unload_ReRegisters_The_Same_Host_On_Reattachment(bool tools)
    {
        using Workspace workspace = new(tools);
        workspace.CancelClose = true;
        workspace.View.Content = null;
        Assert.Same(workspace.Host, Assert.Single(workspace.Manager.FloatingWindows));
        Pump();
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Same(workspace.Host, Assert.Single(workspace.Manager.DockFactory.HostWindows));
        workspace.OwnerRoot.FloatingWindows.Add(new LayoutDocumentFloatingWindow());
        Assert.Empty(workspace.Manager.FloatingWindows);
        workspace.View.Content = workspace.Manager;
        Pump();
        Assert.Contains(workspace.Host, workspace.Manager.FloatingWindows);
        Assert.True(workspace.Host.IsVisible);
        Assert.False(IsClosed(workspace.Host));
        Assert.Same(workspace.OwnerRoot, workspace.Content.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unloading_Retries_Previously_Canceled_Empty_Hosts(bool tools)
    {
        using Workspace workspace = new(tools);
        workspace.CancelClose = true;
        workspace.Content.Close();
        Pump();
        Assert.False(IsClosed(workspace.Host));
        workspace.CancelClose = false;
        workspace.View.Content = null;
        Pump();
        Assert.True(IsClosed(workspace.Host));
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
        Assert.Null(workspace.Model.DockWindow.Factory);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reattachment_Before_The_Unload_Turn_Keeps_The_Existing_Host(bool tools)
    {
        using Workspace workspace = new(tools);
        int requests = 0;
        workspace.Host.Closing += (_, _) => requests++;
        workspace.View.Content = null;
        workspace.View.Content = workspace.Manager;
        Pump();
        Assert.Equal(0, requests);
        Assert.Same(workspace.Host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.True(workspace.Host.IsVisible);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Explicit_Close_Before_The_Queued_Turn_Does_Not_Close_Twice(bool tools)
    {
        using Workspace workspace = new(tools);
        int requests = 0;
        int closed = 0;
        workspace.Host.Closing += (_, _) => requests++;
        workspace.Host.Closed += (_, _) => closed++;
        workspace.Content.Close();
        workspace.Host.Close();
        Pump();
        Assert.Equal(1, requests);
        Assert.Equal(1, closed);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Closed_Document_Can_Be_Adopted_Through_Public_Or_Native_Collections(bool native)
    {
        using Workspace workspace = new(tools: false);
        workspace.Content.Close();
        Pump();
        Assert.Same(workspace.Model, workspace.Content.Parent);
        Assert.Null(workspace.Content.Root);
        LayoutDocumentPane pane = (LayoutDocumentPane)workspace.OwnerRoot.RootPanel.Children[0];
        if (native) workspace.Manager.DockFactory.AddDockable((IDock)pane.DockModel, workspace.Content.DockModel);
        else pane.Children.Add(workspace.Content);
        Assert.Same(pane, workspace.Content.Parent);
        Assert.Same(workspace.OwnerRoot, workspace.Content.Root);
        Assert.Contains(workspace.Content, pane.Children);
        Assert.Same(pane.DockModel, workspace.Content.DockModel.Owner);
    }

    [AvaloniaFact]
    public void A_Replacement_Host_Created_From_WindowClosed_Survives_The_Old_Queue()
    {
        using Workspace workspace = new(tools: false);
        LayoutDocumentFloatingWindow model = (LayoutDocumentFloatingWindow)workspace.Model;
        LayoutDocument replacement = new() { Title = "Replacement", Content = "Replacement" };
        LayoutFloatingWindowControl? replacementHost = null;
        workspace.Manager.DockFactory.WindowClosed += (_, _) =>
        {
            if (replacementHost is not null) return;
            model.RootDocument = replacement;
            // Without an empty pane, collection retains the model until the
            // old host's internal-close phase. Reuse its rooted registration.
            Assert.Contains(model, workspace.OwnerRoot.FloatingWindows);
            model.DockWindow.Present(false);
            replacementHost = (LayoutFloatingWindowControl)model.DockWindow.Host!;
        };
        workspace.Content.Close();
        Pump();
        Assert.NotNull(replacementHost);
        Assert.NotSame(workspace.Host, replacementHost);
        Assert.True(replacementHost.IsVisible);
        Assert.False(IsClosed(replacementHost));
        Assert.Same(model, Assert.Single(workspace.OwnerRoot.FloatingWindows));
        Assert.Same(replacementHost, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Same(replacementHost, Assert.Single(workspace.Manager.DockFactory.HostWindows));
        Assert.Same(workspace.OwnerRoot, replacement.Root);
    }

    [AvaloniaFact]
    public void A_Queued_Close_Targets_Its_Old_Host_After_Native_Host_Replacement()
    {
        using Workspace workspace = new(tools: false);
        LayoutDocumentFloatingWindow model = (LayoutDocumentFloatingWindow)workspace.Model;
        workspace.Content.Close();
        model.RootDocument = new() { Title = "Replacement" };
        workspace.Manager.DockFactory.InitDockWindow(model.DockWindow, workspace.OwnerRoot.DockModel);
        LayoutFloatingWindowControl replacement = (LayoutFloatingWindowControl)model.DockWindow.Host!;
        model.DockWindow.Present(false);
        Pump();
        Assert.True(IsClosed(workspace.Host));
        Assert.False(IsClosed(replacement));
        Assert.True(replacement.IsVisible);
        Assert.Same(replacement, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Same(replacement, Assert.Single(workspace.Manager.DockFactory.HostWindows));
        Assert.Same(model, Assert.Single(workspace.OwnerRoot.FloatingWindows));
    }

    [AvaloniaFact]
    public void A_Standalone_Host_Closes_When_Its_Document_Is_Removed()
    {
        LayoutDocumentFloatingWindow model = new() { RootDocument = new() };
        StandaloneHost host = new(model);
        try
        {
            host.Show();
            model.RootDocument = null;
            Assert.False(IsClosed(host));
            Pump();
            Assert.True(IsClosed(host));
        }
        finally { host.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Retaining_A_Floating_Model_Does_Not_Retain_Its_Closed_Host(bool tools)
    {
        var (model, host) = CreateClosedHost(tools);
        for (int i = 0; i < 20; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Pump();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (!IsRetained(host)) break;
            await Task.Delay(10);
        }
        Assert.False(IsRetained(host));
        GC.KeepAlive(model);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsRetained(WeakReference<LayoutFloatingWindowControl> host) => host.TryGetTarget(out _);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (LayoutFloatingWindow Model, WeakReference<LayoutFloatingWindowControl> Host) CreateClosedHost(bool tools)
    {
        using Workspace workspace = new(tools);
        workspace.Host.Close();
        Assert.True(IsClosed(workspace.Host));
        Assert.Null(workspace.Model.DockModel.Parent);
        Assert.Null(workspace.Model.DockWindow.Host);
        foreach (LayoutElement child in workspace.Model.Descendents().OfType<LayoutElement>())
            Assert.Null(child.DockModel.Parent);
        return (workspace.Model, new(workspace.Host));
    }

    private static bool IsClosed(LayoutFloatingWindowControl host) => ((IHostWindowCloseState)host).IsClosed;
    private static void Pump() => Dispatcher.UIThread.RunJobs();

    private sealed class StandaloneHost(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model);

    private sealed class Workspace : IDisposable
    {
        internal LayoutContent Content { get; }
        internal DockingManager Manager { get; }
        internal LayoutRoot OwnerRoot { get; }
        internal LayoutFloatingWindow Model { get; }
        internal LayoutFloatingWindowControl Host { get; }
        internal Window View { get; }
        internal bool CancelClose { get; set; }

        internal Workspace(bool tools)
        {
            Content = tools ? new LayoutAnchorable { CanClose = true } : new LayoutDocument();
            Content.Title = "Floating content";
            Content.Content = "Content";
            LayoutDocumentPane documents = new(new LayoutDocument { Title = "Other" });
            LayoutPanel panel = new(documents);
            if (Content is LayoutAnchorable tool) panel.Children.Add(new LayoutAnchorablePane(tool));
            else documents.Children.Add(Content);
            OwnerRoot = new() { RootPanel = panel };
            Manager = new() { Layout = OwnerRoot };
            View = new() { Content = Manager, Width = 640, Height = 400 };
            View.Show();
            Pump();
            Content.Float();
            Pump();
            Model = Assert.Single(OwnerRoot.FloatingWindows);
            Host = Assert.Single(Manager.FloatingWindows);
            Host.Closing += (_, args) => args.Cancel = CancelClose;
        }

        public void Dispose()
        {
            CancelClose = false;
            if (!IsClosed(Host)) Host.Close();
            OwnerRoot.FloatingWindows.Clear();
            // Reset preserves public parent/root references. Explicitly detach
            // the disposed workspace's containers and their retained Views.
            Manager.Layout = new();
            View.Close();
            Pump();
        }
    }
}
