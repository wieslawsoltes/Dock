// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal abstract class DockManagerState : IDockManagerState
{
    private readonly IDockManager _dockManager;

    protected IDockManager DockManager => _dockManager;

    protected Control? DropControl { get; set; }

    protected AdornerHelper<DockTarget> LocalAdornerHelper { get; }

    protected AdornerHelper<GlobalDockTarget> GlobalAdornerHelper { get; }
 
    /// <summary>
    /// Initializes a new instance of the <see cref="DockManagerState"/> class.
    /// </summary>
    /// <param name="dockManager">The dock manager.</param>
    protected DockManagerState(IDockManager dockManager)
    {
        _dockManager = dockManager;
        LocalAdornerHelper = new AdornerHelper<DockTarget>(DockSettings.UseFloatingDockAdorner);
        GlobalAdornerHelper = new AdornerHelper<GlobalDockTarget>(DockSettings.UseFloatingDockAdorner);
    }

    protected void AddAdorners(bool isLocalValid, bool isGlobalValid)
    {
        // Local dock target
        if (isLocalValid && DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            var host = DockProperties.GetDockAdornerHost(control) ?? control;
            var indicatorsOnly = DockProperties.GetShowDockIndicatorOnly(control);
            LocalAdornerHelper.AddAdorner(host, indicatorsOnly, true, true);
        }

        // Global dock target
        if (isGlobalValid && DropControl is { } dropControl)
        {
            bool horizontalGlobalDocking = true;
            bool verticalGlobalDocking = true;

            if (DropControl.DataContext is IDockable { Factory: { } factory } dockable)
            {
                var root = factory.FindRoot(dockable);

                if (root is { EnableAdaptiveGlobalDockTargets: true })
                {
                    (horizontalGlobalDocking, verticalGlobalDocking) = GlobalDockingHelper.CanGlobalDock(root);
                }
            }
            
            // Try to find DockControl ancestor - look through the visual tree more thoroughly
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            
            // If not found directly, walk up the visual tree manually
            if (dockControl is null)
            {
                var current = dropControl.GetVisualParent();
                while (current is not null)
                {
                    if (current is DockControl dc)
                    {
                        dockControl = dc;
                        break;
                    }
                    current = current.GetVisualParent();
                }
            }
            
            if (dockControl?.Layout?.ActiveDockable is IDock activeDock)
            {
                var targetDock = DockHelpers.FindProportionalDock(activeDock) ?? activeDock;
                // Only show global adorners if the target dock (or any ancestor) has global docking enabled
                if (DockInheritanceHelper.GetEffectiveEnableGlobalDocking(targetDock))
                {
                    var indicatorsOnly = DockProperties.GetShowDockIndicatorOnly(dropControl);
                    GlobalAdornerHelper.AddAdorner(dockControl, indicatorsOnly, horizontalGlobalDocking, verticalGlobalDocking);
                }
            }
        }
    }

    protected void RemoveAdorners()
    {
        // Local dock target
        if (DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            var host = DockProperties.GetDockAdornerHost(control) ?? control;
            LocalAdornerHelper.RemoveAdorner(host);
        }

        // Global dock target
        if (DropControl is { } dropControl)
        {
            // Try to find DockControl ancestor - look through the visual tree more thoroughly
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            
            // If not found directly, walk up the visual tree manually
            if (dockControl is null)
            {
                var current = dropControl.GetVisualParent();
                while (current is not null)
                {
                    if (current is DockControl dc)
                    {
                        dockControl = dc;
                        break;
                    }
                    current = current.GetVisualParent();
                }
            }
            
            if (dockControl is not null)
            {
                GlobalAdornerHelper.RemoveAdorner(dockControl);
            }
        }
    }

    protected virtual void Execute(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo, IDockable sourceDockable, IDockable targetDockable)
    {
        _dockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            return;
        }

        var relativePoint = DockHelpers.GetScreenPoint(relativeTo, point);
        _dockManager.ScreenPosition = DockHelpers.ToDockPoint(relativePoint);

        _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: true);
    }

    protected bool ValidateDockable(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo, IDockable sourceDockable)
    {
        if (DropControl?.DataContext is not IDockable targetDockable)
        {
            return false;
        }

        if (!ValidateLocalTarget(sourceDockable, targetDockable))
        {
            return false;
        }

        _dockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            return false;
        }

        var screenPoint = DockHelpers.GetScreenPoint(relativeTo, point);
        _dockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

        return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
    }

    protected bool ValidateLocalTarget(IDockable sourceDockable, IDockable targetDockable)
    {
        if (targetDockable is not ILocalTarget)
        {
            return false;
        }

        // Validate docking groups according to business rules
        return DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable);
    }

    protected bool ValidateGlobalTarget(IDockable sourceDockable, IDockable targetDockable)
    {
        // Validate both interface and docking groups
        if (targetDockable is not IGlobalTarget)
        {
            return false;
        }

        // For global targets, use special validation that allows non-grouped dockables
        if (targetDockable is IDock targetDock)
        {
            return DockGroupValidator.ValidateGlobalDocking(sourceDockable, targetDock);
        }

        // Fallback to standard validation for non-dock targets
        return DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable);
    }

    protected static void Float(Point point, DockControl inputActiveDockControl, IDockable dockable, IFactory factory)
    {
        var screen = inputActiveDockControl.PointToScreen(point);
        dockable.SetPointerScreenPosition(screen.X, screen.Y);
        factory.FloatDockable(dockable);
    }
}
