using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace Dock.Avalonia.Menus;

/// <summary>
/// Provides runtime access to Dock context menus.
/// </summary>
public static class ContextMenuManager
{
    /// <summary>
    /// Gets or sets the default menu provider used when no custom provider is registered.
    /// </summary>
    public static Func<string, ContextMenu?> DefaultProvider { get; set; } = key =>
        Application.Current?.FindResource(key) as ContextMenu;

    private static readonly Dictionary<string, Func<ContextMenu?>> _providers = new();
    private static readonly Dictionary<string, Action<ContextMenu>> _customizers = new();

    /// <summary>
    /// Registers a custom provider for a menu key.
    /// </summary>
    public static void RegisterProvider(string key, Func<ContextMenu?> provider)
    {
        _providers[key] = provider;
    }

    /// <summary>
    /// Registers a customizer that can modify the menu before it is displayed.
    /// </summary>
    public static void RegisterCustomizer(string key, Action<ContextMenu> customizer)
    {
        if (_customizers.TryGetValue(key, out var existing))
        {
            _customizers[key] = existing + customizer;
        }
        else
        {
            _customizers[key] = customizer;
        }
    }

    /// <summary>
    /// Gets a menu instance for the specified key.
    /// </summary>
    public static ContextMenu? GetMenu(string key)
    {
        ContextMenu? menu;
        if (_providers.TryGetValue(key, out var provider))
        {
            menu = provider();
        }
        else
        {
            menu = DefaultProvider(key);
        }

        if (menu != null && _customizers.TryGetValue(key, out var customize))
        {
            customize(menu);
        }

        return menu;
    }
}

