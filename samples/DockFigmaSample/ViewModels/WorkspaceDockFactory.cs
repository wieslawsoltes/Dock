using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Navigation.Controls;
using Dock.Settings;
using DockFigmaSample.ViewModels.Documents;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI;

namespace DockFigmaSample.ViewModels;

public class WorkspaceDockFactory : Factory
{
    private readonly IScreen _host;

    public WorkspaceDockFactory(IScreen host)
    {
        _host = host;
    }

    public CanvasDocumentViewModel CanvasDocument { get; private set; } = null!;
    public InspectorToolViewModel InspectorTool { get; private set; } = null!;

    public override IRootDock CreateLayout()
    {
        CanvasDocument = new CanvasDocumentViewModel(_host)
        {
            Id = "Canvas",
            Title = "Canvas"
        };

        var toolbarTool = new ToolbarToolViewModel
        {
            Id = "Toolbar",
            Title = "Tools"
        };

        var layersTool = new LayersToolViewModel
        {
            Id = "Layers",
            Title = "Layers"
        };

        var assetsTool = new AssetsToolViewModel
        {
            Id = "Assets",
            Title = "Assets"
        };

        InspectorTool = new InspectorToolViewModel(_host)
        {
            Id = "Inspector",
            Title = "Properties"
        };

        var commentsTool = new CommentsToolViewModel
        {
            Id = "Comments",
            Title = "Comments"
        };

        var documentDock = new DocumentDock
        {
            Id = "Documents",
            VisibleDockables = CreateList<IDockable>(CanvasDocument),
            ActiveDockable = CanvasDocument,
            CanCreateDocument = false
        };

        var toolbarDock = new ToolDock
        {
            Id = "ToolbarDock",
            Alignment = Alignment.Left,
            Proportion = 0.08,
            VisibleDockables = CreateList<IDockable>(toolbarTool),
            ActiveDockable = toolbarTool
        };

        var leftPanelDock = new ToolDock
        {
            Id = "LeftPanel",
            Alignment = Alignment.Left,
            Proportion = 0.22,
            VisibleDockables = CreateList<IDockable>(layersTool, assetsTool),
            ActiveDockable = layersTool
        };

        var leftGroup = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(toolbarDock, new ProportionalDockSplitter(), leftPanelDock),
            ActiveDockable = leftPanelDock
        };

        var bottomDock = new ToolDock
        {
            Id = "BottomPanel",
            Alignment = Alignment.Bottom,
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(commentsTool),
            ActiveDockable = commentsTool
        };

        var centerGroup = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>(documentDock, new ProportionalDockSplitter(), bottomDock),
            ActiveDockable = documentDock
        };

        var rightDock = new ToolDock
        {
            Id = "RightPanel",
            Alignment = Alignment.Right,
            Proportion = 0.26,
            VisibleDockables = CreateList<IDockable>(InspectorTool),
            ActiveDockable = InspectorTool
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(leftGroup, new ProportionalDockSplitter(), centerGroup, new ProportionalDockSplitter(), rightDock),
            ActiveDockable = centerGroup
        };

        var root = new RoutableRootDock(_host)
        {
            VisibleDockables = CreateList<IDockable>(mainLayout),
            DefaultDockable = mainLayout,
            ActiveDockable = mainLayout
        };

        root.LeftPinnedDockables = CreateList<IDockable>();
        root.RightPinnedDockables = CreateList<IDockable>();
        root.TopPinnedDockables = CreateList<IDockable>();
        root.BottomPinnedDockables = CreateList<IDockable>();
        root.PinnedDock = null;

        return root;
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => DockSettings.UseManagedWindows ? new ManagedHostWindow() : new HostWindow()
        };

        base.InitLayout(layout);
    }
}
