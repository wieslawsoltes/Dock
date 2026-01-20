using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Overlays;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Services;
using Dock.Model.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class AvaloniaHostServiceResolver : IHostServiceResolver
{
    private readonly IHostServiceResolver _fallback = new OwnerChainHostServiceResolver();

    public TService? Resolve<TService>(IScreen screen) where TService : class
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return _fallback.Resolve<TService>(screen);
        }

        if (screen is IDockable dockable)
        {
            var control = FindTrackedControl(dockable);
            if (control is not null)
            {
                var resolved = ResolveFromControl<TService>(control);
                if (resolved is not null)
                {
                    return resolved;
                }
            }

            var dockControl = FindDockControl(dockable);
            if (dockControl is not null)
            {
                var resolved = ResolveFromControl<TService>(dockControl);
                if (resolved is not null)
                {
                    return resolved;
                }
            }
        }

        return _fallback.Resolve<TService>(screen);
    }

    private static Control? FindTrackedControl(IDockable dockable)
    {
        var current = dockable;
        while (current is not null)
        {
            var factory = current.Factory;
            if (factory is not null)
            {
                if (TryGetControl(factory.VisibleDockableControls, current, out var control)
                    || TryGetControl(factory.PinnedDockableControls, current, out control)
                    || TryGetControl(factory.TabDockableControls, current, out control))
                {
                    return control;
                }

                if (TryGetControl(factory.VisibleRootControls, current, out control)
                    || TryGetControl(factory.PinnedRootControls, current, out control)
                    || TryGetControl(factory.TabRootControls, current, out control))
                {
                    return control;
                }
            }

            current = current.Owner;
        }

        return null;
    }

    private static DockControl? FindDockControl(IDockable dockable)
    {
        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return null;
        }

        foreach (var dockControl in factory.DockControls.OfType<DockControl>())
        {
            if (dockControl.GetVisualRoot() is null || dockControl.Layout is null)
            {
                continue;
            }

            if (ReferenceEquals(dockControl.Layout, dockable)
                || factory.FindDockable(dockControl.Layout, d => ReferenceEquals(d, dockable)) is not null)
            {
                return dockControl;
            }
        }

        return null;
    }

    private static IFactory? FindFactory(IDockable dockable)
    {
        var current = dockable;
        while (current is not null)
        {
            if (current.Factory is IFactory factory)
            {
                return factory;
            }

            current = current.Owner;
        }

        return null;
    }

    private static bool TryGetControl<TKey>(
        IDictionary<IDockable, TKey> map,
        IDockable dockable,
        out Control? control)
        where TKey : class
    {
        if (map.TryGetValue(dockable, out var value))
        {
            control = value as Control;
            if (control is not null)
            {
                return true;
            }
        }

        control = null;
        return false;
    }

    private static TService? ResolveFromControl<TService>(Control control) where TService : class
    {
        var overlayHost = control.FindAncestorOfType<OverlayHost>();
        var resolved = ResolveFromDataContext<TService>(overlayHost?.DataContext);
        if (resolved is not null)
        {
            return resolved;
        }

        if (control.GetVisualRoot() is not TopLevel topLevel)
        {
            return null;
        }

        if (topLevel is HostWindow hostWindow)
        {
            resolved = ResolveFromDataContext<TService>(hostWindow.DataContext);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        foreach (var host in topLevel.GetVisualDescendants().OfType<OverlayHost>())
        {
            resolved = ResolveFromDataContext<TService>(host.DataContext);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        foreach (var dockControl in topLevel.GetVisualDescendants().OfType<DockControl>())
        {
            if (dockControl.Layout is TService fromLayout)
            {
                return fromLayout;
            }
        }

        return null;
    }

    private static TService? ResolveFromDataContext<TService>(object? dataContext) where TService : class
    {
        if (dataContext is TService typed)
        {
            return typed;
        }

        if (dataContext is IRootDock root && root is TService rootService)
        {
            return rootService;
        }

        if (dataContext is IHostOverlayServices hostOverlay && hostOverlay is TService hostService)
        {
            return hostService;
        }

        return null;
    }
}
