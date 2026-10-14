using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class CustomDragSourcePreviewTests
{
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Custom_preview_uses_native_drag_lifetime_even_for_retained_panes(int ending)
    {
        using var workspace = new Workspace();
        var host = workspace.Host;
        var state = (DockControlState)host.DockControlState;
        var source = workspace.Pane.ActiveDockable!;
        var control = (Control)workspace.Factory.VisibleDockableControls[workspace.Pane];
        state.StartDrag(control, default, new Point(300, 200), host);
        Assert.Equal(1, workspace.Factory.Created);
        Assert.Same(workspace.Pane, workspace.Factory.Source);
        Assert.Same(host, workspace.Factory.Host);
        Assert.False(control.IsVisible);
        Assert.Same(workspace.Pane, source.Owner);
        Assert.Single(workspace.Pane.VisibleDockables!);

        if (ending == 0) workspace.Window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        else if (ending == 1) state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
        else if (ending == 2) state.Process(default, default, EventType.Moved, DragAction.Copy, host, new List<IDockControl> { host });
        else if (ending == 3) workspace.Window.Content = null;
        else if (ending == 4) { host.IsDockingEnabled = false; state.Process(default, default, EventType.Moved, DragAction.Move, host, new List<IDockControl> { host }); }
        else if (ending == 5) state.Process(default, default, EventType.Released, DragAction.Move, host, new List<IDockControl> { host });
        else
        {
            var transferred = new DockControl();
            var target = (DockControlState)transferred.DockControlState;
            Assert.True(state.TryTransferDragTo(target, host, transferred));
            target.Process(default, default, EventType.CaptureLost, DragAction.None, transferred, new List<IDockControl> { transferred });
        }
        Assert.True(control.IsVisible);
        Assert.Equal(1, workspace.Factory.Disposed);
        state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
        Assert.Equal(1, workspace.Factory.Disposed);
        Assert.Same(workspace.Pane, source.Owner);
    }

    [AvaloniaFact]
    public void Layout_changes_replace_the_custom_scope_and_null_opts_out()
    {
        using var workspace = new Workspace();
        var source = workspace.Pane.ActiveDockable!;
        using var preview = DockDragPreviewLayout.TryCreate(source, workspace.Host);
        Assert.NotNull(preview);
        Assert.True(preview!.IsCurrent);
        workspace.Pane.MinWidth = 50;
        Assert.False(preview.IsCurrent);
        preview.Dispose();
        workspace.Factory.Enabled = false;
        Assert.Null(DockDragPreviewLayout.TryCreate(source, workspace.Host));
        Assert.Equal(1, workspace.Factory.Created);
        Assert.Equal(1, workspace.Factory.Disposed);
    }

    [AvaloniaFact]
    public void Multi_item_panes_keep_native_tab_preview_and_selection()
    {
        using var workspace = new Workspace();
        var source = workspace.Pane.ActiveDockable!;
        var remaining = workspace.Factory.CreateTool();
        workspace.Factory.AddDockable(workspace.Pane, remaining);
        workspace.Window.UpdateLayout();
        using var preview = DockDragPreviewLayout.TryCreate(source, workspace.Host);
        Assert.NotNull(preview);
        Assert.Equal(0, workspace.Factory.Created);
        Assert.Same(remaining, workspace.Pane.ActiveDockable);
        preview!.Dispose();
        Assert.Same(source, workspace.Pane.ActiveDockable);
    }

    private sealed class PreviewFactory : Model.Mvvm.Factory, IDockDragSourcePreviewProvider
    {
        internal bool Enabled { get; set; } = true;
        internal int Created { get; private set; }
        internal int Disposed { get; private set; }
        internal IDockable? Source { get; private set; }
        internal DockControl? Host { get; private set; }

        public IDisposable? CreateDragSourcePreview(IDockable source, DockControl host)
        {
            if (!Enabled) return null;
            Created++;
            Source = source;
            Host = host;
            var pane = source as IDock ?? (IDock)source.Owner!;
            var control = (Control)VisibleDockableControls[pane];
            return new Scope(this, control.SetValue(Visual.IsVisibleProperty, false, BindingPriority.Animation)!);
        }

        private sealed class Scope(PreviewFactory factory, IDisposable visibility) : IDisposable
        {
            private bool _disposed;
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                visibility.Dispose();
                factory.Disposed++;
            }
        }
    }

    private sealed class Workspace : IDisposable
    {
        internal PreviewFactory Factory { get; } = new();
        internal IToolDock Pane { get; }
        internal DockControl Host { get; }
        internal Window Window { get; }

        internal Workspace()
        {
            var item = Factory.CreateTool();
            item.CanFloat = false;
            Pane = Factory.CreateToolDock();
            Pane.IsCollapsable = false;
            Pane.CanFloat = false;
            Pane.VisibleDockables = Factory.CreateList<IDockable>(item);
            Pane.ActiveDockable = item;
            var root = Factory.CreateRootDock();
            root.VisibleDockables = Factory.CreateList<IDockable>(Pane);
            root.ActiveDockable = Pane;
            Factory.InitLayout(root);
            Host = new DockControl { Layout = root };
            Window = new Window { Width = 800, Height = 500, Content = Host };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public void Dispose()
        {
            ((DockControlState)Host.DockControlState).Process(default, default, EventType.CaptureLost, DragAction.None, Host, new List<IDockControl> { Host });
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
