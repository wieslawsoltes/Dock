using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Navigation.Controls;
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
    private IDocumentDock? _documentDock;

    public DockFactory(
        IScreen host,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory)
    {
        _host = host;
        _repository = repository;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;
    }

    public override IRootDock CreateLayout()
    {
        var projectList = new ProjectListDocumentViewModel(_host, _repository, _dockNavigation, _workspaceFactory)
        {
            Id = "Projects",
            Title = "Projects"
        };

        var documentDock = new DocumentDock
        {
            Id = "Documents",
            VisibleDockables = CreateList<IDockable>(projectList),
            ActiveDockable = projectList,
            CanCreateDocument = false
        };

        var root = new RoutableRootDock(_host)
        {
            VisibleDockables = CreateList<IDockable>(documentDock),
            DefaultDockable = documentDock,
            ActiveDockable = documentDock
        };

        root.LeftPinnedDockables = CreateList<IDockable>();
        root.RightPinnedDockables = CreateList<IDockable>();
        root.TopPinnedDockables = CreateList<IDockable>();
        root.BottomPinnedDockables = CreateList<IDockable>();

        root.PinnedDock = null;

        _documentDock = documentDock;
        _dockNavigation.AttachFactory(this, _host);

        return root;
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);

        if (window is not null)
        {
            window.Title = dockable.Title ?? "Dock";
        }

        return window;
    }

    public void OpenDocument(IDockable document, bool floatWindow)
    {
        if (_documentDock is null)
        {
            return;
        }

        AddDockable(_documentDock, document);
        SetActiveDockable(document);
        SetFocusedDockable(_documentDock, document);

        if (floatWindow)
        {
            FloatDockable(document);
        }
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}
