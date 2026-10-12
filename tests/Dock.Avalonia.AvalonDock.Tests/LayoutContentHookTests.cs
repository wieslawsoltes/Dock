using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutContentHookTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selection_Hooks_And_Events_Precede_The_Property_Changed_Notification(bool tools)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        content.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName == nameof(content.IsSelected)) probe.Trace.Add("selection:changing");
        };
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(content.IsSelected)) probe.Trace.Add("selection:changed");
        };
        content.IsSelectedChanged += (_, _) => probe.Trace.Add("selection:event");
        probe.Selection = (oldValue, newValue) =>
        {
            Assert.NotEqual(oldValue, newValue);
            Assert.Equal(newValue, content.IsSelected);
        };

        content.IsSelected = true;
        content.IsSelected = true;
        content.IsSelected = false;

        Assert.Equal(new[]
        {
            "selection:changing", "selection:hook", "selection:event", "selection:changed",
            "selection:changing", "selection:hook", "selection:event", "selection:changed"
        }, probe.Trace);
        Assert.Equal(new[] { (false, true), (true, false) }, probe.Selections);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Direct_Native_Selection_Invokes_The_Derived_Hook_With_Old_And_New_Values(bool tools)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        IDockingWindowState native = (IDockingWindowState)content.DockModel;
        content.IsSelectedChanged += (_, _) => probe.Trace.Add("selection:event");

        native.IsSelected = true;
        native.IsSelected = false;

        Assert.Equal(new[] { (false, true), (true, false) }, probe.Selections);
        Assert.Equal(new[] { "selection:hook", "selection:event", "selection:hook", "selection:event" }, probe.Trace);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Activation_Selects_Detached_Content_Before_The_Hook_And_Records_Time_In_The_Base_Hook(bool tools)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        DateTime previous = new(2000, 1, 1);
        content.LastActivationTimeStamp = previous;
        probe.Activation = (_, newValue) =>
        {
            Assert.Equal(newValue, content.IsActive);
            Assert.True(content.IsSelected);
            if (newValue) Assert.Equal(previous, content.LastActivationTimeStamp);
        };
        content.IsActiveChanged += (_, _) =>
        {
            Assert.NotEqual(previous, content.LastActivationTimeStamp);
            probe.Trace.Add("active:event");
        };
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(content.IsActive)) probe.Trace.Add("active:changed");
        };

        content.IsActive = true;
        DateTime? activated = content.LastActivationTimeStamp;
        content.IsActive = true;
        content.IsActive = false;

        Assert.Equal(new[] { (false, true), (true, false) }, probe.Activations);
        Assert.Equal(activated, content.LastActivationTimeStamp);
        Assert.Equal(new[] { "selection:hook", "active:hook", "active:event", "active:changed", "active:hook", "active:event", "active:changed" }, probe.Trace);
    }

    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    [InlineData(false, 4)]
    [InlineData(true, 4)]
    public void Activation_Hooks_See_The_Selected_Content_And_Updated_Root(bool tools, int route)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        LayoutContent other = tools ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutGroup pane = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        pane.InsertChildAt(0, other);
        pane.InsertChildAt(1, content);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) };
        other.IsActive = true;
        probe.Activations.Clear();
        probe.Activation = (_, newValue) =>
        {
            if (!newValue) return;
            Assert.Same(content, root.ActiveContent);
            Assert.Same(content, ((ILayoutContentSelector)pane).SelectedContent);
            Assert.True(content.IsSelected);
        };

        switch (route)
        {
            case 0: content.IsActive = true; break;
            case 1: ((IDockingWindowState)content.DockModel).IsActive = true; break;
            case 2: root.DockFactory.SetActiveDockable(content.DockModel); break;
            case 3: root.ActiveContent = content; break;
            case 4:
                content.IsSelected = true;
                Assert.Same(other, root.ActiveContent);
                root.DockFactory.SetActiveDockable(content.DockModel);
                break;
        }

        Assert.Equal(new[] { (false, true) }, probe.Activations);
        Assert.Same(content, root.ActiveContent);
        Assert.False(other.IsActive);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Omitting_Base_State_Hooks_Suppresses_Their_Events_And_Timestamp_Only(bool tools)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        probe.CallBase = false;
        DateTime previous = new(2000, 1, 1);
        content.LastActivationTimeStamp = previous;
        int events = 0;
        content.IsSelectedChanged += (_, _) => events++;
        content.IsActiveChanged += (_, _) => events++;

        content.IsActive = true;

        Assert.True(content.IsActive);
        Assert.True(content.IsSelected);
        Assert.Equal(previous, content.LastActivationTimeStamp);
        Assert.Equal(0, events);
        Assert.Equal(new[] { (false, true) }, probe.Activations);
        Assert.Equal(new[] { (false, true) }, probe.Selections);
    }

    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void Closing_Hooks_Cancel_And_Then_Report_Detachment_Through_Each_Close_Path(bool tools, int route)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        ILayoutGroup pane = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        pane.InsertChildAt(0, content);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) };
        DockingManager manager = new() { Layout = root };
        probe.Trace.Clear();
        probe.CancelClose = true;
        probe.Closing = () => Assert.Same(pane, content.Parent);
        probe.Closed = () => { Assert.Null(content.Parent); Assert.Null(content.Root); };
        content.Closing += (_, args) => { Assert.Equal(probe.CancelClose, args.Cancel); probe.Trace.Add("closing:event"); };
        content.Closed += (_, _) => probe.Trace.Add("closed:event");
        manager.DocumentClosing += (_, _) => probe.Trace.Add("manager:closing");
        manager.DocumentClosed += (_, _) => probe.Trace.Add("manager:closed");
        void Close()
        {
            if (route == 0) content.Close();
            else if (route == 1) root.DockFactory.CloseDockable(content.DockModel);
            else manager.GetLayoutItemFromModel(content)!.CloseCommand!.Execute(null);
        }

        Close();
        Assert.Same(pane, content.Parent);
        Assert.DoesNotContain("closed:hook", probe.Trace);
        probe.CancelClose = false;
        probe.Trace.Clear();
        Close();

        Assert.Null(content.Parent);
        List<string> closeTrace = probe.Trace.FindAll(value => !value.StartsWith("selection:") && !value.StartsWith("active:"));
        Assert.Equal(tools
            ? new[] { "closing:hook", "closing:event", "closed:hook", "closed:event" }
            : new[] { "manager:closing", "closing:hook", "closing:event", "closed:hook", "closed:event", "manager:closed" }, closeTrace);
    }

    [AvaloniaFact]
    public void A_Manager_Cancellation_Prevents_The_Content_Closing_Hook()
    {
        ProbeDocument document = new();
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        document.Probe.Trace.Clear();
        manager.DocumentClosing += (_, args) => args.Cancel = true;

        document.Close();

        Assert.Empty(document.Probe.Trace);
        Assert.Same(manager.Layout, document.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Detached_Contents_Use_The_Same_Closing_And_Closed_Hooks(bool tools)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        ILayoutGroup pane = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        pane.InsertChildAt(0, content);
        probe.CancelClose = true;
        probe.Closed = () => Assert.Null(content.Parent);

        content.Close();
        Assert.Same(pane, content.Parent);
        probe.CancelClose = false;
        content.Close();

        Assert.Null(content.Parent);
        Assert.Contains("closed:hook", probe.Trace);
    }

    [AvaloniaFact]
    public void A_Closed_Override_Can_Reopen_The_Content_In_Another_Layout()
    {
        ProbeDocument document = new();
        LayoutRoot first = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
        LayoutDocumentPane destination = new();
        LayoutRoot second = new() { RootPanel = new LayoutPanel(destination) };
        document.Probe.Closed = () =>
        {
            Assert.Null(document.Parent);
            destination.Children.Add(document);
        };

        document.Close();

        Assert.Same(destination, document.Parent);
        Assert.Same(second, document.Root);
        Assert.Contains(document, destination.Children);
        GC.KeepAlive(first);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void An_Activation_Override_Can_Redirect_Focus_To_Another_Content(bool tools, bool native)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        LayoutDocument first = new();
        LayoutDocument destination = new();
        LayoutDocumentPane pane = new(first);
        pane.Children.Add(content);
        pane.Children.Add(destination);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        first.IsActive = true;
        probe.Activation = (_, active) => { if (active) destination.IsActive = true; };

        if (native) root.DockFactory.SetActiveDockable(content.DockModel);
        else content.IsActive = true;

        Assert.Same(destination, root.ActiveContent);
        Assert.Same(destination, pane.SelectedContent);
        Assert.True(destination.IsActive);
        Assert.False(content.IsActive);
        Assert.False(first.IsActive);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Throwing_Activation_Override_Does_Not_Block_Later_Focus_Changes(bool tools)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        LayoutDocument other = new();
        LayoutDocumentPane pane = new(other);
        pane.Children.Add(content);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        other.IsActive = true;
        probe.Activation = (_, active) => { if (active) throw new InvalidOperationException("activation observer"); };

        Assert.Throws<InvalidOperationException>(() => root.ActiveContent = content);
        probe.Activation = null;
        other.IsActive = true;
        content.IsActive = true;

        Assert.Same(content, root.ActiveContent);
        Assert.Same(content, pane.SelectedContent);
        Assert.True(content.IsActive);
        Assert.False(other.IsActive);
    }

    [AvaloniaFact]
    public void A_Failed_Activation_Notification_Does_Not_Retain_Removed_Content()
    {
        LayoutRoot root = new();
        WeakReference<LayoutContent> reference = RemoveAfterFailedActivation(root);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(reference.TryGetTarget(out _));
        GC.KeepAlive(root);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<LayoutContent> RemoveAfterFailedActivation(LayoutRoot root)
    {
        ProbeDocument content = new();
        LayoutDocument other = new();
        LayoutDocumentPane pane = new(other);
        pane.Children.Add(content);
        root.RootPanel = new LayoutPanel(pane);
        other.IsActive = true;
        content.Probe.Activation = (_, active) => { if (active) throw new InvalidOperationException("activation observer"); };
        Assert.Throws<InvalidOperationException>(() => root.ActiveContent = content);
        content.Probe.Activation = null;
        pane.Children.Remove(content);
        return new(content);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Content_Command_Uses_The_Derived_Content_Hooks(bool tools)
    {
        (LayoutContent content, Probe probe) = CreateContent(tools);
        LayoutDocumentPane pane = new(content);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        Window owner = new() { Content = manager, Width = 640, Height = 400 };
        owner.Show();
        HostWindow? host = null;
        try
        {
            content.Float();
            Dispatcher.UIThread.RunJobs();
            host = Assert.IsAssignableFrom<HostWindow>(Assert.Single(manager.Layout.FloatingWindows).DockWindow.Host);
            probe.CancelClose = true;
            probe.Closed = () =>
            {
                if (tools) { Assert.Null(content.Parent); Assert.Null(content.Root); }
                else { Assert.Same(host.Window!.Layout, Assert.IsType<LayoutDocumentFloatingWindow>(content.Parent).DockModel); Assert.Same(manager.Layout, content.Root); }
            };

            manager.GetLayoutItemFromModel(content)!.CloseCommand!.Execute(null);
            Assert.True(host.IsVisible);
            Assert.DoesNotContain("closed:hook", probe.Trace);
            probe.CancelClose = false;
            manager.GetLayoutItemFromModel(content)!.CloseCommand!.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsVisible);
            if (tools) Assert.Null(content.Parent);
            else Assert.IsType<LayoutDocumentFloatingWindow>(content.Parent);
            Assert.Null(content.Root);
            Assert.Single(probe.Trace, value => value == "closed:hook");
        }
        finally { probe.CancelClose = false; host?.Close(); owner.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Floating_Title_Bindings_Keep_The_Activated_Content_Title_Current(bool tools)
    {
        (LayoutContent content, _) = CreateContent(tools);
        content.Title = "Initial title";
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) } };
        Window owner = new() { Content = manager, Width = 640, Height = 400 };
        owner.Show();
        HostWindow? host = null;
        try
        {
            content.Float();
            Dispatcher.UIThread.RunJobs();
            host = Assert.IsAssignableFrom<HostWindow>(Assert.Single(manager.Layout.FloatingWindows).DockWindow.Host);
            Assert.Equal("Initial title", host.Title);

            content.Title = "Updated title";

            Assert.Equal("Updated title", host.Title);
        }
        finally { host?.Close(); owner.Close(); }
    }

    private static (LayoutContent, Probe) CreateContent(bool tools)
    {
        if (tools)
        {
            ProbeTool tool = new() { CanClose = true };
            return (tool, tool.Probe);
        }
        ProbeDocument document = new();
        return (document, document.Probe);
    }

    private sealed class Probe
    {
        internal bool CallBase { get; set; } = true;
        internal bool CancelClose { get; set; }
        internal List<string> Trace { get; } = new();
        internal List<(bool Old, bool New)> Selections { get; } = new();
        internal List<(bool Old, bool New)> Activations { get; } = new();
        internal Action<bool, bool>? Selection { get; set; }
        internal Action<bool, bool>? Activation { get; set; }
        internal Action? Closing { get; set; }
        internal Action? Closed { get; set; }
        internal void Select(bool oldValue, bool newValue)
        {
            Trace.Add("selection:hook");
            Selections.Add((oldValue, newValue));
            Selection?.Invoke(oldValue, newValue);
        }
        internal void Activate(bool oldValue, bool newValue)
        {
            Trace.Add("active:hook");
            Activations.Add((oldValue, newValue));
            Activation?.Invoke(oldValue, newValue);
        }
        internal void Close(CancelEventArgs args)
        {
            Trace.Add("closing:hook");
            args.Cancel |= CancelClose;
            Closing?.Invoke();
        }
        internal void CompleteClose()
        {
            Trace.Add("closed:hook");
            Closed?.Invoke();
        }
    }

    private sealed class ProbeDocument : LayoutDocument
    {
        internal Probe Probe { get; } = new();
        protected override void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            Probe.Select(oldValue, newValue);
            if (Probe.CallBase) base.OnIsSelectedChanged(oldValue, newValue);
        }
        protected override void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            Probe.Activate(oldValue, newValue);
            if (Probe.CallBase) base.OnIsActiveChanged(oldValue, newValue);
        }
        protected override void OnClosing(CancelEventArgs args)
        {
            Probe.Close(args);
            if (Probe.CallBase) base.OnClosing(args);
        }
        protected override void OnClosed()
        {
            Probe.CompleteClose();
            if (Probe.CallBase) base.OnClosed();
        }
    }

    private sealed class ProbeTool : LayoutAnchorable
    {
        internal Probe Probe { get; } = new();
        protected override void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            Probe.Select(oldValue, newValue);
            if (Probe.CallBase) base.OnIsSelectedChanged(oldValue, newValue);
        }
        protected override void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            Probe.Activate(oldValue, newValue);
            if (Probe.CallBase) base.OnIsActiveChanged(oldValue, newValue);
        }
        protected override void OnClosing(CancelEventArgs args)
        {
            Probe.Close(args);
            if (Probe.CallBase) base.OnClosing(args);
        }
        protected override void OnClosed()
        {
            Probe.CompleteClose();
            if (Probe.CallBase) base.OnClosed();
        }
    }
}
