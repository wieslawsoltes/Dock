// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal sealed class GlobalDockingService : IGlobalDockingService
{
    public static readonly IGlobalDockingService Instance = new GlobalDockingService();

    public IDock? ResolveGlobalTargetDock(Control? dropControl)
    {
        if (DockSettings.GlobalDockingPreset == DockGlobalDockingPreset.LocalFirst)
        {
            if (dropControl?.DataContext is IDockable legacyDockable)
            {
                return legacyDockable as IDock ?? legacyDockable.Owner as IDock;
            }

            if (dropControl?.FindAncestorOfType<DockControl>()?.Layout?.ActiveDockable is IDock legacyActiveDock)
            {
                return legacyActiveDock;
            }

            return null;
        }

        if (dropControl?.DataContext is IDockable dockable)
        {
            return ResolveOutermostGlobalTargetDock(dockable) ?? dockable as IDock ?? dockable.Owner as IDock;
        }

        if (dropControl?.FindAncestorOfType<DockControl>()?.Layout?.ActiveDockable is IDock activeDock)
        {
            return ResolveOutermostGlobalTargetDock(activeDock) ?? activeDock;
        }

        return null;
    }

    public bool ShouldUseGlobalOperation(bool hasLocalAdorner, DockOperation localOperation, DockOperation globalOperation)
    {
        if (globalOperation == DockOperation.None)
        {
            return false;
        }

        if (DockSettings.GlobalDockingPreset == DockGlobalDockingPreset.LocalFirst)
        {
            if (!hasLocalAdorner)
            {
                return true;
            }

            return localOperation == DockOperation.Window;
        }

        return true;
    }

    public bool TryApplyGlobalDockingProportion(IDockable sourceDockable, IDockable? sourceRoot, IDockable? targetRoot, double proportion)
    {
        if (sourceRoot is null || targetRoot is null || sourceDockable.Owner is null)
        {
            return false;
        }

        sourceDockable.Owner.Proportion = proportion;
        sourceDockable.Owner.CollapsedProportion = proportion;
        return true;
    }

    private static IDock? ResolveOutermostGlobalTargetDock(IDockable? dockable)
    {
        var current = dockable;
        IDock? resolved = null;
        while (current is not null)
        {
            if (current is IDock dock && current is IGlobalTarget)
            {
                resolved = dock;
            }

            current = current.Owner;
        }

        return resolved;
    }
}
