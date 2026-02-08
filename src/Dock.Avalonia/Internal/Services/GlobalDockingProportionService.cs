// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal sealed class GlobalDockingProportionService : IGlobalDockingProportionService
{
    public static readonly IGlobalDockingProportionService Instance = new GlobalDockingProportionService();

    public bool TryApply(IDockable sourceDockable, IDock targetDock, double proportion)
    {
        var sourceFactory = sourceDockable.Factory
            ?? (sourceDockable.Owner as IDock)?.Factory
            ?? targetDock.Factory;
        var targetFactory = targetDock.Factory ?? sourceFactory;

        var sourceRoot = sourceFactory?.FindRoot(sourceDockable, _ => true);
        var targetRoot = targetFactory?.FindRoot(targetDock, _ => true);

        if (sourceRoot is null || targetRoot is null || sourceDockable.Owner is null)
        {
            return false;
        }

        sourceDockable.Owner.Proportion = proportion;
        sourceDockable.Owner.CollapsedProportion = proportion;
        return true;
    }
}
