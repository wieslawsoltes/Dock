using System;
using Avalonia.Controls;

namespace AvalonDock.Themes;

/// <summary>Supplies an existing Avalonia resource dictionary as an AvalonDock theme.</summary>
public abstract class DictionaryTheme : Theme
{
    /// <summary>Creates a theme without a resource dictionary.</summary>
    public DictionaryTheme() { }

    /// <summary>Creates a theme backed by the supplied dictionary.</summary>
    /// <param name="themeResourceDictionary">Resources shared by managers and their auxiliary windows.</param>
    public DictionaryTheme(ResourceDictionary? themeResourceDictionary) => ThemeResourceDictionary = themeResourceDictionary;

    /// <summary>Gets the supplied dictionary, or null when none was supplied.</summary>
    public ResourceDictionary? ThemeResourceDictionary { get; private set; }

    /// <summary>Returns null because this theme supplies a dictionary directly.</summary>
    public override Uri? GetResourceUri() => null;
}
