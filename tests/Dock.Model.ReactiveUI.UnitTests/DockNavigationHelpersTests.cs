using System;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;
using Dock.Model.ReactiveUI.Navigation.Services;
using ReactiveUI;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests;

public class DockNavigationHelpersTests
{
    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }

    private sealed class TestRoutableViewModel : ReactiveObject, IRoutableViewModel
    {
        public TestRoutableViewModel(IScreen hostScreen, string? segment = null)
        {
            HostScreen = hostScreen;
            UrlPathSegment = segment ?? "test";
        }

        public string? UrlPathSegment { get; }

        public IScreen HostScreen { get; }
    }

    [Fact]
    public void TryNavigateBack_ReturnsFalse_WhenStackHasSingleItem()
    {
        var screen = new TestScreen();
        var first = new TestRoutableViewModel(screen, "first");

        screen.Router.Navigate.Execute(first).Subscribe();

        var result = DockNavigationHelpers.TryNavigateBack(screen);

        Assert.False(result);
        Assert.Single(screen.Router.NavigationStack);
        Assert.Same(first, screen.Router.NavigationStack[0]);
    }

    [Fact]
    public void TryNavigateBack_Navigates_WhenStackHasMultipleItems()
    {
        var screen = new TestScreen();
        var first = new TestRoutableViewModel(screen, "first");
        var second = new TestRoutableViewModel(screen, "second");

        screen.Router.Navigate.Execute(first).Subscribe();
        screen.Router.Navigate.Execute(second).Subscribe();

        var result = DockNavigationHelpers.TryNavigateBack(screen);

        Assert.True(result);
        Assert.Single(screen.Router.NavigationStack);
        Assert.Same(first, screen.Router.NavigationStack[0]);
    }
}

public class DockNavigationServiceTests
{
    private sealed class TestFactory : Factory
    {
        public IDock? AddedDock { get; private set; }
        public IDockable? AddedDockable { get; private set; }
        public IDockable? ActiveDockable { get; private set; }
        public IDock? FocusedDock { get; private set; }
        public IDockable? FocusedDockable { get; private set; }
        public IDockable? FloatedDockable { get; private set; }
        public int FloatDockableCalls { get; private set; }

        public override void AddDockable(IDock dock, IDockable dockable)
        {
            AddedDock = dock;
            AddedDockable = dockable;
            base.AddDockable(dock, dockable);
        }

        public override void SetActiveDockable(IDockable dockable)
        {
            ActiveDockable = dockable;
            base.SetActiveDockable(dockable);
        }

        public override void SetFocusedDockable(IDock dock, IDockable? dockable)
        {
            FocusedDock = dock;
            FocusedDockable = dockable;
            base.SetFocusedDockable(dock, dockable);
        }

        public override void FloatDockable(IDockable dockable)
        {
            FloatDockableCalls++;
            FloatedDockable = dockable;
            base.FloatDockable(dockable);
        }
    }

    private sealed class TestScreenDockable : DockableBase, IScreen
    {
        public RoutingState Router { get; } = new();
    }

    [Fact]
    public void OpenDocument_UsesDocumentDockFromOwnerChain()
    {
        var factory = CreateFactory(out var hostScreen, out var documentDock);
        var document = new Document();
        var service = new DockNavigationService();

        service.AttachFactory(factory, hostScreen);
        service.OpenDocument(hostScreen, document, floatWindow: false);

        Assert.Same(documentDock, factory.AddedDock);
        Assert.Same(document, factory.AddedDockable);
        Assert.Same(document, factory.ActiveDockable);
        Assert.Same(documentDock, factory.FocusedDock);
        Assert.Same(document, factory.FocusedDockable);
        Assert.Equal(0, factory.FloatDockableCalls);
    }

    [Fact]
    public void OpenDocument_Floats_WhenRequested()
    {
        var factory = CreateFactory(out var hostScreen, out _);
        var document = new Document();
        var service = new DockNavigationService();

        service.AttachFactory(factory, hostScreen);
        service.OpenDocument(hostScreen, document, floatWindow: true);

        Assert.Equal(1, factory.FloatDockableCalls);
    }

    [Fact]
    public void OpenDocument_Uses_Document_Already_In_Dock()
    {
        var factory = CreateFactory(out var hostScreen, out var documentDock);
        var document = new Document { Id = "Doc1" };
        factory.AddDockable(documentDock, document);
        var service = new DockNavigationService();

        service.AttachFactory(factory, hostScreen);
        service.OpenDocument(hostScreen, document, floatWindow: false);

        var dockables = documentDock.VisibleDockables;
        Assert.NotNull(dockables);
        Assert.Single(dockables!);
        Assert.Same(document, factory.ActiveDockable);
        Assert.Same(documentDock, factory.FocusedDock);
        Assert.Same(document, factory.FocusedDockable);
    }

    [Fact]
    public void OpenDocument_Adds_New_Document_When_Id_Matches_Existing()
    {
        var factory = CreateFactory(out var hostScreen, out var documentDock);
        var existing = new Document { Id = "Doc1" };
        factory.AddDockable(documentDock, existing);
        var newDocument = new Document { Id = "Doc1" };
        var service = new DockNavigationService();

        service.AttachFactory(factory, hostScreen);
        service.OpenDocument(hostScreen, newDocument, floatWindow: false);

        var dockables = documentDock.VisibleDockables;
        Assert.NotNull(dockables);
        Assert.Equal(2, dockables!.Count);
        Assert.Contains(existing, dockables);
        Assert.Contains(newDocument, dockables);
        Assert.Same(newDocument, factory.ActiveDockable);
        Assert.Same(documentDock, factory.FocusedDock);
        Assert.Same(newDocument, factory.FocusedDockable);
    }

    private static TestFactory CreateFactory(out TestScreenDockable hostScreen, out DocumentDock documentDock)
    {
        var factory = new TestFactory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };

        documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };

        root.VisibleDockables.Add(documentDock);
        root.DefaultDockable = documentDock;
        root.Factory = factory;
        documentDock.Owner = root;
        documentDock.Factory = factory;

        hostScreen = new TestScreenDockable
        {
            Owner = documentDock,
            Factory = factory
        };

        return factory;
    }
}
