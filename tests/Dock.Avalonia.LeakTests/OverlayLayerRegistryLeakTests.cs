using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls.Overlays;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class OverlayLayerRegistryLeakTests
{
    [ReleaseFact]
    public void OverlayLayerRegistry_ProvidersAndHandlers_DoNotLeak()
    {
        var result = RunInSession(() =>
        {
            var listener = new ProviderListener();

            EventHandler handler = (_, _) => listener.Touch();
            Func<IEnumerable<IOverlayLayer>> provider = () =>
            {
                listener.Touch();
                return Array.Empty<IOverlayLayer>();
            };
            Func<IEnumerable<IOverlayLayerFactory>> factoryProvider = () => Array.Empty<IOverlayLayerFactory>();

            OverlayLayerRegistry.ProviderChanged += handler;
            OverlayLayerRegistry.Provider = provider;
            OverlayLayerRegistry.FactoryProvider = factoryProvider;

            OverlayLayerRegistry.ProviderChanged -= handler;
            OverlayLayerRegistry.Provider = null;
            OverlayLayerRegistry.FactoryProvider = null;

            var weak = new WeakReference(listener);
            listener = null;
            handler = null!;
            provider = null!;
            factoryProvider = null!;

            return weak;
        });

        AssertCollected(result);
    }

    private sealed class ProviderListener
    {
        public void Touch()
        {
        }
    }
}
