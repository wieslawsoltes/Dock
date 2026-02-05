using System;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ManagedDockWindowDocumentLeakTests
{
    [ReleaseFact]
    public void ManagedDockWindowDocument_Dispose_DoesNotLeak_WhenWindowAlive()
    {
        var result = RunInSession(() =>
        {
            var factory = new Factory();
            var root = (IRootDock)factory.CreateRootDock();
            root.Factory = factory;
            root.VisibleDockables = factory.CreateList<IDockable>();

            var focusedDockable = factory.CreateDocument();
            focusedDockable.Factory = factory;
            focusedDockable.Title = "Focused";
            root.FocusedDockable = focusedDockable;

            var dockWindow = new DockWindow
            {
                Factory = factory,
                Layout = root,
                Title = "Managed"
            };

            var document = new ManagedDockWindowDocument(dockWindow)
            {
                Factory = factory
            };

            var documentRef = new WeakReference(document);
            document.Dispose();
            document = null;

            return new ManagedDockWindowDocumentLeakResult(
                documentRef,
                dockWindow,
                root,
                focusedDockable,
                factory);
        });

        AssertCollected(result.DocumentRef);
        GC.KeepAlive(result.DockWindowKeepAlive);
        GC.KeepAlive(result.RootKeepAlive);
        GC.KeepAlive(result.FocusedDockableKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    private sealed record ManagedDockWindowDocumentLeakResult(
        WeakReference DocumentRef,
        DockWindow DockWindowKeepAlive,
        IRootDock RootKeepAlive,
        IDockable FocusedDockableKeepAlive,
        Factory FactoryKeepAlive);
}
