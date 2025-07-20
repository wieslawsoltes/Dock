using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable property changed event args.
/// </summary>
public class DockablePropertyChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets changed dockable.
    /// </summary>
    public IDockable? Dockable { get; }

    /// <summary>
    /// Gets changed property name.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockablePropertyChangedEventArgs"/> class.
    /// </summary>
    /// <param name="dockable">The dockable that changed.</param>
    /// <param name="propertyName">The changed property name.</param>
    public DockablePropertyChangedEventArgs(IDockable? dockable, string? propertyName)
    {
        Dockable = dockable;
        PropertyName = propertyName;
    }
}
