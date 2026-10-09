using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class WindowDragLifecycleTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Interrupted_Drag_Removes_Targets_And_Can_Restart(bool newPress)
    {
        using var drag = new DragScenario();
        drag.Start();
        drag.EnterTarget();
        var targets = drag.GetTargets();
        var detachCallbacks = 0;
        foreach (var target in targets)
        {
            target.DetachedFromVisualTree += (_, _) =>
            {
                detachCallbacks++;
                drag.MoveOverTarget();
            };
        }

        drag.State.Process(default, newPress ? EventType.Pressed : EventType.CaptureLost);

        drag.AssertRemoved(targets);
        Assert.Equal(2, detachCallbacks);
        if (!newPress)
        {
            drag.EnterTarget(expectTargets: false);
        }
        drag.Start();
        drag.EnterTarget();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Release_Removes_Targets_After_Target_Is_Invalidated(bool detachTarget)
    {
        using var drag = new DragScenario();
        drag.Start();
        drag.EnterTarget();
        var targets = drag.GetTargets();
        DockProperties.SetIsDropEnabled(drag.DropArea, false);
        DockProperties.SetIsDockTarget(drag.DropArea, false);
        if (detachTarget)
        {
            drag.TargetPanel.Children.Remove(drag.DropArea);
        }

        drag.State.Process(default, EventType.Released);

        drag.AssertRemoved(targets);
        drag.EnterTarget(expectTargets: false);
    }

    [AvaloniaFact]
    public void Detaching_Helper_Cancels_Targets_And_Replacement_Can_Drag()
    {
        using var drag = new DragScenario();
        drag.StartWithHelper();
        drag.EnterTarget();
        var targets = drag.GetTargets();
        drag.Factory.WindowMoveDragEnd += (_, _) => drag.EnterTarget(expectTargets: false);

        drag.Helper.Detach();
        drag.Helper.Detach();

        drag.AssertRemoved(targets);
        Assert.Equal(1, drag.Factory.DragEnds);
        drag.ReplaceHelper();
        drag.StartWithHelper();
        drag.EnterTarget();
        Assert.Equal(2, drag.Factory.DragBegins);
    }

    [AvaloniaFact]
    public void New_Press_Recovers_After_Missing_Release()
    {
        using var drag = new DragScenario();
        drag.StartWithHelper();
        drag.EnterTarget();
        var targets = drag.GetTargets();

        drag.Press();

        drag.AssertRemoved(targets);
        Assert.Equal(1, drag.Factory.DragEnds);
        drag.MovePointer();
        drag.EnterTarget();
        Assert.Equal(2, drag.Factory.DragBegins);
    }

    [AvaloniaFact]
    public void Closing_Dragged_Window_Removes_Targets()
    {
        using var drag = new DragScenario();
        drag.StartWithHelper();
        drag.EnterTarget();
        var targets = drag.GetTargets();

        drag.Source.Close();

        drag.AssertRemoved(targets);
        drag.EnterTarget(expectTargets: false);
        Assert.Equal(1, drag.Factory.DragEnds);
    }

    [AvaloniaFact]
    public void Handled_Release_Still_Completes_Window_Drag()
    {
        using var drag = new DragScenario();
        drag.StartWithHelper();
        drag.EnterTarget();
        var targets = drag.GetTargets();
        // Prevent docking so this test only exercises release routing and cleanup.
        DockProperties.SetIsDropEnabled(drag.DropArea, false);

        drag.Release(handled: true);

        drag.AssertRemoved(targets);
        drag.EnterTarget(expectTargets: false);
        Assert.Equal(1, drag.Factory.DragEnds);
        drag.StartWithHelper();
        DockProperties.SetIsDropEnabled(drag.DropArea, true);
        drag.EnterTarget();
    }

    [AvaloniaFact]
    public void Release_Callback_Can_Close_Window_After_Cleanup()
    {
        using var drag = new DragScenario();
        drag.StartWithHelper();
        drag.EnterTarget();
        var targets = drag.GetTargets();
        DockProperties.SetIsDropEnabled(drag.DropArea, false);
        drag.Factory.WindowMoveDragEnd += (_, _) =>
        {
            drag.AssertRemoved(targets);
            drag.Source.Close();
        };

        drag.Release();

        Assert.Equal(1, drag.Factory.DragEnds);
        drag.AssertRemoved(targets);
    }

    private sealed class DragScenario : IDisposable
    {
        private readonly Window _targetWindow;
        private readonly Border _grip = new() { Background = Brushes.Transparent };
        private readonly Pointer _pointer = new(1, PointerType.Mouse, true);
        private PixelVector _pressOffset;

        public TrackingFactory Factory { get; } = new();
        public HostWindow Source { get; }
        public HostWindowState State => (HostWindowState)Source.HostWindowState;
        public Border DropArea { get; }
        public Panel TargetPanel { get; }
        public WindowDragHelper Helper { get; private set; }
        public AdornerLayer Layer { get; }

        public DragScenario()
        {
            var targetRoot = CreateLayout("Target");
            DropArea = new Border { DataContext = targetRoot.ActiveDockable, Background = Brushes.Transparent };
            DockProperties.SetIsDropArea(DropArea, true);
            DockProperties.SetIsDockTarget(DropArea, true);
            TargetPanel = new Panel { Children = { DropArea } };
            var target = new DockControl
            {
                Factory = Factory,
                Layout = targetRoot,
                InitializeFactory = false,
                InitializeLayout = false,
                Template = new FuncControlTemplate<DockControl>((_, _) => TargetPanel)
            };
            _targetWindow = new Window
            {
                Content = target, Width = 600, Height = 400, Position = new PixelPoint(1000, 1000)
            };
            _targetWindow.Show();
            _targetWindow.UpdateLayout();
            Assert.Contains(target, Factory.DockControls);
            Layer = AdornerLayer.GetAdornerLayer(DropArea)!;
            Assert.NotNull(Layer);

            var sourceRoot = CreateLayout("Source");
            var dockWindow = new DockWindow { Factory = Factory, Layout = sourceRoot };
            sourceRoot.Window = dockWindow;
            Source = new HostWindow
            {
                Window = dockWindow,
                Template = new FuncControlTemplate<HostWindow>((_, _) => _grip),
                Width = 300, Height = 200, Position = new PixelPoint(100, 100)
            };
            Source.Show();
            Source.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Assert.Same(Source, TopLevel.GetTopLevel(_grip));
            Helper = CreateHelper();
        }

        private RootDock CreateLayout(string title)
        {
            var document = new Document { Title = title };
            var dock = new DocumentDock
            {
                VisibleDockables = Factory.CreateList<IDockable>(document), ActiveDockable = document
            };
            var root = new RootDock
            {
                VisibleDockables = Factory.CreateList<IDockable>(dock), ActiveDockable = dock,
                FocusedDockable = document
            };
            Factory.InitLayout(root);
            return root;
        }

        private WindowDragHelper CreateHelper()
        {
            var helper = new WindowDragHelper(_grip, () => true, _ => true);
            helper.Attach();
            return helper;
        }

        public void ReplaceHelper() => Helper = CreateHelper();

        public void Start()
        {
            _pressOffset = Source.PointToScreen(default) - Source.Position;
            State.Process(default, EventType.Pressed);
        }

        public void StartWithHelper()
        {
            var begins = Factory.DragBegins;
            Press();
            MovePointer();
            Assert.Equal(begins + 1, Factory.DragBegins);
        }

        public void Press()
        {
            var pressPoint = new Point(10, 10);
            _pressOffset = Source.PointToScreen(pressPoint) - Source.Position;
            _grip.RaiseEvent(new PointerPressedEventArgs(_grip, _pointer, Source, pressPoint, 0,
                new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
                KeyModifiers.None, 1));
        }

        public void MovePointer()
        {
            _grip.RaiseEvent(new PointerEventArgs(InputElement.PointerMovedEvent, _grip, _pointer, Source,
                new Point(40, 40), 0,
                new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other),
                KeyModifiers.None));
        }

        public void Release(bool handled = false)
        {
            _grip.RaiseEvent(new PointerReleasedEventArgs(_grip, _pointer, Source, new Point(40, 40), 0,
                new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
                KeyModifiers.None, MouseButton.Left) { Handled = handled });
        }

        public void EnterTarget(bool expectTargets = true)
        {
            MoveOverTarget();
            Assert.Equal(expectTargets ? 2 : 0, GetTargets().Length);
        }

        public void MoveOverTarget()
        {
            var desiredPointer = _targetWindow.PointToScreen(new Point(80, 80));
            State.Process(desiredPointer - _pressOffset, EventType.Moved);
        }

        public Control[] GetTargets() => Layer.Children.OfType<DockTargetBase>().Cast<Control>().ToArray();

        public void AssertRemoved(Control[] targets)
        {
            Assert.Equal(2, targets.Length);
            Assert.Empty(GetTargets());
            foreach (var target in targets)
            {
                Assert.Null(target.GetVisualParent());
                Assert.Null(target.Parent);
                Assert.Null(AdornerLayer.GetAdornedElement(target));
            }
        }

        public void Dispose()
        {
            Helper.Detach();
            Source.Close();
            _targetWindow.Close();
        }
    }

    private sealed class TrackingFactory : Factory
    {
        public int DragBegins { get; private set; }
        public int DragEnds { get; private set; }

        public override bool OnWindowMoveDragBegin(IDockWindow? window)
        {
            DragBegins++;
            return base.OnWindowMoveDragBegin(window);
        }

        public override void OnWindowMoveDragEnd(IDockWindow? window)
        {
            DragEnds++;
            base.OnWindowMoveDragEnd(window);
        }
    }
}
