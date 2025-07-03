using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable restored event args.
/// </summary>
public class DockableRestoredEventArgs : EventArgs
{
    /// <summary>
    /// Gets restored dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockableRestoredEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The restored dockable.</param>
    public DockableRestoredEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
