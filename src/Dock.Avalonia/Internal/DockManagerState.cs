// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal abstract class DockManagerState : IDockManagerState
{
    private readonly IDockManager _dockManager;
    private Control? _globalAdornerHost;

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

    [Conditional("DEBUG")]
    protected static void LogDropRejection(string stage, string message)
    {
    DockLogger.LogDebug(stage, message);
    }

    [Conditional("DEBUG")]
    protected static void LogDragState(string message)
    {
    DockLogger.LogDebug("DragState", message);
    }

    protected void AddAdorners(bool isLocalValid, bool isGlobalValid)
    {
        // Local dock target
        if (isLocalValid && DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            var host = DockProperties.GetDockAdornerHost(control) ?? control;
            var indicatorsOnly = DockProperties.GetShowDockIndicatorOnly(control);
            var allowHorizontalDocking = true;
            var allowVerticalDocking = true;

            if (control.DataContext is IRootDock { OpenedDockablesCount: 0 })
            {
                allowHorizontalDocking = false;
                allowVerticalDocking = false;
            }

            LocalAdornerHelper.AddAdorner(host, indicatorsOnly, allowHorizontalDocking, allowVerticalDocking);
            LocalAdornerHelper.SetGlobalDockAvailability(isGlobalValid);
            LocalAdornerHelper.SetGlobalDockActive(false);
        }
        else
        {
            LocalAdornerHelper.SetGlobalDockAvailability(false);
            LocalAdornerHelper.SetGlobalDockActive(false);
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
            
            var dropControlType = dropControl.GetType().Name;
            var dropDataContextType = dropControl.DataContext?.GetType().Name ?? "null";
            DockLogger.LogDebug(
                "GlobalAdorner",
                $"Evaluating global adorners for drop control '{dropControlType}' (DataContext={dropDataContextType}) " +
                $"with horizontal={horizontalGlobalDocking}, vertical={verticalGlobalDocking}.");

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
                    _globalAdornerHost = dockControl;
                    var dockControlName = dockControl.GetType().Name;
                    var targetDockTitle = targetDock.Title ?? targetDock.GetType().Name;
                    DockLogger.LogDebug(
                        "GlobalAdorner",
                        $"Added global adorners to dock control '{dockControlName}' targeting dock '{targetDockTitle}'.");
                }
                else
                {
                    var targetDockTitle = targetDock.Title ?? targetDock.GetType().Name;
                    DockLogger.LogDebug(
                        "GlobalAdorner",
                        $"Skipped global adorners because EnableGlobalDocking is disabled for '{targetDockTitle}'.");
                }
            }
            else
            {
                DockLogger.LogDebug(
                    "GlobalAdorner",
                    $"Unable to resolve dock control host for drop control '{dropControlType}'. Global adorners not added.");
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
        LocalAdornerHelper.SetGlobalDockAvailability(false);
        LocalAdornerHelper.SetGlobalDockActive(false);

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
                var dockControlName = dockControl.GetType().Name;
                GlobalAdornerHelper.RemoveAdorner(dockControl);
                if (ReferenceEquals(_globalAdornerHost, dockControl))
                {
                    _globalAdornerHost = null;
                }
                DockLogger.LogDebug(
                    "GlobalAdorner",
                    $"Removed global adorners from dock control '{dockControlName}'.");
            }
            else
            {
                var dropControlType = dropControl.GetType().Name;
                if (_globalAdornerHost is { } cachedHost)
                {
                    GlobalAdornerHelper.RemoveAdorner(cachedHost);
                    DockLogger.LogDebug(
                        "GlobalAdorner",
                        $"Used cached host '{cachedHost.GetType().Name}' to remove global adorners after drop control '{dropControlType}' was detached.");
                    _globalAdornerHost = null;
                }
                else
                {
                    DockLogger.LogDebug(
                        "GlobalAdorner",
                        $"Drop control '{dropControlType}' no longer has a dock control ancestor and no cached host; global adorners may already be detached.");
                }
            }
        }
        else if (_globalAdornerHost is { } cachedHost)
        {
            GlobalAdornerHelper.RemoveAdorner(cachedHost);
            DockLogger.LogDebug(
                "GlobalAdorner",
                $"Removed global adorners using cached host '{cachedHost.GetType().Name}' after DropControl was cleared.");
            _globalAdornerHost = null;
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
            LogDropRejection(nameof(ValidateDockable), $"DropControl data context is not IDockable (control={DropControl?.GetType().Name}).");
            return false;
        }

        if (!ValidateLocalTarget(sourceDockable, targetDockable))
        {
            return false;
        }

        _dockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            LogDropRejection(nameof(ValidateDockable), "Relative visual is not attached to a visual root.");
            return false;
        }

        var screenPoint = DockHelpers.GetScreenPoint(relativeTo, point);
        _dockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

        var isValid = _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
        if (!isValid)
        {
            LogDropRejection(
                nameof(ValidateDockable),
                $"DockManager rejected operation {operation} from '{sourceDockable.Title}' to '{targetDockable.Title}'.");
        }

        return isValid;
    }

    protected bool ValidateLocalTarget(IDockable sourceDockable, IDockable targetDockable)
    {
        if (targetDockable is not ILocalTarget)
        {
            LogDropRejection(
                nameof(ValidateLocalTarget),
                $"Target '{targetDockable.Title}' does not implement {nameof(ILocalTarget)}.");
            return false;
        }

        // Validate docking groups according to business rules
        var isValid = DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable);
        if (!isValid)
        {
            LogDropRejection(
                nameof(ValidateLocalTarget),
                $"Dock group validation failed for '{sourceDockable.Title}' -> '{targetDockable.Title}'.");
        }

        return isValid;
    }

    protected bool ValidateGlobalTarget(IDockable sourceDockable, IDockable targetDockable)
    {
        // Validate both interface and docking groups
        if (targetDockable is not IGlobalTarget)
        {
            LogDropRejection(
                nameof(ValidateGlobalTarget),
                $"Target '{targetDockable.Title}' does not implement {nameof(IGlobalTarget)}.");
            return false;
        }

        // For global targets, use special validation that allows non-grouped dockables
        if (targetDockable is IDock targetDock)
        {
            var isValid = DockGroupValidator.ValidateGlobalDocking(sourceDockable, targetDock);
            if (!isValid)
            {
                LogDropRejection(
                    nameof(ValidateGlobalTarget),
                    $"Global docking validation failed for '{sourceDockable.Title}' -> '{targetDock.Title}'.");
            }

            return isValid;
        }

        // Fallback to standard validation for non-dock targets
        var defaultValid = DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable);
        if (!defaultValid)
        {
            LogDropRejection(
                nameof(ValidateGlobalTarget),
                $"Dock group validation failed for '{sourceDockable.Title}' -> '{targetDockable.Title}'.");
        }

        return defaultValid;
    }

    protected static void Float(Point point, DockControl inputActiveDockControl, IDockable dockable, IFactory factory)
    {
        var screen = inputActiveDockControl.PointToScreen(point);
        dockable.SetPointerScreenPosition(screen.X, screen.Y);
        factory.FloatDockable(dockable);
    }
}
