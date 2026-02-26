using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Dock.Avalonia.Contract;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ExternalDockSurfaceTests
{
    [AvaloniaFact]
    public void RegisterExternalDockSurface_SetsOwner()
    {
        var dockControl = new DockControl();
        var externalSurface = new TestExternalSurfaceControl();

        dockControl.RegisterExternalDockSurface(externalSurface);

        Assert.Same(dockControl, externalSurface.DockControl);
    }

    [AvaloniaFact]
    public void UnregisterExternalDockSurface_ClearsOwner()
    {
        var dockControl = new DockControl();
        var externalSurface = new TestExternalSurfaceControl();
        dockControl.RegisterExternalDockSurface(externalSurface);

        var removed = dockControl.UnregisterExternalDockSurface(externalSurface);

        Assert.True(removed);
        Assert.Null(externalSurface.DockControl);
    }

    [AvaloniaFact]
    public void RegisterExternalDockSurface_MovesSurfaceBetweenOwners()
    {
        var firstDockControl = new DockControl();
        var secondDockControl = new DockControl();
        var externalSurface = new TestExternalSurfaceControl();

        firstDockControl.RegisterExternalDockSurface(externalSurface);
        secondDockControl.RegisterExternalDockSurface(externalSurface);

        Assert.Null(firstDockControl.EnumerateExternalDockSurfaceControls().FirstOrDefault());
        Assert.Same(secondDockControl, externalSurface.DockControl);
    }

    [AvaloniaFact]
    public void ResolveDockControl_UsesRegisteredExternalSurfaceOwner()
    {
        var dockControl = new DockControl
        {
            Width = 120,
            Height = 80
        };
        var externalSurface = new TestExternalSurfaceControl
        {
            Width = 140,
            Height = 90
        };
        var nestedControl = new Border
        {
            Width = 40,
            Height = 20
        };
        externalSurface.Children.Add(nestedControl);

        var window = ShowInSharedWindow(dockControl, externalSurface);
        try
        {
            dockControl.RegisterExternalDockSurface(externalSurface);

            var resolved = DockHelpers.ResolveDockControl(nestedControl);

            Assert.Same(dockControl, resolved);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GetControlIncludingExternal_FindsDropAreaOnRegisteredSurface()
    {
        var dockControl = new DockControl
        {
            Width = 120,
            Height = 80
        };
        var externalSurface = new TestExternalSurfaceControl
        {
            Width = 140,
            Height = 90,
            Background = Brushes.Transparent
        };
        DockProperties.SetIsDropArea(externalSurface, true);
        DockProperties.SetIsDockTarget(externalSurface, true);

        var window = ShowInSharedWindow(dockControl, externalSurface);
        try
        {
            dockControl.RegisterExternalDockSurface(externalSurface);

            var dropAreaScreenPoint = externalSurface.PointToScreen(new Point(10, 10));
            var dockPoint = dockControl.PointToClient(dropAreaScreenPoint);
            var hit = DockHelpers.GetControlIncludingExternal(dockControl, dockPoint, DockProperties.IsDropAreaProperty);

            Assert.Same(externalSurface, hit);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DetachedDockControl_ClearsRegisteredExternalSurfaceOwners()
    {
        var dockControl = new DockControl
        {
            Width = 120,
            Height = 80
        };
        var externalSurface = new TestExternalSurfaceControl
        {
            Width = 140,
            Height = 90
        };

        var window = ShowInSharedWindow(dockControl, externalSurface);
        dockControl.RegisterExternalDockSurface(externalSurface);
        Assert.Same(dockControl, externalSurface.DockControl);

        window.Close();

        Assert.Null(externalSurface.DockControl);
    }

    private static Window ShowInSharedWindow(DockControl dockControl, Control externalSurface)
    {
        var canvas = new Canvas
        {
            Width = 500,
            Height = 300
        };
        Canvas.SetLeft(dockControl, 20);
        Canvas.SetTop(dockControl, 20);
        Canvas.SetLeft(externalSurface, 220);
        Canvas.SetTop(externalSurface, 40);
        canvas.Children.Add(dockControl);
        canvas.Children.Add(externalSurface);

        var window = new Window
        {
            Width = 600,
            Height = 400,
            Content = canvas
        };

        window.Show();
        window.UpdateLayout();
        canvas.UpdateLayout();
        dockControl.UpdateLayout();
        externalSurface.UpdateLayout();
        return window;
    }

    private sealed class TestExternalSurfaceControl : Canvas, IExternalDockSurface
    {
        public DockControl? DockControl { get; set; }

        public Control SurfaceControl => this;
    }
}
