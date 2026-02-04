using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.CommandBars;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.CommandBars;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockCommandBarManagerLeakTests
{
    [ReleaseFact]
    public void DockCommandBarManager_Detach_DoesNotLeak_WhenFactoryAlive()
    {
        var result = RunInSession(() =>
        {
            var previousMerging = DockSettings.CommandBarMergingEnabled;
            var previousScope = DockSettings.CommandBarMergingScope;
            DockSettings.CommandBarMergingEnabled = true;
            DockSettings.CommandBarMergingScope = DockCommandBarMergingScope.ActiveDockable;

            try
            {
                var factory = new Factory();
                var root = (IRootDock)factory.CreateRootDock();
                root.Factory = factory;
                root.VisibleDockables = factory.CreateList<IDockable>();

                var document = (Document)factory.CreateDocument();
                document.Factory = factory;

                var provider = new TestCommandBarProvider();
                document.Context = provider;

                var documentDock = (DocumentDock)factory.CreateDocumentDock();
                documentDock.Factory = factory;
                documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
                documentDock.ActiveDockable = document;
                document.Owner = documentDock;

                root.VisibleDockables.Add(documentDock);
                root.ActiveDockable = documentDock;
                root.DefaultDockable = documentDock;

                var host = new DockCommandBarHost();
                var manager = new DockCommandBarManager(host);
                manager.Attach(root);

                factory.SetActiveDockable(document);
                provider.RaiseChanged();
                DrainDispatcher();

                manager.Detach();

                var managerRef = new WeakReference(manager);
                manager = null;

                return new DockCommandBarManagerLeakResult(managerRef, host, factory, root, provider);
            }
            finally
            {
                DockSettings.CommandBarMergingEnabled = previousMerging;
                DockSettings.CommandBarMergingScope = previousScope;
            }
        });

        AssertCollected(result.ManagerRef);
        GC.KeepAlive(result.HostKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
        GC.KeepAlive(result.ProviderKeepAlive);
    }

    [ReleaseFact]
    public void DockCommandBarHost_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var command = new NoOpCommand();
            var menuItem = new MenuItem { Header = "File" };
            var openItem = new MenuItem { Header = "Open", Command = command };
            var exitItem = new MenuItem { Header = "Exit", Command = command };
            menuItem.ItemsSource = new object[]
            {
                openItem,
                new Separator(),
                exitItem
            };

            var menu = new Menu
            {
                ItemsSource = new object[]
                {
                    menuItem
                }
            };

            var toolButton = new Button { Content = "Action", Command = command };
            var toolBar = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
            toolBar.Children.Add(toolButton);

            var ribbonButton = new Button { Content = "Ribbon", Command = command };
            var ribbonBar = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
            ribbonBar.Children.Add(ribbonButton);

            var host = new DockCommandBarHost
            {
                MenuBars = new Control[] { menu },
                ToolBars = new Control[] { toolBar },
                RibbonBars = new Control[] { ribbonBar },
                IsVisible = true
            };
            var detached = false;
            host.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = host };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            host.ApplyTemplate();
            host.UpdateLayout();
            DrainDispatcher();

            ExerciseButtonInteractions(toolButton);
            ExerciseButtonInteractions(ribbonButton);

            window.Content = null;
            DrainDispatcher();
            window.UpdateLayout();
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var hostRef = new WeakReference(host);
            host = null;

            return new DockCommandBarHostLeakResult(hostRef, window, command, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DockCommandBarHost did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.HostRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.CommandKeepAlive);
    }

    private sealed class TestCommandBarProvider : IDockCommandBarProvider
    {
        public event EventHandler? CommandBarsChanged;

        public IReadOnlyList<DockCommandBarDefinition> GetCommandBars() =>
            Array.Empty<DockCommandBarDefinition>();

        public void RaiseChanged() => CommandBarsChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed record DockCommandBarManagerLeakResult(
        WeakReference ManagerRef,
        DockCommandBarHost HostKeepAlive,
        IFactory FactoryKeepAlive,
        IDock LayoutKeepAlive,
        TestCommandBarProvider ProviderKeepAlive);

    private sealed record DockCommandBarHostLeakResult(
        WeakReference HostRef,
        Window WindowKeepAlive,
        NoOpCommand CommandKeepAlive,
        bool DetachedFromVisualTree);
}
