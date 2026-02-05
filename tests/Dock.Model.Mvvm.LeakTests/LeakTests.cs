using System;
using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Xunit;
using static Dock.Model.Mvvm.LeakTests.LeakTestHelpers;

namespace Dock.Model.Mvvm.LeakTests;

[Collection("LeakTests")]
public class LeakTests
{
    private sealed class StubSerializer : IDockSerializer
    {
        public string Serialize<T>(T value) => throw new NotSupportedException();

        public T? Deserialize<T>(string text) => throw new NotSupportedException();

        public T? Load<T>(System.IO.Stream stream) => throw new NotSupportedException();

        public void Save<T>(System.IO.Stream stream, T value) => throw new NotSupportedException();
    }

    [ReleaseFact]
    public void StopTracking_DoesNotLeak_Manager()
    {
        var factory = new Factory();
        var managerRef = CreateStoppedManager(factory);

        AssertCollected(managerRef);
        GC.KeepAlive(factory);
    }

    [ReleaseFact]
    public void TrackLayout_Swap_DoesNotLeak_PreviousLayout()
    {
        var result = CreateLayoutSwap();

        AssertCollected(result.LayoutARef, result.FactoryARef);
        GC.KeepAlive(result.Manager);
        GC.KeepAlive(result.FactoryB);
        GC.KeepAlive(result.LayoutB);
    }

    private static WeakReference CreateStoppedManager(Factory factory)
    {
        var layout = factory.CreateLayout();
        layout.Factory = factory;

        var manager = new DockWorkspaceManager(new StubSerializer());
        manager.TrackLayout(layout);
        manager.StopTracking();

        return new WeakReference(manager);
    }

    private static LayoutSwapResult CreateLayoutSwap()
    {
        var manager = new DockWorkspaceManager(new StubSerializer());

        var factoryA = new Factory();
        var layoutA = factoryA.CreateLayout();
        layoutA.Factory = factoryA;
        manager.TrackLayout(layoutA);

        var factoryB = new Factory();
        var layoutB = factoryB.CreateLayout();
        layoutB.Factory = factoryB;
        manager.TrackLayout(layoutB);

        return new LayoutSwapResult(
            new WeakReference(layoutA),
            new WeakReference(factoryA),
            manager,
            factoryB,
            layoutB);
    }

    private sealed record LayoutSwapResult(
        WeakReference LayoutARef,
        WeakReference FactoryARef,
        DockWorkspaceManager Manager,
        Factory FactoryB,
        IDock LayoutB);
}
