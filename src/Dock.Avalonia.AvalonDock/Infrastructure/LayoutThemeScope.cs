using System;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Themes;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutThemeScope : IDisposable
{
    internal static readonly AttachedProperty<LayoutThemeScope?> ScopeProperty =
        AvaloniaProperty.RegisterAttached<LayoutThemeScope, StyledElement, LayoutThemeScope?>("ThemeScope");

    private readonly StyledElement _owner;
    private readonly WeakReference<LayoutThemeCoordinator> _coordinator;
    private IResourceProvider? _resources;
    private bool _disposed;

    internal LayoutThemeScope(StyledElement owner, LayoutThemeCoordinator coordinator)
    {
        _owner = owner;
        _coordinator = new(coordinator);
        if (owner is Window window) window.Closed += WindowClosed;
    }

    internal bool BelongsTo(LayoutThemeCoordinator coordinator) =>
        !_disposed && _coordinator.TryGetTarget(out LayoutThemeCoordinator? current) && ReferenceEquals(current, coordinator);

    internal void Apply(Theme? theme)
    {
        if (_disposed) return;
        IResourceProvider? previous = _resources;
        _resources = null;
        if (previous is not null) _owner.Resources.MergedDictionaries.Remove(previous);
        if (theme is DictionaryTheme dictionary && ReferenceEquals(dictionary.ThemeResourceDictionary, _owner.Resources))
            throw new InvalidOperationException("A theme dictionary cannot contain the resource scope that consumes it.");
        IResourceProvider? next = LayoutThemeResources.Create(theme);
        _resources = next;
        if (next is not null) _owner.Resources.MergedDictionaries.Add(next);
    }

    private void WindowClosed(object? sender, EventArgs args) => Dispose();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_owner is Window window) window.Closed -= WindowClosed;
        if (_resources is not null) _owner.Resources.MergedDictionaries.Remove(_resources);
        _resources = null;
        if (ReferenceEquals(_owner.GetValue(ScopeProperty), this)) _owner.ClearValue(ScopeProperty);
    }
}
