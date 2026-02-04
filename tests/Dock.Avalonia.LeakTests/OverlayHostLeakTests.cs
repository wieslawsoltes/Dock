using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls.Overlays;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class OverlayHostLeakTests
{
    [ReleaseFact]
    public void OverlayHost_OverlayLayers_DoesNotLeak_WhenLayersAlive()
    {
        var result = RunInSession(() =>
        {
            var layer = new OverlayLayer { Overlay = null };
            var layers = new OverlayLayerCollection { layer };

            var host = new OverlayHost
            {
                Content = new Border(),
                OverlayLayers = layers,
                UseServiceLayers = false
            };

            var window = new Window { Content = host };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            layer.IsVisible = false;
            layer.IsVisible = true;
            layers.Add(new OverlayLayer { Overlay = null, ZIndex = 1 });
            DrainDispatcher();

            var result = new OverlayHostLayerLeakResult(
                new WeakReference(host),
                layers,
                layer);

            CleanupWindow(window);
            return result;
        });

        AssertCollected(result.HostRef);
        GC.KeepAlive(result.LayersKeepAlive);
        GC.KeepAlive(result.LayerKeepAlive);
    }

    [ReleaseFact]
    public void OverlayHost_ServiceLayers_DoesNotLeak_WhenProviderAlive()
    {
        var result = RunInSession(() =>
        {
            var previousProvider = OverlayLayerRegistry.Provider;
            var previousFactoryProvider = OverlayLayerRegistry.FactoryProvider;

            var layers = new OverlayLayerCollection
            {
                new OverlayLayer { Overlay = null, ZIndex = 1 }
            };

            Func<IEnumerable<IOverlayLayer>> provider = () => layers;

            try
            {
                var host = new OverlayHost
                {
                    Content = new Border(),
                    UseServiceLayers = true
                };

                var window = new Window { Content = host };
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());

                ShowWindow(window);

                OverlayLayerRegistry.Provider = provider;
                OverlayLayerRegistry.FactoryProvider = null;
                DrainDispatcher();

                OverlayLayerRegistry.Provider = null;
                OverlayLayerRegistry.Provider = provider;
                DrainDispatcher();

                var result = new OverlayHostServiceLeakResult(
                    new WeakReference(host),
                    layers,
                    provider);

                CleanupWindow(window);
                return result;
            }
            finally
            {
                OverlayLayerRegistry.Provider = previousProvider;
                OverlayLayerRegistry.FactoryProvider = previousFactoryProvider;
            }
        });

        AssertCollected(result.HostRef);
        GC.KeepAlive(result.LayersKeepAlive);
        GC.KeepAlive(result.ProviderKeepAlive);
    }


    private sealed record OverlayHostLayerLeakResult(
        WeakReference HostRef,
        OverlayLayerCollection LayersKeepAlive,
        OverlayLayer LayerKeepAlive);

    private sealed record OverlayHostServiceLeakResult(
        WeakReference HostRef,
        OverlayLayerCollection LayersKeepAlive,
        Func<IEnumerable<IOverlayLayer>> ProviderKeepAlive);
}
