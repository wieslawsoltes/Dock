using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable hidden event args.
/// </summary>
public record DockableHiddenEventArgs(IDockable? Dockable) : EventArgs;
