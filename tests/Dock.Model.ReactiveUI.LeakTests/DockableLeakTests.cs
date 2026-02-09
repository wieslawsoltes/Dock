using System;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Xunit;
using static Dock.Model.ReactiveUI.LeakTests.LeakTestHelpers;

namespace Dock.Model.ReactiveUI.LeakTests;

[Collection("LeakTests")]
public class DockableLeakTests
{
    [ReleaseFact]
    public void CloseDockable_DoesNotLeak_DocumentAndTool()
    {
        var factory = new Factory();

        var root = factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.Factory = factory;
        documentDock.VisibleDockables = factory.CreateList<IDockable>();

        IDocument document = factory.CreateDocument();
        document.Factory = factory;
        factory.AddDockable(documentDock, document);
        documentDock.ActiveDockable = document;

        var toolDock = factory.CreateToolDock();
        toolDock.Factory = factory;
        toolDock.VisibleDockables = factory.CreateList<IDockable>();

        ITool tool = factory.CreateTool();
        tool.Factory = factory;
        factory.AddDockable(toolDock, tool);
        toolDock.ActiveDockable = tool;

        factory.AddDockable(root, documentDock);
        factory.AddDockable(root, toolDock);

        root.ActiveDockable = documentDock;
        root.DefaultDockable = documentDock;

        var documentRef = new WeakReference(document);
        var toolRef = new WeakReference(tool);

        factory.CloseDockable(document);
        factory.CloseDockable(tool);

        root.ActiveDockable = null;
        documentDock.ActiveDockable = null;
        toolDock.ActiveDockable = null;

        document = null!;
        tool = null!;

        AssertCollected(documentRef, toolRef);
        GC.KeepAlive(factory);
        GC.KeepAlive(root);
        GC.KeepAlive(documentDock);
        GC.KeepAlive(toolDock);
    }

    [ReleaseFact]
    public void DockingState_Transitions_DoNotLeak_Dockables()
    {
        var factory = new Factory();

        var root = factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var toolDock = factory.CreateToolDock();
        toolDock.Factory = factory;
        toolDock.VisibleDockables = factory.CreateList<IDockable>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.Factory = factory;
        documentDock.VisibleDockables = factory.CreateList<IDockable>();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(root, documentDock);

        ITool tool = factory.CreateTool();
        tool.Factory = factory;
        factory.AddDockable(toolDock, tool);

        IDocument document = factory.CreateDocument();
        document.Factory = factory;
        factory.AddDockable(documentDock, document);

        factory.DockAsDocument(tool);
        factory.MoveDockable(documentDock, toolDock, tool, null);
        factory.PinDockable(tool);
        factory.UnpinDockable(tool);
        factory.FloatDockable(tool);
        factory.HideDockable(tool);
        factory.RestoreDockable(tool);
        factory.FloatDockable(document);
        factory.HideDockable(document);
        factory.RestoreDockable(document);
        if (document.Owner is IDock documentOwnerDock && documentOwnerDock is not IRootDock)
        {
            factory.HideDockable(documentOwnerDock);
            factory.RestoreDockable(documentOwnerDock);
        }

        var toolRef = new WeakReference(tool);
        var documentRef = new WeakReference(document);

        factory.RemoveDockable(tool, collapse: true);
        factory.RemoveDockable(document, collapse: true);

        toolDock.ActiveDockable = null;
        documentDock.ActiveDockable = null;
        root.ActiveDockable = null;

        tool = null!;
        document = null!;

        AssertCollected(toolRef, documentRef);
        GC.KeepAlive(factory);
        GC.KeepAlive(root);
        GC.KeepAlive(toolDock);
        GC.KeepAlive(documentDock);
    }
}
