// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Optional factory policy for validating docking destinations.</summary>
/// <remarks>DockManager checks participating factories before showing targets or executing a drop.
/// Implementations must not mutate the layout. Factories without this policy retain native behavior;
/// direct factory collection operations do not consult the policy.</remarks>
public interface IDockDropPolicy
{
    /// <summary>Determines whether a source may dock at a destination using the requested operation.</summary>
    /// <param name="source">The content or group being dragged.</param>
    /// <param name="target">The proposed destination.</param>
    /// <param name="operation">The requested docking or floating operation.</param>
    /// <returns>True to continue native validation; false to reject the destination.</returns>
    bool CanDrop(IDockable source, IDockable target, DockOperation operation);
}
