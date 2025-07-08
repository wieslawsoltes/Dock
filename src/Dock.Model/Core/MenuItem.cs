namespace Dock.Model.Core;

using System.Collections.Generic;
using System.Windows.Input;

/// <summary>
/// Represents a menu item used for context menus and flyouts.
/// </summary>
public interface IMenuItem
{
    /// <summary>Gets or sets item header.</summary>
    string Header { get; set; }

    /// <summary>Gets or sets command executed by the item.</summary>
    ICommand? Command { get; set; }

    /// <summary>Gets or sets command parameter.</summary>
    object? CommandParameter { get; set; }

    /// <summary>Gets or sets a value indicating whether the item is enabled.</summary>
    bool IsEnabled { get; set; }

    /// <summary>Gets or sets a value indicating whether the item is visible.</summary>
    bool IsVisible { get; set; }

    /// <summary>Gets collection of child menu items.</summary>
    IList<IMenuItem>? Items { get; set; }
}

/// <summary>
/// Default implementation of <see cref="IMenuItem"/>.
/// </summary>
public class MenuItem : IMenuItem
{
    /// <inheritdoc />
    public string Header { get; set; } = string.Empty;

    /// <inheritdoc />
    public ICommand? Command { get; set; }

    /// <inheritdoc />
    public object? CommandParameter { get; set; }

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool IsVisible { get; set; } = true;

    /// <inheritdoc />
    public IList<IMenuItem>? Items { get; set; }
}

