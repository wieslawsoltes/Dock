using System;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class TabItemDragPolicyTests
{
    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    public void Native_Tabs_Use_The_Optional_Factory_Policy_And_Preserve_Default_Reordering(bool tool, int policy)
    {
        Factory factory = policy == 0 ? new Factory() : new PolicyFactory { AllowDrag = policy != 3, AllowReorder = policy != 2 };
        using Workspace workspace = new(tool, factory);
        workspace.PressAndMove();
        workspace.Release();
        Assert.Equal(policy < 2 ? new[] { workspace.Second, workspace.First } : new[] { workspace.First, workspace.Second }, workspace.Pane.VisibleDockables!);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void A_Policy_Change_Before_Release_Cancels_The_Pending_Move(bool tool, bool drag)
    {
        PolicyFactory factory = new();
        using Workspace workspace = new(tool, factory);
        workspace.PressAndMove();
        if (drag) factory.AllowDrag = false;
        else factory.AllowReorder = false;
        workspace.Release();
        Assert.Equal(new[] { workspace.First, workspace.Second }, workspace.Pane.VisibleDockables!);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Disabling_Drag_During_Pointer_Movement_Cancels_The_Gesture(bool tool)
    {
        PolicyFactory factory = new();
        using Workspace workspace = new(tool, factory);
        workspace.PressAndMove();
        factory.AllowDrag = false;
        workspace.MoveAgain();
        factory.AllowDrag = true;
        workspace.Release();
        Assert.Equal(new[] { workspace.First, workspace.Second }, workspace.Pane.VisibleDockables!);
    }

    private sealed class PolicyFactory : Factory, IDockItemDragPolicy
    {
        public bool AllowDrag { get; set; } = true;
        public bool AllowReorder { get; set; } = true;
        public bool CanDragItem(IDockable dockable) => AllowDrag;
        public bool CanReorderItem(IDockable dockable) => AllowReorder;
    }

    private sealed class Workspace : IDisposable
    {
        public IDockable First { get; }
        public IDockable Second { get; }
        public IDock Pane { get; }
        private readonly Window _window;
        private readonly Control _first;
        private readonly Control _second;
        private Point _end;

        public Workspace(bool tool, Factory factory)
        {
            First = tool ? new Tool { Id = "first", Title = "First tab" } : new Document { Id = "first", Title = "First tab" };
            Second = tool ? new Tool { Id = "second", Title = "Second tab" } : new Document { Id = "second", Title = "Second tab" };
            Pane = tool ? new ToolDock() : new DocumentDock();
            Pane.VisibleDockables = new AvaloniaList<IDockable> { First, Second };
            Pane.ActiveDockable = First;
            RootDock root = new() { VisibleDockables = new AvaloniaList<IDockable> { Pane }, ActiveDockable = Pane, DefaultDockable = Pane };
            factory.InitLayout(root);
            DockControl control = new() { Factory = factory, Layout = root };
            _window = new() { Content = control, Width = 800, Height = 350 };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
            _window.UpdateLayout();
            _first = control.GetVisualDescendants().OfType<TabStripItem>().Single(tab => ReferenceEquals(tab.DataContext, First));
            _second = control.GetVisualDescendants().OfType<TabStripItem>().Single(tab => ReferenceEquals(tab.DataContext, Second));
        }

        public void PressAndMove()
        {
            Point start = PointFor(_first);
            _end = PointFor(_second);
            _window.MouseMove(start);
            _window.MouseDown(start, MouseButton.Left);
            _window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            _window.MouseMove(_end, RawInputModifiers.LeftMouseButton);
        }
        public void MoveAgain() => _window.MouseMove(_end + new Vector(1, 0), RawInputModifiers.LeftMouseButton);
        public void Release() { _window.MouseUp(_end, MouseButton.Left); Dispatcher.UIThread.RunJobs(); }
        private Point PointFor(Control tab)
        {
            Control header = tab.GetVisualDescendants().OfType<TextBlock>().First(text => text.Text == ((IDockable)tab.DataContext!).Title);
            return header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), _window)!.Value;
        }
        public void Dispose() { _window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
