using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Themes;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutThemeCoordinator
{
    private readonly LayoutThemeScope _manager;
    private readonly ThemeResourceRegistry _registry;
    private readonly List<WeakReference<LayoutThemeScope>> _windows = new();
    private Theme? _theme;
    private bool _updating;
    private bool _pending;

    internal LayoutThemeCoordinator(DockingManager manager)
    {
        _registry = manager.ThemeResources;
        _manager = new(manager, this);
    }

    internal IResourceProvider? CreateResources(Theme? theme, IResourceDictionary ownerResources) =>
        LayoutThemeResources.Create(theme, _registry, ownerResources);

    internal void Apply(Theme? theme)
    {
        _theme = theme;
        _pending = true;
        if (_updating) return;
        _updating = true;
        try
        {
            do
            {
                _pending = false;
                _manager.Apply(_theme);
                if (_pending) continue;
                WeakReference<LayoutThemeScope>[] windows = _windows.ToArray();
                foreach (WeakReference<LayoutThemeScope> reference in windows)
                {
                    if (reference.TryGetTarget(out LayoutThemeScope? scope)) scope.Apply(_theme);
                    if (_pending) break;
                }
            } while (_pending);
        }
        finally { _updating = false; }
    }

    internal void Attach(StyledElement window)
    {
        LayoutThemeScope? previous = window.GetValue(LayoutThemeScope.ScopeProperty);
        if (previous?.BelongsTo(this) == true) return;
        previous?.Dispose();
        for (int index = _windows.Count - 1; index >= 0; index--)
            if (!_windows[index].TryGetTarget(out _)) _windows.RemoveAt(index);
        LayoutThemeScope scope = new(window, this);
        window.SetValue(LayoutThemeScope.ScopeProperty, scope);
        _windows.Add(new(scope));
        scope.Apply(_theme);
    }
}
