using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class OuterWindowDragTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (DockOperation operation in new[] { DockOperation.OuterLeft, DockOperation.OuterTop, DockOperation.OuterRight, DockOperation.OuterBottom })
            foreach (bool managed in new[] { false, true }) yield return new object[] { operation, managed };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Window_Drags_Expose_Outer_Targets_When_Tab_Docking_Is_Disabled(DockOperation operation, bool managed)
    {
        bool previous = DockSettings.UseFloatingDockAdorner;
        DockSettings.UseFloatingDockAdorner = false;
        PolicyFactory factory = new();
        DocumentDock destination = new() { VisibleDockables = factory.CreateList<IDockable>(new Document { Title = "Document" }) };
        ProportionalDock region = new() { VisibleDockables = factory.CreateList<IDockable>(destination) };
        factory.Region = region;
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(new ProportionalDock
            { VisibleDockables = factory.CreateList<IDockable>(region) }) };
        factory.InitLayout(root);
        root.ActiveDockable = root.VisibleDockables![0];
        destination.ActiveDockable = destination.VisibleDockables![0];
        DockControl control = new() { Layout = root };
        Window window = new() { Width = 900, Height = 600, Content = control };
        Tool first = new() { CanDockAsDocument = false };
        Tool second = new() { CanDockAsDocument = false };
        ToolDock pane = new() { VisibleDockables = factory.CreateList<IDockable>(first, second) };
        RootDock floatingRoot = new() { VisibleDockables = factory.CreateList<IDockable>(pane), ActiveDockable = pane, FocusedDockable = first };
        factory.InitLayout(floatingRoot);
        DockWindow model = new() { Factory = factory, Layout = floatingRoot };
        floatingRoot.Window = model;
        HostWindow host = new() { Width = 200, Height = 150, Position = new PixelPoint(1200, 0), Window = model };
        ManagedHostWindow managedHost = new() { Window = model };
        ControlTheme theme = new(typeof(DockTarget));
        theme.Setters.Add(new Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate((_, scope) =>
        {
            Border selector = new() { Width = 40, Height = 40 };
            Panel indicator = new() { IsHitTestVisible = false };
            DockProperties.SetIndicatorDockOperation(selector, operation);
            DockProperties.SetIndicatorDockOperation(indicator, operation);
            scope.Register("PART_" + operation + "Selector", selector);
            scope.Register("PART_" + operation + "Indicator", indicator);
            return new Panel { Children = { indicator, selector } };
        })));
        DockTarget.SetTargetTheme(control, theme);
        Action<PixelPoint, EventType>? process = null;
        try
        {
            window.Show(); window.UpdateLayout();
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            if (managed)
            {
                model.Host = managedHost;
                managedHost.Present(false);
                ManagedWindowRegistry.GetOrCreateDock(factory).VisibleDockables!.OfType<ManagedDockWindowDocument>()
                    .Single(document => ReferenceEquals(document.Window, model)).MdiBounds = new DockRect(1200, 0, 200, 150);
                process = new ManagedHostWindowState(new DockManager(new DockService()), managedHost).Process;
            }
            else
            {
                model.Host = host;
                host.Show();
                process = new HostWindowState(new DockManager(new DockService()), host).Process;
            }
            floatingRoot.ActiveDockable = pane;
            floatingRoot.FocusedDockable = first;
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Control[] surfaces = control.GetVisualDescendants().OfType<Control>().Where(child =>
                ReferenceEquals(child.DataContext, destination) && DockProperties.GetIsDockTarget(child) &&
                child.IsEffectivelyVisible && child.Bounds.Width > 10 && child.Bounds.Height > 10).ToArray();
            Assert.True(surfaces.Length > 0, string.Join("; ", control.GetVisualDescendants().OfType<Control>()
                .Select(child => child.GetType().Name + ":" + child.DataContext?.GetType().Name)));
            Control surface = surfaces[0];
            PixelPoint center = surface.PointToScreen(new Point(surface.Bounds.Width / 2, surface.Bounds.Height / 2));
            window.MouseMove(window.PointToClient(center));
            Assert.NotNull(DockHelpers.GetControlIncludingExternal(control, control.PointToClient(center), DockProperties.IsDropAreaProperty));
            PixelPoint origin = managed ? default : host.PointToScreen(default);
            PixelPoint offset = managed ? default : new PixelPoint(origin.X - host.Position.X, origin.Y - host.Position.Y);
            process(default, EventType.Pressed);
            PixelPoint move = new(center.X - offset.X, center.Y - offset.Y);
            process(move, EventType.Moved);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            DockTarget target = Assert.Single(window.GetVisualDescendants().OfType<DockTarget>());
            Control selector = Assert.IsAssignableFrom<Control>(target.GetSelector(operation));
            PixelPoint drop = selector.PointToScreen(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2));
            move = new(drop.X - offset.X, drop.Y - offset.Y);
            process(move, EventType.Moved);
            Assert.True(selector.Opacity > 0);
            process(move, EventType.Released);
            Assert.Same(region.Owner, pane.Owner);
            Assert.Same(pane, first.Owner);
            Assert.Equal(new IDockable[] { first, second }, pane.VisibleDockables);
        }
        finally
        {
            process?.Invoke(default, EventType.Released);
            managedHost.Exit(); host.Close(); window.Close();
            Dispatcher.UIThread.RunJobs();
            DockSettings.UseFloatingDockAdorner = previous;
        }
    }

    private sealed class PolicyFactory : Factory, IDockOuterSplitPolicy
    {
        internal IDock? Region { get; set; }
        public IDock? GetOuterSplitTarget(IDockable source, IDockable target, DockOperation operation) => Region;
    }
}
