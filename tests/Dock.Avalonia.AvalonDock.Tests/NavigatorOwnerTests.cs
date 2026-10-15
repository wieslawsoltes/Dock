using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class NavigatorOwnerTests
{
    [AvaloniaFact]
    public void Document_Selection_Hides_The_Navigator_And_Owner_Closure_Later_Closes_It()
    {
        using Workspace workspace = new();
        NavigatorWindow navigator = workspace.Open();
        int closed = 0;
        navigator.Closed += (_, _) => closed++;
        navigator.SelectedDocument = navigator.Documents![0];
        Assert.False(navigator.IsVisible);
        Assert.Null(navigator.Owner);
        Assert.Equal(0, closed);
        NavigatorWindow next = workspace.Open();
        Assert.NotSame(navigator, next);
        next.Close();
        workspace.Owner.Close();
        Assert.Equal(1, closed);
        Assert.Throws<InvalidOperationException>(() => navigator.Show());
    }

    [AvaloniaFact]
    public void Canceled_Owner_Closure_Keeps_The_Hidden_Navigator_Until_The_Owner_Actually_Closes()
    {
        using Workspace workspace = new();
        NavigatorWindow navigator = workspace.Open();
        bool closed = false;
        navigator.Closed += (_, _) => closed = true;
        navigator.Hide();
        EventHandler<WindowClosingEventArgs> cancel = (_, args) => args.Cancel = true;
        workspace.Owner.Closing += cancel;
        workspace.Owner.Close();
        Assert.False(closed);
        Assert.True(workspace.Owner.IsVisible);
        workspace.Owner.Closing -= cancel;
        workspace.Owner.Close();
        Assert.True(closed);
    }

    [AvaloniaFact]
    public void Hidden_Navigator_Cannot_Cancel_Cleanup_After_Its_Owner_Has_Closed()
    {
        using Workspace workspace = new();
        NavigatorWindow navigator = workspace.Open();
        navigator.Hide();
        bool programmatic = false;
        navigator.Closing += (_, args) => { programmatic = args.IsProgrammatic; args.Cancel = true; };
        bool closed = false;
        navigator.Closed += (_, _) => closed = true;
        workspace.Owner.Close();
        Assert.True(closed);
        Assert.True(programmatic);
    }

    [AvaloniaFact]
    public void Showing_With_A_New_Owner_Replaces_The_Previous_Owner_Connection()
    {
        using Workspace first = new();
        using Workspace second = new();
        NavigatorWindow navigator = first.Open();
        navigator.Hide();
        _ = navigator.ShowDialog(second.Owner);
        navigator.Hide();
        bool closed = false;
        navigator.Closed += (_, _) => closed = true;
        first.Owner.Close();
        Assert.False(closed);
        second.Owner.Close();
        Assert.True(closed);
    }

    [AvaloniaFact]
    public void Showing_Without_An_Owner_Releases_The_Previous_Owner_Connection()
    {
        using Workspace workspace = new();
        NavigatorWindow navigator = workspace.Open();
        navigator.Hide();
        navigator.Show();
        try
        {
            workspace.Owner.Close();
            Assert.True(navigator.IsVisible);
        }
        finally { navigator.Close(); }
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        internal Window Owner { get; }
        internal Workspace()
        {
            LayoutDocumentPane pane = new();
            pane.Children.Add(new LayoutDocument { Title = "A" });
            pane.Children.Add(new LayoutDocument { Title = "B" });
            Manager.Layout = new() { RootPanel = new(pane) };
            Owner = new() { Content = Manager, Width = 640, Height = 480 };
            Owner.Show();
            Dispatcher.UIThread.RunJobs();
        }
        internal NavigatorWindow Open()
        {
            NavigatorWindow navigator = Manager.CreateNavigatorWindow();
            _ = navigator.ShowDialog(Owner);
            Dispatcher.UIThread.RunJobs();
            return navigator;
        }
        public void Dispose() { Owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
