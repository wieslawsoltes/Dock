using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable restored event args.
/// </summary>
public record DockableRestoredEventArgs(IDockable? Dockable) : EventArgs;
