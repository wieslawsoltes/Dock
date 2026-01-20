using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Services;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public class DockFactory : Factory
{
    private readonly IScreen _host;
    private readonly IProjectRepository _repository;
    private readonly IDockNavigationService _dockNavigation;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;
    private readonly IWindowLifecycleService _windowLifecycleService;
    private readonly IDockDispatcher _dispatcher;
    private IDocumentDock? _documentDock;

    public DockFactory(
        IScreen host,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        IWindowLifecycleService windowLifecycleService,
        IDockDispatcher dispatcher)
    {
        _host = host;
        _repository = repository;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;
        _overlayServicesProvider = overlayServicesProvider;
        _windowLifecycleService = windowLifecycleService;
        _dispatcher = dispatcher;

        WindowLifecycleServices.Add(_windowLifecycleService);
    }

    public override IRootDock CreateLayout()
    {
        var projectList = new ProjectListDocumentViewModel(
            _host,
            _repository,
            _dockNavigation,
            _workspaceFactory,
            _overlayServicesProvider,
            _dispatcher)
        {
            Id = "Projects",
            Title = "Projects",
            CanClose = false
        };

        var documentDock = CreateDocumentDock();
        documentDock.Id = "Documents";
        documentDock.VisibleDockables = CreateList<IDockable>(projectList);
        documentDock.ActiveDockable = projectList;
        documentDock.CanCreateDocument = false;
        documentDock.CanCloseLastDockable = true;

        var root = (BusyRootDock)CreateRootDock();
        root.VisibleDockables = CreateList<IDockable>(documentDock);
        root.DefaultDockable = documentDock;
        root.ActiveDockable = documentDock;

        root.PinnedDock = null;

        _documentDock = documentDock;
        _dockNavigation.AttachFactory(this, _host);

        return root;
    }

    public override IDocumentDock CreateDocumentDock()
        => new DocumentDock
        {
            CanCloseLastDockable = true
        };

    public override IRootDock CreateRootDock()
        => new BusyRootDock(
            _host,
            _overlayServicesProvider.GetServices(_host))
        {
            LeftPinnedDockables = CreateList<IDockable>(),
            RightPinnedDockables = CreateList<IDockable>(),
            TopPinnedDockables = CreateList<IDockable>(),
            BottomPinnedDockables = CreateList<IDockable>()
        };

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);

        if (window is not null)
        {
            window.Title = dockable.Title ?? "Dock";
        }

        return window;
    }

    public void OpenDocument(IDockable document, IDocumentDock? documentDock, bool floatWindow)
    {
        var targetDock = documentDock ?? _documentDock;

        if (targetDock is null)
        {
            return;
        }

        AddDockable(targetDock, document);
        SetActiveDockable(document);
        SetFocusedDockable(targetDock, document);

        if (floatWindow)
        {
            FloatDockable(document);
        }
    }

    public void OpenDocument(IDockable document, bool floatWindow)
        => OpenDocument(document, _documentDock, floatWindow);

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

}
