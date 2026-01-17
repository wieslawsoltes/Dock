using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class ProjectFileWorkspaceFactory
{
    private readonly IBusyServiceFactory _busyServiceFactory;
    private readonly IGlobalBusyService _globalBusyService;
    private readonly IDialogServiceFactory _dialogServiceFactory;
    private readonly IGlobalDialogService _globalDialogService;
    private readonly IConfirmationServiceFactory _confirmationServiceFactory;
    private readonly IGlobalConfirmationService _globalConfirmationService;
    private readonly IDialogServiceProvider _dialogServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;

    public ProjectFileWorkspaceFactory(
        IBusyServiceFactory busyServiceFactory,
        IGlobalBusyService globalBusyService,
        IDialogServiceFactory dialogServiceFactory,
        IGlobalDialogService globalDialogService,
        IConfirmationServiceFactory confirmationServiceFactory,
        IGlobalConfirmationService globalConfirmationService,
        IDialogServiceProvider dialogServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
    {
        _busyServiceFactory = busyServiceFactory;
        _globalBusyService = globalBusyService;
        _dialogServiceFactory = dialogServiceFactory;
        _globalDialogService = globalDialogService;
        _confirmationServiceFactory = confirmationServiceFactory;
        _globalConfirmationService = globalConfirmationService;
        _dialogServiceProvider = dialogServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;
    }

    public ProjectFileWorkspace CreateWorkspace(IScreen hostScreen, Project project, ProjectFile file)
    {
        var factory = new WorkspaceDockFactory(
            hostScreen,
            _busyServiceFactory,
            _globalBusyService,
            _dialogServiceFactory,
            _globalDialogService,
            _confirmationServiceFactory,
            _globalConfirmationService);
        var root = (BusyRootDock)factory.CreateRootDock();
        root.Id = "FileWorkspaceRoot";
        root.PinnedDock = null;

        var fileActionsTool = new FileActionsToolViewModel(
            root,
            project,
            file,
            _dialogServiceProvider,
            _confirmationServiceProvider)
        {
            Id = $"FileActions-{project.Id}-{file.Id}",
            Title = "File Actions",
            CanClose = false,
            CanFloat = true,
            CanDrag = true
        };

        var rightTools = new Dictionary<string, ToolPanelViewModel>
        {
            ["outline"] = new ToolPanelViewModel(
                root,
                "outline",
                "Outline",
                "Symbols and declarations for quick navigation.",
                _dialogServiceProvider,
                _confirmationServiceProvider)
            {
                Id = $"Outline-{project.Id}-{file.Id}",
                Title = "Outline",
                CanClose = false,
                CanFloat = true,
                CanDrag = true
            },
            ["insights"] = new ToolPanelViewModel(
                root,
                "insights",
                "Insights",
                "Metrics, warnings, and quality signals.",
                _dialogServiceProvider,
                _confirmationServiceProvider)
            {
                Id = $"Insights-{project.Id}-{file.Id}",
                Title = "Insights",
                CanClose = false,
                CanFloat = true,
                CanDrag = true
            },
            ["history"] = new ToolPanelViewModel(
                root,
                "history",
                "History",
                "Recent edits and change summaries.",
                _dialogServiceProvider,
                _confirmationServiceProvider)
            {
                Id = $"History-{project.Id}-{file.Id}",
                Title = "History",
                CanClose = true,
                CanFloat = true,
                CanDrag = true
            }
        };

        var leftDock = new ToolDock
        {
            Id = "FileActionsDock",
            Alignment = Alignment.Left,
            AutoHide = true,
            IsExpanded = false,
            GripMode = GripMode.AutoHide,
            CanFloat = true,
            CanDrag = true,
            Proportion = 0.2,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var rightDock = new ToolDock
        {
            Id = "WorkspaceToolsDock",
            Alignment = Alignment.Right,
            AutoHide = true,
            IsExpanded = false,
            GripMode = GripMode.AutoHide,
            CanFloat = true,
            CanDrag = true,
            Proportion = 0.25,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var workspace = new ProjectFileWorkspace(factory, leftDock, fileActionsTool, rightDock, rightTools);

        var ribbonTool = new RibbonToolViewModel(root, workspace, rightTools.Values)
        {
            Id = $"Ribbon-{project.Id}-{file.Id}",
            Title = "Ribbon",
            CanClose = false
        };

        var ribbonDock = new ToolDock
        {
            Id = "RibbonDock",
            Alignment = Alignment.Top,
            AutoHide = false,
            IsExpanded = true,
            GripMode = GripMode.Hidden,
            Proportion = 0.15,
            VisibleDockables = factory.CreateList<IDockable>(ribbonTool),
            ActiveDockable = ribbonTool
        };

        var editorDocument = new ProjectFileEditorDocumentViewModel(project, file);

        var editorDock = new DocumentDock
        {
            Id = "EditorDock",
            VisibleDockables = factory.CreateList<IDockable>(editorDocument),
            ActiveDockable = editorDocument,
            CanCreateDocument = false
        };

        var contentLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            ActiveDockable = editorDock,
            VisibleDockables = factory.CreateList<IDockable>(
                leftDock,
                new ProportionalDockSplitter(),
                editorDock,
                new ProportionalDockSplitter(),
                rightDock)
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            ActiveDockable = contentLayout,
            VisibleDockables = factory.CreateList<IDockable>(
                ribbonDock,
                new ProportionalDockSplitter { CanResize = false },
                contentLayout)
        };

        root.ActiveDockable = mainLayout;
        root.DefaultDockable = mainLayout;
        root.VisibleDockables = factory.CreateList<IDockable>(mainLayout);

        factory.InitLayout(root);
        workspace.Layout = root;

        return workspace;
    }

    public async Task<ProjectFileWorkspace> CreateWorkspaceAsync(
        IScreen hostScreen,
        Project project,
        ProjectFile file,
        CancellationToken cancellationToken)
    {
        await Task.Delay(350, cancellationToken).ConfigureAwait(false);
        return CreateWorkspace(hostScreen, project, file);
    }
}
