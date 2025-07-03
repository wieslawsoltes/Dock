using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable hidden event args.
/// </summary>
public class DockableHiddenEventArgs : EventArgs
{
    /// <summary>
    /// Gets hidden dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableHiddenEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The hidden dockable.</param>
    public DockableHiddenEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
