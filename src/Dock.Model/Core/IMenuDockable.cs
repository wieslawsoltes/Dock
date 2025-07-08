namespace Dock.Model.Core;

using System.Collections.Generic;

/// <summary>
/// Provides a collection of menu items for context menus.
/// </summary>
public interface IMenuDockable
{
    /// <summary>Gets collection of menu items.</summary>
    IList<IMenuItem> MenuItems { get; }
}

