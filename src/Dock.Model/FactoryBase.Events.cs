using System;
using Dock.Model.Core;
using Dock.Model.Core.Events;

namespace Dock.Model
{
    /// <summary>
    /// Factory base class.
    /// </summary>
    public abstract partial class FactoryBase
    {
        /// <inheritdoc />
        public event EventHandler<ActiveDockableChangedEventArgs>? ActiveDockableChanged;

        /// <inheritdoc />
        public event EventHandler<FocusedDockableChangedEventArgs>? FocusedDockableChanged;

        /// <inheritdoc />
        public virtual void OnActiveDockableChanged(IDockable? dockable)
        {
            ActiveDockableChanged?.Invoke(this, new ActiveDockableChangedEventArgs(dockable));
        }

        /// <inheritdoc />
        public virtual void OnFocusedDockableChanged(IDockable? dockable)
        {
            FocusedDockableChanged?.Invoke(this, new FocusedDockableChangedEventArgs(dockable));
        }
    }
}
