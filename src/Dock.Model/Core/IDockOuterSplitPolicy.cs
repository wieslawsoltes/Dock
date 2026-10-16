// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Selects the layout region for an optional outer split target.</summary>
/// <remarks>Dock validates capabilities and moves the existing source dock as a unit.
/// Factories without this policy do not expose outer split operations.</remarks>
public interface IDockOuterSplitPolicy
{
    /// <summary>Resolves the hovered target to itself or an enclosing dock to split beside.</summary>
    /// <param name="source">The content or dock being dragged.</param>
    /// <param name="target">The hovered target.</param>
    /// <param name="operation">An OuterLeft, OuterTop, OuterRight or OuterBottom operation.</param>
    /// <returns>The target region, or null when this outer selector is unavailable.</returns>
    /// <remarks>Resolution must not mutate the layout; it also runs during pointer movement.</remarks>
    IDock? GetOuterSplitTarget(IDockable source, IDockable target, DockOperation operation);
}
