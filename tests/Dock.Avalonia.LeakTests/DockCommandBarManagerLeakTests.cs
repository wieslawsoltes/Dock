using System;
using System.Collections.Generic;
using Dock.Avalonia.CommandBars;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.CommandBars;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
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
}
