// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>Describes an ownership transition before the dockable changes its owner.</summary>
public class DockableOwnerChangingEventArgs : EventArgs
{
    /// <summary>Creates notification data for an ownership transition.</summary>
    /// <param name="oldOwner">The current owner, or null for detached content.</param>
    /// <param name="newOwner">The requested owner, or null when detaching.</param>
    public DockableOwnerChangingEventArgs(IDockable? oldOwner, IDockable? newOwner)
    {
        OldOwner = oldOwner;
        NewOwner = newOwner;
    }

    /// <summary>Gets the current owner before the transition.</summary>
    public IDockable? OldOwner { get; }
    /// <summary>Gets the requested owner.</summary>
    public IDockable? NewOwner { get; }
}
