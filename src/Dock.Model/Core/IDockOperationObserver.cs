// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core;

/// <summary>
/// Optional factory contract for observing an executed docking operation as a whole.
/// </summary>
/// <remarks>
/// <see cref="DockManager.ValidateDockable"/> opens a scope on the source's factory.
/// Validation and rejected operations do not open a scope. Composite operations may
/// open nested scopes. Observers do not replace Dock's validation or movement logic.
/// </remarks>
public interface IDockOperationObserver
{
    /// <summary>Begins observing a validated operation before its first layout mutation.</summary>
    /// <param name="source">The content or group being moved.</param>
    /// <param name="target">The requested docking target.</param>
    /// <param name="operation">The requested docking operation.</param>
    /// <returns>A scope, or null when no observation is needed.</returns>
    IDockOperationScope? BeginDockOperation(IDockable source, IDockable target, DockOperation operation);
}

/// <summary>Observes completion and releases state for one docking operation.</summary>
public interface IDockOperationScope : IDisposable
{
    /// <summary>
    /// Reports that execution succeeded. Disposal alone does not report completion,
    /// including when execution throws or returns false.
    /// </summary>
    void Complete();
}
