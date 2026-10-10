using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingCloseTests
{
    [AvaloniaFact]
    public void Native_Document_Host_Respects_Capability_And_Content_Closing_Cancellation()
    {
        using HostedSources fixture = new();
        LayoutDocument document = Assert.Single(fixture.Manager.Layout.Descendents().OfType<LayoutDocument>());
        document.Float();
        HostWindow host = fixture.GetHost();
        document.CanClose = false;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Same(fixture.Manager.Layout, document.Root);

        document.CanClose = true;
        int closing = 0;
        int closed = 0;
        bool cancel = true;
        document.Closing += (_, args) => { closing++; args.Cancel = cancel; };
        fixture.Manager.DocumentClosed += (_, _) => closed++;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Equal(1, closing);
        Assert.Equal(0, closed);
        cancel = false;
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Equal(2, closing);
        Assert.Equal(1, closed);
        Assert.Null(document.Parent);
        Assert.Empty(fixture.Manager.Layout.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Native_Tool_Host_Closes_Hides_Or_Stays_Open_According_To_Capabilities(bool canClose, bool canHide)
    {
        using HostedSources fixture = new();
        LayoutAnchorable tool = Assert.Single(fixture.Manager.Layout.Descendents().OfType<LayoutAnchorable>());
        tool.CanClose = canClose;
        tool.CanHide = canHide;
        tool.Float();
        LayoutFloatingWindow window = Assert.Single(fixture.Manager.Layout.FloatingWindows);
        HostWindow host = fixture.GetHost();
        int closed = 0;
        tool.Closed += (_, _) => closed++;
        host.Close();

        Assert.Equal(!canClose && !canHide, host.IsVisible);
        Assert.Equal(canClose ? 1 : 0, closed);
        if (canClose)
        {
            Assert.Null(tool.Parent);
            Assert.Empty(fixture.Manager.Layout.FloatingWindows);
        }
        else if (canHide)
        {
            Assert.Same(tool, Assert.Single(fixture.Manager.Layout.Hidden));
            Assert.Same(window, Assert.Single(fixture.Manager.Layout.FloatingWindows));
            tool.Show();
            HostWindow restored = fixture.GetHost();
            Assert.NotSame(host, restored);
            Assert.True(restored.IsVisible);
            Assert.Same(window, tool.FindParent<LayoutFloatingWindow>());
        }
        else Assert.Same(window, tool.FindParent<LayoutFloatingWindow>());
    }

    [AvaloniaFact]
    public void Native_Tool_Host_Keeps_Canceled_Content_And_Closes_It_Only_Once_When_Allowed()
    {
        using HostedSources fixture = new();
        LayoutAnchorable tool = Assert.Single(fixture.Manager.Layout.Descendents().OfType<LayoutAnchorable>());
        tool.CanClose = true;
        tool.Float();
        HostWindow host = fixture.GetHost();
        bool cancel = true;
        int closing = 0;
        int closed = 0;
        tool.Closing += (_, args) => { closing++; args.Cancel = cancel; };
        tool.Closed += (_, _) => closed++;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Equal(1, closing);
        Assert.Equal(0, closed);
        Assert.Same(fixture.Manager.Layout, tool.Root);
        cancel = false;
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Equal(2, closing);
        Assert.Equal(1, closed);
        Assert.Null(tool.Parent);
        Assert.Empty(fixture.Manager.Layout.FloatingWindows);
    }

    [AvaloniaFact]
    public void Native_Window_Veto_Precedes_Content_Closing_And_Hiding_Can_Also_Be_Canceled()
    {
        using HostedSources fixture = new();
        LayoutAnchorable tool = Assert.Single(fixture.Manager.Layout.Descendents().OfType<LayoutAnchorable>());
        tool.Float();
        HostWindow host = fixture.GetHost();
        int hiding = 0;
        bool cancelHide = true;
        tool.Hiding += (_, args) => { hiding++; args.Cancel = cancelHide; };
        EventHandler<Dock.Model.Core.Events.WindowClosingEventArgs> cancelWindow = (_, args) => args.Cancel = true;
        fixture.Manager.DockFactory.WindowClosing += cancelWindow;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Same(fixture.Manager.Layout, tool.Root);
        Assert.Equal(0, hiding);
        fixture.Manager.DockFactory.WindowClosing -= cancelWindow;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Equal(1, hiding);
        cancelHide = false;
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Equal(2, hiding);
        Assert.Same(tool, Assert.Single(fixture.Manager.Layout.Hidden));
    }

    [AvaloniaFact]
    public void Closing_A_Group_Keeps_The_Canceled_Tool_Attached_While_Other_Tools_Close()
    {
        using HostedSources fixture = new();
        LayoutAnchorable first = Assert.Single(fixture.Manager.Layout.Descendents().OfType<LayoutAnchorable>());
        first.CanClose = true;
        first.Float();
        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(first.Parent);
        LayoutAnchorable second = new() { CanClose = true, Content = new SourceContent("Second tool") };
        pane.Children.Add(second);
        HostWindow host = fixture.GetHost();
        int secondClosed = 0;
        second.Closed += (_, _) => secondClosed++;
        bool cancel = true;
        first.Closing += (_, args) => args.Cancel = cancel;
        host.Close();
        Assert.True(host.IsVisible);
        Assert.Same(first, Assert.Single(pane.Children));
        Assert.Same(fixture.Manager.Layout, first.Root);
        Assert.Null(second.Parent);
        Assert.Equal(1, secondClosed);
        cancel = false;
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Empty(fixture.Manager.Layout.FloatingWindows);
        Assert.Equal(1, secondClosed);
    }

    [AvaloniaFact]
    public void A_Group_That_Cannot_Close_Hides_All_Tools_And_Restores_Their_Shared_Pane()
    {
        using HostedSources fixture = new();
        LayoutAnchorable first = Assert.Single(fixture.Manager.Layout.Descendents().OfType<LayoutAnchorable>());
        first.CanClose = true;
        first.Float();
        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(first.Parent);
        LayoutAnchorable second = new() { Content = new SourceContent("Second tool") };
        pane.Children.Add(second);
        HostWindow host = fixture.GetHost();
        host.Close();
        Assert.False(host.IsVisible);
        Assert.Equal(new[] { first, second }, fixture.Manager.Layout.Hidden);
        // Hide records the index at the time of removal; undo in reverse order.
        second.Show();
        first.Show();
        Assert.Equal(new[] { first, second }, pane.Children);
        Assert.Empty(fixture.Manager.Layout.Hidden);
        Assert.True(fixture.GetHost().IsVisible);
    }

    private sealed class HostedSources : IDisposable
    {
        private readonly SourcesFixture _view = new() { DataContext = new SourceViewModel() };
        internal DockingManager Manager { get; }
        internal HostedSources()
        {
            Manager = _view.FindControl<DockingManager>("Manager")!;
            _view.Show();
            Dispatcher.UIThread.RunJobs();
        }
        internal HostWindow GetHost()
        {
            Dispatcher.UIThread.RunJobs();
            LayoutFloatingWindow window = Assert.Single(Manager.Layout.FloatingWindows);
            HostWindow host = Assert.IsAssignableFrom<HostWindow>(window.DockWindow.Host);
            Assert.Same(window.DockWindow, host.Window);
            Assert.Same(Manager.DockFactory, host.Window!.Factory);
            Assert.True(host.IsTracked);
            Assert.True(host.IsVisible);
            return host;
        }
        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            _view.Close();
        }
    }
}
