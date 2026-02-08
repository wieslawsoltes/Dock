// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal sealed class GlobalDockingProportionService : IGlobalDockingProportionService
{
    public static readonly IGlobalDockingProportionService Instance = new GlobalDockingProportionService();

    public bool TryApply(IDockable sourceDockable, IDockable? sourceRoot, IDockable? targetRoot, double proportion)
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
