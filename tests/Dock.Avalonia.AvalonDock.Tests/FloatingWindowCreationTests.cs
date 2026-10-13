using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingWindowCreationTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Creates_A_Typed_Unshown_Host_With_The_Native_Model_And_Requested_Content_Protection(bool tool, bool immutable)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        content.FloatingWidth = 480;
        content.FloatingHeight = 320;
        content.FloatingLeft = 50;
        content.FloatingTop = 70;
        List<string> events = new();
        workspace.Manager.PreviewFloat += (_, args) => { Assert.Same(content, args.Source); events.Add("preview"); };
        workspace.Manager.LayoutFloatingWindowControlCollectionChanged += (_, args) =>
        {
            if (args.CollectionChangedEventArgs.NewItems is { } added)
            {
                LayoutFloatingWindowControl host = Assert.IsAssignableFrom<LayoutFloatingWindowControl>(Assert.Single(added));
                Assert.Equal(immutable, host.IsContentImmutable);
                Assert.False(host.IsVisible);
                events.Add("control");
            }
        };
        workspace.Manager.Floated += (_, args) => { Assert.Same(content, args.Source); events.Add("floated"); };
        int opened = 0;
        workspace.Manager.DockFactory.WindowOpened += (_, _) => opened++;

        LayoutFloatingWindowControl control = Assert.IsAssignableFrom<LayoutFloatingWindowControl>(workspace.Manager.CreateFloatingWindow(content, immutable));
        Assert.Equal(tool, control is LayoutAnchorableFloatingWindowControl);
        Assert.Equal(!tool, control is LayoutDocumentFloatingWindowControl);
        Assert.Same(control, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Same(content.FindParent<LayoutFloatingWindow>(), control.Model);
        Assert.Same(control, ((LayoutFloatingWindow)control.Model).DockWindow.Host);
        Assert.True(control.IsTracked);
        Assert.False(control.IsVisible);
        Assert.Equal(0, opened);
        Assert.Equal(new[] { "preview", "control", "floated" }, events);
        Assert.Equal(480, control.Width);
        Assert.Equal(320, control.Height);
        Assert.Equal(50, control.Position.X);
        Assert.Equal(70, control.Position.Y);

        control.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(control.IsVisible);
        Assert.Equal(1, opened);
        Assert.Contains(control, workspace.Manager.DockFactory.HostWindows);
        Assert.Contains(control.GetVisualDescendants(), element => ReferenceEquals(element, content.Content));
        object? before = control.Content;
        TextBlock replacement = new() { Text = "Custom floating window content" };
        control.Content = replacement;
        Assert.Same(immutable ? before : replacement, control.Content);
        if (!immutable)
        {
            control.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(replacement, control.GetVisualDescendants());
            control.Content = before;
            control.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(content.Content, control.GetVisualDescendants());
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Native_Presentation_And_Showing_From_The_Collection_Event_Notify_The_Factory_Once(bool showInNotification)
    {
        using Workspace workspace = new();
        int opened = 0;
        workspace.Manager.DockFactory.WindowOpened += (_, _) => opened++;
        if (showInNotification)
            workspace.Manager.LayoutFloatingWindowControlCollectionChanged += (_, args) =>
            {
                if (args.CollectionChangedEventArgs.NewItems is { } added)
                    ((LayoutFloatingWindowControl)added[0]!).Show();
            };
        LayoutFloatingWindowControl control = workspace.Manager.CreateFloatingWindow(workspace.Document, true)!;
        if (!showInNotification) ((IHostWindow)control).Present(false);
        Dispatcher.UIThread.RunJobs();
        Assert.True(control.IsVisible);
        Assert.Equal(1, opened);
        Assert.Single(workspace.Manager.DockFactory.HostWindows, host => ReferenceEquals(host, control));
        Assert.Contains(workspace.Document.Content, control.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Nested_Creation_Keeps_Each_Windows_Options_And_Pinned_Content_Remains_In_The_Destination()
    {
        using Workspace workspace = new();
        workspace.Tool.ToggleAutoHide();
        LayoutFloatingWindowControl? nested = null;
        workspace.Manager.PreviewFloat += (_, args) =>
        {
            if (ReferenceEquals(args.Source, workspace.Document))
                nested = workspace.Manager.CreateFloatingWindow(workspace.Tool, false);
        };
        LayoutFloatingWindowControl created = workspace.Manager.CreateFloatingWindow(workspace.Document, true)!;
        Assert.NotNull(nested);
        Assert.True(created.IsContentImmutable);
        Assert.False(created.IsVisible);
        Assert.False(nested.IsContentImmutable);
        Assert.False(nested.IsVisible);
        Assert.False(workspace.Tool.IsAutoHidden);
        Assert.Same(workspace.Manager.Layout, workspace.Tool.Root);
        Assert.DoesNotContain(workspace.Tool, workspace.Manager.Layout.LeftSide!.Descendents());
        Assert.DoesNotContain(workspace.Tool, workspace.Manager.Layout.RightSide!.Descendents());
        nested.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Contains(workspace.Tool.Content, nested.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Creating_Before_The_Manager_Loads_Still_Prepares_A_Native_Host_And_Later_Floats_Use_Typed_Hosts()
    {
        using Workspace workspace = new(show: false);
        int opened = 0;
        workspace.Manager.DockFactory.WindowOpened += (_, _) => opened++;
        LayoutFloatingWindowControl control = workspace.Manager.CreateFloatingWindow(workspace.Document, true)!;
        Assert.NotNull(control);
        Assert.False(control.IsVisible);
        control.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, opened);
        workspace.View.Show();
        Dispatcher.UIThread.RunJobs();
        workspace.Tool.Float();
        Assert.Equal(2, workspace.Manager.FloatingWindows.Count());
        Assert.IsType<LayoutAnchorableFloatingWindowControl>(workspace.Tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host);
    }

    [AvaloniaFact]
    public void Creates_A_Floating_Window_For_An_Unattached_Anchorable()
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = new() { ContentId = "unattached", Content = new TextBlock { Text = "Unattached" }, FloatingWidth = 420, FloatingHeight = 280, FloatingLeft = 80, FloatingTop = 90 };
        LayoutAnchorableFloatingWindowControl control = Assert.IsType<LayoutAnchorableFloatingWindowControl>(workspace.Manager.CreateFloatingWindow(tool, false));
        Assert.Same(workspace.Manager.Layout, tool.Root);
        Assert.Same(control.Model, tool.FindParent<LayoutFloatingWindow>());
        Assert.False(control.IsVisible);
        Assert.Equal(420, control.Width);
        Assert.Equal(280, control.Height);
        Assert.Equal(80, control.Position.X);
        Assert.Equal(90, control.Position.Y);
        control.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Contains(tool.Content, control.GetVisualDescendants());
        tool.Dock();
        Assert.False(tool.IsFloating);
        Assert.Same(workspace.Manager.Layout, tool.Root);
    }

    [AvaloniaFact]
    public void Standalone_And_NonVisual_Content_From_Another_Manager_Can_Be_Moved_Into_This_Manager()
    {
        using Workspace destination = new();
        using Workspace source = new();
        LayoutDocument standalone = new() { Content = "Standalone document" };
        LayoutDocumentPane pane = new(standalone);
        LayoutDocumentFloatingWindowControl standaloneHost = Assert.IsType<LayoutDocumentFloatingWindowControl>(destination.Manager.CreateFloatingWindow(standalone, false));
        Assert.Empty(pane.Children);
        Assert.Same(destination.Manager.Layout, standalone.Root);
        Assert.False(standaloneHost.IsVisible);
        // UI controls retain their original manager's logical ownership until
        // its deferred cleanup; model content can move synchronously.
        source.Document.Content = "Transferred document";
        LayoutDocumentFloatingWindowControl transferredHost = Assert.IsType<LayoutDocumentFloatingWindowControl>(destination.Manager.CreateFloatingWindow(source.Document, true));
        Assert.Same(destination.Manager.Layout, source.Document.Root);
        Assert.DoesNotContain(source.Document, source.Manager.Layout.Descendents());
        Assert.True(transferredHost.IsContentImmutable);
        Assert.False(transferredHost.IsVisible);
    }

    [AvaloniaFact]
    public void Creation_Uses_A_New_Window_And_Preserves_The_Docked_Return_Pane()
    {
        using Workspace workspace = new();
        ILayoutContainer previousPane = workspace.Tool.Parent!;
        workspace.Tool.Float();
        LayoutFloatingWindow original = workspace.Tool.FindParent<LayoutFloatingWindow>()!;
        workspace.Tool.Dock();
        LayoutFloatingWindowControl created = workspace.Manager.CreateFloatingWindow(workspace.Tool, true)!;
        Assert.NotSame(original, created.Model);
        Assert.False(created.IsVisible);
        workspace.Tool.Dock();
        Assert.Same(previousPane, workspace.Tool.Parent);
        Assert.Contains(created, workspace.Manager.FloatingWindows);
        Assert.False(created.IsVisible);
    }

    [AvaloniaFact]
    public void Reentrant_Floating_Does_Not_Inherit_The_Other_Contents_Creation_Options()
    {
        using Workspace workspace = new();
        workspace.Manager.PreviewFloat += (_, args) =>
        {
            if (ReferenceEquals(args.Source, workspace.Document)) workspace.Tool.Float();
        };
        LayoutFloatingWindowControl created = workspace.Manager.CreateFloatingWindow(workspace.Document, true)!;
        LayoutFloatingWindowControl nested = (LayoutFloatingWindowControl)workspace.Tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
        Assert.True(created.IsContentImmutable);
        Assert.False(created.IsVisible);
        Assert.False(nested.IsContentImmutable);
        Assert.True(nested.IsVisible);
    }

    [AvaloniaFact]
    public void Capability_Checks_Run_After_Preview_And_Only_Success_Raises_Completed()
    {
        using Workspace workspace = new();
        int preview = 0;
        int completed = 0;
        bool allow = false;
        workspace.Document.CanFloat = false;
        workspace.Manager.PreviewFloat += (_, _) => { preview++; if (allow) workspace.Document.CanFloat = true; };
        workspace.Manager.Floated += (_, _) => completed++;
        Assert.Null(workspace.Manager.CreateFloatingWindow(workspace.Document, true));
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Equal(1, preview);
        Assert.Equal(0, completed);
        allow = true;
        Assert.NotNull(workspace.Manager.CreateFloatingWindow(workspace.Document, false));
        Assert.Equal(2, preview);
        Assert.Equal(1, completed);
    }

    [AvaloniaFact]
    public void Throwing_Preview_Releases_Options_And_A_Changed_Layout_Stops_Creation()
    {
        using Workspace workspace = new();
        EventHandler<global::Avalonia.Interactivity.RoutedEventArgs> throwing = (_, _) => throw new InvalidOperationException("Preview failed.");
        workspace.Manager.PreviewFloat += throwing;
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.CreateFloatingWindow(workspace.Document, true));
        workspace.Manager.PreviewFloat -= throwing;
        workspace.Tool.Float();
        LayoutFloatingWindowControl normal = (LayoutFloatingWindowControl)workspace.Tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
        Assert.True(normal.IsVisible);
        Assert.False(normal.IsContentImmutable);
        workspace.Manager.PreviewFloat += (_, _) => workspace.Manager.Layout = new LayoutRoot();
        Assert.Null(workspace.Manager.CreateFloatingWindow(workspace.Document, true));
        Assert.Empty(workspace.Manager.FloatingWindows);
    }

    private sealed class Workspace : IDisposable
    {
        internal XamlFixture View { get; } = new();
        internal DockingManager Manager { get; }
        internal LayoutDocument Document { get; }
        internal LayoutAnchorable Tool { get; }

        internal Workspace(bool show = true)
        {
            Manager = View.FindControl<DockingManager>("Manager")!;
            Document = Manager.Layout.Descendents().OfType<LayoutDocument>().First();
            Tool = Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
            if (show) { View.Show(); Dispatcher.UIThread.RunJobs(); }
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
