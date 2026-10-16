using System;
using Avalonia;

namespace AvalonDock.Themes;

/// <summary>Describes the resources applied to a manager and its auxiliary windows.</summary>
public abstract class Theme : AvaloniaObject
{
    /// <summary>Initializes an AvalonDock resource theme.</summary>
    public Theme() { }

    /// <summary>Gets the compiled resource URI, or null for a dictionary-backed theme.</summary>
    public abstract Uri? GetResourceUri();
}
