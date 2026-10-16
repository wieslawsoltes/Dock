using System;
using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowClosedCoreTests
{
    [AvaloniaFact]
    public void An_Unbound_Host_Passes_A_Null_Closing_Model()
    {
        ProbeHost host = new();
        int closed = 0;
        host.Before = _ =>
        {
            Assert.True(((IHostWindowCloseState)host).IsClosed);
            Assert.Null(host.ClosingModel);
        };
        host.Closed += (_, _) => closed++;
        host.Show();
        host.Close();
        Assert.Equal(1, closed);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Core_Hook_Runs_After_Closed_State_And_Before_Presentation_Release_And_Factory_Notification(bool preserve)
    {
        using Workspace workspace = new(preserve);
        List<string> calls = new();
        EventArgs? arguments = null;
        workspace.Host.Before = e =>
        {
            Assert.Same(workspace.Model, workspace.Host.ClosingModel);
            Assert.True(((IHostWindowCloseState)workspace.Host).IsClosed);
            Assert.Same(workspace.Host, workspace.Layout.Parent);
            arguments = e;
            calls.Add("core-before");
        };
        workspace.Host.Closed += (_, e) =>
        {
            Assert.Same(arguments, e);
            Assert.Null(workspace.Layout.Parent);
            Assert.Contains(workspace.Host, workspace.Factory.HostWindows);
            calls.Add("closed");
        };
        workspace.Host.After = _ => calls.Add("core-after");
        workspace.Factory.WindowClosed += (_, _) =>
        {
            Assert.DoesNotContain(workspace.Host, workspace.Factory.HostWindows);
            calls.Add("factory");
        };

        workspace.Host.Close();

        Assert.Equal(new[] { "core-before", "closed", "core-after", "factory" }, calls);
    }

    [AvaloniaFact]
    public void Removing_The_Model_In_The_Core_Hook_Retains_The_Captured_Factory_Notification()
    {
        using Workspace workspace = new(preserve: true);
        workspace.Host.Before = _ => workspace.Factory.RemoveWindow(workspace.Model);
        int closed = 0;
        workspace.Host.Closed += (_, _) =>
        {
            Assert.Null(workspace.Model.Factory);
            Assert.Empty(workspace.Owner.Windows!);
            Assert.Null(workspace.Layout.Parent);
        };
        workspace.Factory.WindowClosed += (_, e) =>
        {
            Assert.Same(workspace.Model, e.Window);
            closed++;
        };

        workspace.Host.Close();

        Assert.Equal(1, closed);
        Assert.Empty(workspace.Factory.HostWindows);
        Assert.Null(workspace.Model.Host);
    }

    [AvaloniaFact]
    public void A_Replacement_Host_Installed_By_The_Core_Hook_Survives_Retirement()
    {
        using Workspace workspace = new(preserve: true);
        HostWindow replacement = new() { Window = workspace.Model };
        workspace.Host.Before = _ => workspace.Model.Host = replacement;
        try
        {
            workspace.Host.Close();
            Assert.Same(replacement, workspace.Model.Host);
            Assert.Empty(workspace.Factory.HostWindows);
        }
        finally { replacement.Close(); }
    }

    [AvaloniaFact]
    public void A_Throwing_Core_Hook_Stops_Later_Stages_And_Can_Be_Replayed()
    {
        using Workspace workspace = new(preserve: true);
        InvalidOperationException failure = new("core failure");
        int closed = 0;
        int notified = 0;
        workspace.Host.Closed += (_, _) => closed++;
        workspace.Factory.WindowClosed += (_, _) => notified++;
        workspace.Host.Before = _ => throw failure;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(workspace.Host.Close));
        Assert.True(((IHostWindowCloseState)workspace.Host).IsClosed);
        Assert.Equal(0, closed);
        Assert.Equal(0, notified);
        Assert.Same(workspace.Host, workspace.Layout.Parent);

        workspace.Host.Before = null;
        workspace.Host.Replay();

        Assert.Equal(1, closed);
        Assert.Equal(1, notified);
        Assert.Null(workspace.Layout.Parent);
        Assert.Empty(workspace.Factory.HostWindows);
    }

    [AvaloniaFact]
    public void Suppressing_The_Base_Core_Hook_Suppresses_The_Framework_Event()
    {
        using Workspace workspace = new(preserve: true);
        workspace.Host.Suppress = true;
        int closed = 0;
        int notified = 0;
        workspace.Host.Closed += (_, _) => closed++;
        workspace.Factory.WindowClosed += (_, _) => notified++;
        workspace.Host.Close();
        Assert.Equal(0, closed);
        Assert.Equal(1, notified);
        Assert.True(((IHostWindowCloseState)workspace.Host).IsClosed);
        Assert.Same(workspace.Host, workspace.Layout.Parent);
        workspace.Host.Content = null;
    }

    private sealed class ProbeHost : HostWindow
    {
        internal Action<EventArgs>? Before { get; set; }
        internal Action<EventArgs>? After { get; set; }
        internal bool Suppress { get; set; }
        internal IDockWindow? ClosingModel { get; private set; }
        internal void Replay() => base.OnClosed(EventArgs.Empty);
        protected override void OnClosedCore(EventArgs e, IDockWindow? closingWindow)
        {
            ClosingModel = closingWindow;
            Before?.Invoke(e);
            if (!Suppress) base.OnClosedCore(e, closingWindow);
            After?.Invoke(e);
        }
    }

    private sealed class Workspace : IDisposable
    {
        internal CloseFactory Factory { get; }
        internal RootDock Owner { get; } = new();
        internal RootDock Layout { get; } = new();
        internal DockWindow Model { get; }
        internal ProbeHost Host { get; } = new();

        internal Workspace(bool preserve)
        {
            Factory = new(preserve);
            Model = new() { Layout = Layout };
            Layout.Window = Model;
            Host.Content = Layout;
            Factory.AddWindow(Owner, Model);
            Factory.InitDockWindow(Model, Owner, Host);
            Model.Present(false);
        }

        public void Dispose()
        {
            Host.Before = Host.After = null;
            Host.Close();
            Factory.RemoveWindow(Model);
        }
    }

    private sealed class CloseFactory(bool preserve) : Factory, IDockWindowClosePolicy
    {
        public bool PreserveLayoutOnHostClose(IDockWindow window) => preserve;
    }
}
