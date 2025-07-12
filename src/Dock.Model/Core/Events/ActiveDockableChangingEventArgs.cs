using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Active dockable changing event args.
/// </summary>
public class ActiveDockableChangingEventArgs : EventArgs
{
    /// <summary>
    /// Gets dockable that is about to become active.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets or sets flag indicating whether active dockable change should be canceled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes new instance of the <see cref="ActiveDockableChangingEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The dockable that will become active.</param>
    public ActiveDockableChangingEventArgs(IDockable? dockable)
    {
        Dockable = dockable;
    }
}
