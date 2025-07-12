using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Menus;

/// <summary>
/// Markup extension that resolves context menus through <see cref="ContextMenuManager"/>.
/// </summary>
public class MenuExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuExtension"/> class.
    /// </summary>
    public MenuExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuExtension"/> class.
    /// </summary>
    /// <param name="key">The menu key.</param>
    public MenuExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets or sets the menu key.
    /// </summary>
    [ConstructorArgument("key")]
    public string? Key { get; set; }

    /// <summary>
    /// Provides the context menu instance.
    /// </summary>
    public object? ProvideValue(IServiceProvider serviceProvider)
    {
        return Key is null ? null : ContextMenuManager.GetMenu(Key);
    }
}

