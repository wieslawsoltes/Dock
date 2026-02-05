using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.CommandBars;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockControlLayoutLeakTests
{
    [ReleaseFact]
    public void DockControl_SwapLayout_DoesNotLeak_PreviousLayout()
    {
        var result = RunInSession(() =>
        {
            var previousMerging = DockSettings.CommandBarMergingEnabled;
            DockSettings.CommandBarMergingEnabled = true;

            try
            {
                var contextA = LeakContext.Create();
                contextA.Root.ActiveDockable = contextA.DocumentDock;
                contextA.Root.DefaultDockable = contextA.DocumentDock;

                var provider = new TestCommandBarProvider();
                contextA.Document.Context = provider;

                var contextB = LeakContext.Create();
                contextB.Root.ActiveDockable = contextB.DocumentDock;
                contextB.Root.DefaultDockable = contextB.DocumentDock;

                var dockControl = new DockControl
                {
                    Factory = contextA.Factory,
                    Layout = contextA.Root,
                    InitializeFactory = true,
                    InitializeLayout = true
                };

                var window = new Window { Content = dockControl };
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());

                ShowWindow(window);
                DrainDispatcher();

                contextA.Factory.SetActiveDockable(contextA.Document);
                DrainDispatcher();

                var result = new LayoutSwapLeakResult(
                    new WeakReference(contextA.Root),
                    new WeakReference(contextA.Document),
                    new WeakReference(provider),
                    dockControl,
                    contextA.Factory,
                    contextB.Root,
                    contextB.Factory);

                dockControl.Layout = contextB.Root;
                DrainDispatcher();
                ClearFactoryCaches(contextA.Factory);

                CleanupWindow(window);
                return result;
            }
            finally
            {
                DockSettings.CommandBarMergingEnabled = previousMerging;
            }
        });

        AssertCollected(nameof(result.LayoutARef), result.LayoutARef);
        AssertCollected(nameof(result.DocumentARef), result.DocumentARef);
        AssertCollected(nameof(result.ProviderRef), result.ProviderRef);
        GC.KeepAlive(result.DockControl);
        GC.KeepAlive(result.FactoryA);
        GC.KeepAlive(result.FactoryB);
        GC.KeepAlive(result.LayoutB);
    }

    [ReleaseFact]
    public void DockControl_ManagedLayer_DoesNotLeak_WhenFactoryLives()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.Root.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;
            context.Root.ActiveDockable = context.DocumentDock;
            context.Root.DefaultDockable = context.DocumentDock;

            var dockControl = new DockControl
            {
                Factory = context.Factory,
                Layout = context.Root,
                InitializeFactory = true,
                InitializeLayout = true
            };

            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            DrainDispatcher();

            var managedLayer = FindVisualDescendant<ManagedWindowLayer>(dockControl);
            Assert.NotNull(managedLayer);
            var wasVisible = managedLayer!.IsVisible;

            var result = new ManagedLayerLeakResult(
                new WeakReference(managedLayer),
                wasVisible,
                context.Factory);

            CleanupWindow(window);
            return result;
        });

        Assert.True(result.WasVisible, "Managed window layer was not enabled.");
        AssertCollected(nameof(result.LayerRef), result.LayerRef);
        GC.KeepAlive(result.Factory);
    }

    [ReleaseFact]
    public void DockControl_SharedFactory_DetachOne_DoesNotLeak_OldLayout()
    {
        var result = RunInSession(() =>
        {
            var factory = new Factory();
            var layoutA = CreateLayout(factory);
            var layoutB = CreateLayout(factory);

            var dockControlA = new DockControl
            {
                Factory = factory,
                Layout = layoutA,
                InitializeFactory = true,
                InitializeLayout = true
            };

            var dockControlB = new DockControl
            {
                Factory = factory,
                Layout = layoutB,
                InitializeFactory = true,
                InitializeLayout = true
            };

            var windowA = new Window { Content = dockControlA };
            windowA.Styles.Add(new FluentTheme());
            windowA.Styles.Add(new DockFluentTheme());
            ShowWindow(windowA);

            var windowB = new Window { Content = dockControlB };
            windowB.Styles.Add(new FluentTheme());
            windowB.Styles.Add(new DockFluentTheme());
            ShowWindow(windowB);

            var layoutRef = new WeakReference(layoutA);
            var controlRef = new WeakReference(dockControlA);

            CleanupWindow(windowA);
            dockControlA = null!;
            layoutA = null!;
            windowA = null!;
            DrainDispatcher();
            ResetDispatcherForUnitTests();

            CleanupWindow(windowB);
            return new SharedFactoryLeakResult(layoutRef, controlRef, factory, layoutB, dockControlB);
        });

        AssertCollected(nameof(result.LayoutRef), result.LayoutRef);
        AssertCollected(nameof(result.ControlRef), result.ControlRef);
        GC.KeepAlive(result.Factory);
        GC.KeepAlive(result.LayoutB);
        GC.KeepAlive(result.DockControlB);
    }

    private static void AssertCollected(string name, WeakReference reference)
    {
        for (var attempt = 0; attempt < 20 && reference.IsAlive; attempt++)
        {
            CollectGarbage();
        }

        Assert.False(reference.IsAlive, $"{name} leaked");
    }

    private sealed record SharedFactoryLeakResult(
        WeakReference LayoutRef,
        WeakReference ControlRef,
        IFactory Factory,
        IRootDock LayoutB,
        DockControl DockControlB);

    private static IRootDock CreateLayout(IFactory factory)
    {
        var root = (IRootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables ??= factory.CreateList<IDockable>();
        root.LeftPinnedDockables ??= factory.CreateList<IDockable>();
        root.RightPinnedDockables ??= factory.CreateList<IDockable>();
        root.TopPinnedDockables ??= factory.CreateList<IDockable>();
        root.BottomPinnedDockables ??= factory.CreateList<IDockable>();
        root.Windows ??= factory.CreateList<IDockWindow>();

        var tool = factory.CreateTool();
        tool.Factory = factory;

        var document = factory.CreateDocument();
        document.Factory = factory;

        var toolDock = factory.CreateToolDock();
        toolDock.Factory = factory;
        toolDock.VisibleDockables = factory.CreateList<IDockable>(tool);
        toolDock.ActiveDockable = tool;
        tool.Owner = toolDock;

        var documentDock = factory.CreateDocumentDock();
        documentDock.Factory = factory;
        documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
        documentDock.ActiveDockable = document;
        document.Owner = documentDock;

        root.VisibleDockables.Add(toolDock);
        root.VisibleDockables.Add(documentDock);
        root.ActiveDockable = documentDock;
        root.DefaultDockable = documentDock;
        root.PinnedDock = toolDock;
        root.LeftPinnedDockables?.Add(tool);

        return root;
    }

    private sealed class TestCommandBarProvider : IDockCommandBarProvider
    {
        public event EventHandler? CommandBarsChanged;

        public IReadOnlyList<DockCommandBarDefinition> GetCommandBars() => Array.Empty<DockCommandBarDefinition>();
    }

    private sealed record LayoutSwapLeakResult(
        WeakReference LayoutARef,
        WeakReference DocumentARef,
        WeakReference ProviderRef,
        DockControl DockControl,
        IFactory FactoryA,
        IRootDock LayoutB,
        IFactory FactoryB);

    private sealed record ManagedLayerLeakResult(
        WeakReference LayerRef,
        bool WasVisible,
        IFactory Factory);
}
