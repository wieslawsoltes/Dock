using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class GlobalDockTrackingTests
{
    [AvaloniaFact]
    public void ManagedWindowDock_Switch_Updates_Global_Tracking_Context()
    {
        var factory = new Factory();
        var first = CreateContext(factory, "A");
        var second = CreateContext(factory, "B");
        var managedDock = new ManagedWindowDock { Factory = factory };

        managedDock.AddWindow(first.ManagedDocument);
        var reasons = new List<DockTrackingChangeReason>();
        factory.GlobalDockTrackingChanged += (_, args) => reasons.Add(args.Reason);

        managedDock.AddWindow(second.ManagedDocument);

        Assert.Equal(2, reasons.Count);
        Assert.Equal(DockTrackingChangeReason.WindowDeactivated, reasons[0]);
        Assert.Equal(DockTrackingChangeReason.WindowActivated, reasons[1]);
        Assert.Same(second.Dockable, factory.CurrentDockable);
        Assert.Same(second.Root, factory.CurrentRootDock);
        Assert.Same(second.Window, factory.CurrentDockWindow);
    }

    [AvaloniaFact]
    public void ManagedWindowDock_Switch_Keeps_Guard_Against_Background_SetActiveDockable()
    {
        var factory = new Factory();
        var first = CreateContext(factory, "A");
        var second = CreateContext(factory, "B");
        var managedDock = new ManagedWindowDock { Factory = factory };

        managedDock.AddWindow(first.ManagedDocument);
        managedDock.AddWindow(second.ManagedDocument);
        Assert.Same(second.Dockable, factory.CurrentDockable);

        factory.SetActiveDockable(first.Dockable);

        Assert.Same(second.Dockable, factory.CurrentDockable);
        Assert.Same(second.Root, factory.CurrentRootDock);
        Assert.Same(second.Window, factory.CurrentDockWindow);
    }

    [AvaloniaFact]
    public void ManagedWindowDock_Setting_Active_Window_To_Null_Clears_Global_Tracking()
    {
        var factory = new Factory();
        var context = CreateContext(factory, "A");
        var managedDock = new ManagedWindowDock { Factory = factory };
        managedDock.AddWindow(context.ManagedDocument);
        Assert.Same(context.Dockable, factory.CurrentDockable);

        var reasons = new List<DockTrackingChangeReason>();
        factory.GlobalDockTrackingChanged += (_, args) => reasons.Add(args.Reason);

        managedDock.ActiveDockable = null;

        Assert.Single(reasons);
        Assert.Equal(DockTrackingChangeReason.WindowDeactivated, reasons[0]);
        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
    }

    [AvaloniaFact]
    public void DeserializeLike_LayoutReplacement_Reanchors_GlobalTracking_For_SplitDocuments()
    {
        var factory = new Factory();
        var firstLayout = CreateSplitTrackingLayout(factory, "A");
        var secondLayout = CreateSplitTrackingLayout(factory, "B");

        var window = factory.CreateDockWindow();
        window.Id = "main-window";
        window.Layout = firstLayout.Root;
        firstLayout.Root.Window = window;

        var mockDockControl = new MockDockControl
        {
            Factory = factory,
            Layout = firstLayout.Root
        };
        factory.DockControls.Add(mockDockControl);

        factory.OnWindowActivated(window);
        factory.OnFocusedDockableChanged(firstLayout.LeftDocument);
        Assert.Same(firstLayout.Root, factory.CurrentRootDock);

        window.Layout = secondLayout.Root;
        secondLayout.Root.Window = window;
        mockDockControl.Layout = secondLayout.Root;

        var focusedReasons = new List<DockTrackingChangeReason>();
        factory.GlobalDockTrackingChanged += (_, args) =>
        {
            if (args.Reason == DockTrackingChangeReason.FocusedDockableChanged)
            {
                focusedReasons.Add(args.Reason);
            }
        };

        factory.OnFocusedDockableChanged(secondLayout.LeftDocument);
        factory.OnFocusedDockableChanged(secondLayout.RightDocument);

        Assert.Equal(2, focusedReasons.Count);
        Assert.Same(secondLayout.RightDocument, factory.CurrentDockable);
        Assert.Same(secondLayout.Root, factory.CurrentRootDock);
        Assert.Same(window, factory.CurrentDockWindow);
    }

    private static TrackingContext CreateContext(Factory factory, string idSuffix)
    {
        var root = factory.CreateRootDock();
        root.Id = $"root-{idSuffix}";

        var dock = factory.CreateDocumentDock();
        dock.Id = $"dock-{idSuffix}";

        var dockable = factory.CreateDocument();
        dockable.Id = $"doc-{idSuffix}";
        dockable.Title = $"Document-{idSuffix}";
        dockable.Owner = dock;

        dock.VisibleDockables = factory.CreateList<IDockable>(dockable);
        dock.Owner = root;
        dock.ActiveDockable = dockable;
        dock.FocusedDockable = dockable;

        root.VisibleDockables = factory.CreateList<IDockable>(dock);
        root.ActiveDockable = dockable;
        root.FocusedDockable = dockable;

        var window = factory.CreateDockWindow();
        window.Id = $"window-{idSuffix}";
        window.Layout = root;
        root.Window = window;

        var managedDocument = new ManagedDockWindowDocument(window);
        return new TrackingContext(root, window, dockable, managedDocument);
    }

    private static SplitTrackingLayout CreateSplitTrackingLayout(Factory factory, string idSuffix)
    {
        var root = factory.CreateRootDock();
        root.Id = $"split-root-{idSuffix}";

        var splitDock = factory.CreateProportionalDock();
        splitDock.Id = $"split-dock-{idSuffix}";
        splitDock.Owner = root;

        var leftDock = factory.CreateDocumentDock();
        leftDock.Id = $"left-dock-{idSuffix}";
        leftDock.Owner = splitDock;

        var rightDock = factory.CreateDocumentDock();
        rightDock.Id = $"right-dock-{idSuffix}";
        rightDock.Owner = splitDock;

        var leftDocument = factory.CreateDocument();
        leftDocument.Id = $"left-doc-{idSuffix}";
        leftDocument.Title = $"Left-{idSuffix}";
        leftDocument.Owner = leftDock;

        var rightDocument = factory.CreateDocument();
        rightDocument.Id = $"right-doc-{idSuffix}";
        rightDocument.Title = $"Right-{idSuffix}";
        rightDocument.Owner = rightDock;

        leftDock.VisibleDockables = factory.CreateList<IDockable>(leftDocument);
        leftDock.ActiveDockable = leftDocument;
        leftDock.FocusedDockable = leftDocument;

        rightDock.VisibleDockables = factory.CreateList<IDockable>(rightDocument);
        rightDock.ActiveDockable = rightDocument;
        rightDock.FocusedDockable = rightDocument;

        splitDock.VisibleDockables = factory.CreateList<IDockable>(leftDock, rightDock);
        splitDock.ActiveDockable = leftDock;
        splitDock.FocusedDockable = leftDocument;

        root.VisibleDockables = factory.CreateList<IDockable>(splitDock);
        root.ActiveDockable = splitDock;
        root.FocusedDockable = leftDocument;

        return new SplitTrackingLayout(root, leftDocument, rightDocument);
    }

    private sealed record TrackingContext(
        IRootDock Root,
        IDockWindow Window,
        IDockable Dockable,
        ManagedDockWindowDocument ManagedDocument);

    private sealed record SplitTrackingLayout(
        IRootDock Root,
        IDockable LeftDocument,
        IDockable RightDocument);
}
