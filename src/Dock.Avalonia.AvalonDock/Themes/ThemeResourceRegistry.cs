using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace AvalonDock.Themes;

/// <summary>Resolves application theme URIs with typed compiled-resource factories.</summary>
/// <remarks>Register resources on a manager before assigning its theme. Factories are called for the
/// manager and each auxiliary window; they may create a compiled resource dictionary or return a shared
/// dictionary. No assemblies, XAML text, files, or network resources are discovered at runtime.</remarks>
public sealed class ThemeResourceRegistry
{
    private readonly Dictionary<Uri, Func<ResourceDictionary>> _factories = new();

    /// <summary>Associates a theme URI with its compiled dictionary factory.</summary>
    /// <param name="uri">The URI returned by the application's <see cref="Theme.GetResourceUri"/> override.</param>
    /// <param name="factory">Creates or supplies the resources for a consuming scope.</param>
    /// <exception cref="ArgumentException">The URI is already registered or identifies the built-in generic theme.</exception>
    public void Register(Uri uri, Func<ResourceDictionary> factory)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(factory);
        if (uri.Equals(new Uri(GenericTheme.ResourcePath)))
            throw new ArgumentException("The built-in generic theme URI is reserved.", nameof(uri));
        _factories.Add(uri, factory);
    }

    internal ResourceDictionary Create(Uri uri)
    {
        if (!_factories.TryGetValue(uri, out Func<ResourceDictionary>? factory))
            throw new NotSupportedException($"Register a compiled resource factory in DockingManager.ThemeResources for '{uri}'.");
        return factory() ?? throw new InvalidOperationException("A theme resource factory returned null.");
    }
}
