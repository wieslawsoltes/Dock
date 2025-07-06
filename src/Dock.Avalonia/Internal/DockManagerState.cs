using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal abstract class DockManagerState : IDockManagerState
{
    /// <inheritdoc/>
    public IDockManager DockManager { get; set; }

    protected Control? DropControl { get; set; }

    protected readonly AdornerHelper<DockTarget> LocalAdornerHelper = new();

    protected readonly AdornerHelper<GlobalDockTarget> GlobalAdornerHelper = new();
 
    /// <summary>
    /// Initializes a new instance of the <see cref="DockManagerState"/> class.
    /// </summary>
    /// <param name="dockManager">The dock manager.</param>
    protected DockManagerState(IDockManager dockManager)
    {
        DockManager = dockManager;
    }

    protected void AddAdorners(bool isValid)
    {
        // Local dock target
        if (isValid && DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            LocalAdornerHelper.AddAdorner(control);
        }

        // Global dock target
        // TODO: Handle isValid
        if (DropControl is { } dropControl)
        {
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is not null)
            {
                GlobalAdornerHelper.AddAdorner(dockControl);
            }
        }
    }

    protected void RemoveAdorners()
    {
        // Local dock target
        if (DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            LocalAdornerHelper.RemoveAdorner(control);
        }

        // Global dock target
        if (DropControl is { } dropControl)
        {
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is not null)
            {
                GlobalAdornerHelper.RemoveAdorner(dockControl);
            }
        }
    }

    protected static bool IsMinimumDragDistance(Vector diff)
    {
        return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
    }

    protected static bool IsMinimumDragDistance(PixelPoint diff)
    {
        return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
    }

    protected static void Float(Point point, DockControl inputActiveDockControl, IDockable dockable, IFactory factory)
    {
        var screen = inputActiveDockControl.PointToScreen(point);
        dockable.SetPointerScreenPosition(screen.X, screen.Y);
        factory.FloatDockable(dockable);
    }
}
