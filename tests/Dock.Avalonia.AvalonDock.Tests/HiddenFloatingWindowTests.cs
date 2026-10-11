using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class HiddenFloatingWindowTests
{
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Hiding_And_Restoring_Uses_The_Same_Host_Without_Collection_Churn(int operation)
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.CreateWindow();
        LayoutAnchorableFloatingWindow model = (LayoutAnchorableFloatingWindow)host.Model;
        object marker = new();
        host.Resources["ApplicationState"] = marker;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        object? originalContent = host.Content;
        host.Position = new PixelPoint(70, 90);
        host.Width = 420;
        host.Height = 280;
        Dispatcher.UIThread.RunJobs();
        int closed = 0;
        int changes = 0;
        host.Closed += (_, _) => closed++;
        workspace.Manager.LayoutFloatingWindowControlCollectionChanged += (_, _) => changes++;
        for (int i = 0; i < 3; i++)
        {
            if (operation == 0) workspace.Tool.Hide();
            else if (operation == 1) host.HideWindowCommand.Execute(null);
            else host.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.True(workspace.Tool.IsHidden);
            Assert.False(host.IsVisible);
            Assert.False(model.IsVisible);
            Assert.True(host.IsTracked);
            Assert.Same(host, model.DockWindow.Host);
            Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
            Assert.Same(host, Assert.Single(workspace.Manager.DockFactory.HostWindows));
            Assert.Equal(0, closed);
            Assert.Equal(0, changes);

            workspace.Tool.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsVisible);
            Assert.True(model.IsVisible);
            Assert.Same(host, Assert.Single(workspace.Manager.DockFactory.HostWindows));
            Assert.Same(originalContent, host.Content);
            Assert.Same(marker, host.Resources["ApplicationState"]);
            Assert.Same(item, host.SingleContentLayoutItem);
            Assert.Contains(workspace.Tool.Content, host.GetVisualDescendants());
            Assert.Equal(new PixelPoint(70, 90), host.Position);
            Assert.Equal(420, host.Width);
            Assert.Equal(280, host.Height);
        }
        Assert.Equal(0, closed);
        Assert.Equal(0, changes);
    }

    [AvaloniaFact]
    public void A_Canceled_Hide_Leaves_The_Same_Host_Visible_And_The_Last_Hidden_Tool_Hides_It()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.CreateWindow();
        LayoutAnchorablePane pane = (LayoutAnchorablePane)workspace.Tool.Parent!;
        LayoutAnchorable second = new() { Content = new TextBlock { Text = "Second tool" } };
        pane.Children.Add(second);
        bool cancel = true;
        workspace.Tool.Hiding += (_, args) => args.Cancel = cancel;
        host.HideWindowCommand.Execute(null);
        Assert.True(host.IsVisible);
        Assert.True(second.IsHidden);
        cancel = false;
        host.HideWindowCommand.Execute(null);
        Assert.False(host.IsVisible);
        second.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(host.IsVisible);
        Assert.True(workspace.Tool.IsHidden);
        Assert.Contains(second.Content, host.GetVisualDescendants());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unshown_Creation_Does_Not_Present_Until_Requested_And_Show_Respects_Hidden_Contents(bool showWhileHidden)
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.CreateWindow(show: false);
        workspace.Tool.Hide();
        if (showWhileHidden)
        {
            host.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsVisible);
        }
        workspace.Tool.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(showWhileHidden, host.IsVisible);
        if (!showWhileHidden) host.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(host.IsVisible);
        Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Same(host, Assert.Single(workspace.Manager.DockFactory.HostWindows));
    }

    [AvaloniaFact]
    public void Detaching_Closes_The_Hidden_Host_And_Reattaching_Preserves_Its_Model_And_Hidden_State()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl original = workspace.CreateWindow();
        workspace.Tool.Hide();
        int closed = 0;
        original.Closed += (_, _) => closed++;
        workspace.View.Content = null;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, closed);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Empty(workspace.Manager.DockFactory.HostWindows);
        workspace.View.Content = workspace.Manager;
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl replacement = Assert.Single(workspace.Manager.FloatingWindows);
        Assert.NotSame(original, replacement);
        Assert.Same(original.Model, replacement.Model);
        Assert.False(replacement.IsVisible);
        workspace.Tool.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(replacement.IsVisible);
        Assert.Same(replacement, Assert.Single(workspace.Manager.FloatingWindows));
    }

    [AvaloniaFact]
    public void Xml_Replacement_Closes_Old_Hidden_Hosts_And_Restores_Hidden_Tools_Into_New_Hosts()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl original = workspace.CreateWindow();
        workspace.Tool.Hide();
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        int closed = 0;
        original.Closed += (_, _) => closed++;
        serializer.Deserialize(new StringReader(writer.ToString()));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, closed);
        Assert.DoesNotContain(original, workspace.Manager.FloatingWindows);
        LayoutAnchorable tool = Assert.Single(workspace.Manager.Layout.Hidden);
        LayoutFloatingWindowControl replacement = Assert.Single(workspace.Manager.FloatingWindows);
        Assert.False(replacement.IsVisible);
        Assert.Same(workspace.Tool.Content, tool.Content);
        tool.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(replacement.IsVisible);
        Assert.Same(replacement.Model, tool.FindParent<LayoutFloatingWindow>());
        Assert.Contains(tool.Content, replacement.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Retaining_The_Empty_Collection_And_Disposed_Command_Does_Not_Retain_Hidden_Hosts()
    {
        var (collection, command, manager, host) = CreateDisposedWorkspace();
        for (int i = 0; i < 8 && (manager.IsAlive || host.IsAlive); i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.Empty(collection);
        Assert.False(command.CanExecute(null));
        Assert.False(manager.IsAlive);
        Assert.False(host.IsAlive);
        GC.KeepAlive(collection);
        GC.KeepAlive(command);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (IEnumerable<LayoutFloatingWindowControl>, System.Windows.Input.ICommand, WeakReference, WeakReference) CreateDisposedWorkspace()
    {
        using Workspace workspace = new();
        LayoutAnchorableFloatingWindowControl host = workspace.CreateWindow();
        workspace.Tool.Hide();
        return (workspace.Manager.FloatingWindows, host.HideWindowCommand, new(workspace.Manager), new(host));
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        internal LayoutAnchorable Tool { get; } = new() { ContentId = "tool", Content = new TextBox { Text = "Tool content" } };
        internal Window View { get; }
        internal Workspace()
        {
            LayoutPanel panel = new(new LayoutDocumentPane(new LayoutDocument { Content = "Document" }));
            panel.Children.Add(new LayoutAnchorablePane(Tool));
            Manager.Layout = new() { RootPanel = panel };
            View = new() { Width = 800, Height = 500, Content = Manager };
            View.Show();
            Dispatcher.UIThread.RunJobs();
        }
        internal LayoutAnchorableFloatingWindowControl CreateWindow(bool show = true)
        {
            LayoutAnchorableFloatingWindowControl host = (LayoutAnchorableFloatingWindowControl)Manager.CreateFloatingWindow(Tool, true)!;
            if (show) { host.Show(); Dispatcher.UIThread.RunJobs(); }
            return host;
        }
        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
