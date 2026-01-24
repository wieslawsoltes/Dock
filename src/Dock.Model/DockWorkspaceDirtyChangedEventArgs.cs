// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model;

/// <summary>
/// Workspace dirty state change event args.
/// </summary>
public sealed class DockWorkspaceDirtyChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the workspace associated with the dirty state change.
    /// </summary>
    public DockWorkspace? Workspace { get; }

    /// <summary>
    /// Gets whether the workspace or layout is dirty.
    /// </summary>
    public bool IsDirty { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockWorkspaceDirtyChangedEventArgs"/> class.
    /// </summary>
    /// <param name="workspace">The workspace associated with the change.</param>
    /// <param name="isDirty">The dirty state.</param>
    public DockWorkspaceDirtyChangedEventArgs(DockWorkspace? workspace, bool isDirty)
    {
        Workspace = workspace;
        IsDirty = isDirty;
    }
}
