// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal sealed class GlobalDockingService : IGlobalDockingService
{
    public static readonly IGlobalDockingService Instance = new GlobalDockingService();

    public IDock? ResolveGlobalTargetDock(Control? dropControl)
    {
        if (dropControl?.DataContext is IDockable dockable)
        {
            return dockable as IDock ?? dockable.Owner as IDock;
        }

        if (dropControl?.FindAncestorOfType<DockControl>()?.Layout?.ActiveDockable is IDock activeDock)
        {
            return activeDock;
        }

        return null;
    }

    public bool ShouldUseGlobalOperation(bool hasLocalAdorner, DockOperation localOperation, DockOperation globalOperation)
    {
        if (globalOperation == DockOperation.None)
        {
            return false;
        }

        if (!hasLocalAdorner)
        {
            return true;
        }

        return localOperation == DockOperation.Window;
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
}
