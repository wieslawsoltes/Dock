using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Settings;
using DockReactiveUIWindowRelationsSample.ViewModels.Documents;
using DockReactiveUIWindowRelationsSample.ViewModels.Tools;

namespace DockReactiveUIWindowRelationsSample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for ReactiveUI view models.")]
[RequiresDynamicCode("Requires unreferenced code for ReactiveUI view models.")]
public class DockFactory : Factory
{
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;
    private IToolDock? _stagingToolDock;

    public override IRootDock CreateLayout()
    {
        var rootDock = CreateRootDock();

        var documentDock = CreateDocumentDock();
        documentDock.Id = "Documents";
        documentDock.Title = "Documents";
        documentDock.EnableWindowDrag = true;

        var document1 = new CaseDocumentViewModel { Id = "Doc1", Title = "Doc 1", Content = "Primary document." };
        var document2 = new CaseDocumentViewModel { Id = "Doc2", Title = "Doc 2", Content = "Secondary document." };
        documentDock.VisibleDockables = CreateList<IDockable>(document1, document2);
        documentDock.ActiveDockable = document1;

        var stagingToolDock = CreateToolDock();
        stagingToolDock.Id = "StagingTools";
        stagingToolDock.Title = "Staging Tools";
        stagingToolDock.Alignment = Alignment.Left;
        stagingToolDock.VisibleDockables = CreateList<IDockable>();

        var casesTool = new WindowCasesToolViewModel(this, rootDock, stagingToolDock, documentDock)
        {
            Id = "WindowCases",
            Title = "Window Cases"
        };

        var casesDock = CreateToolDock();
        casesDock.Id = "CaseDock";
        casesDock.Title = "Window Cases";
        casesDock.Alignment = Alignment.Left;
        casesDock.VisibleDockables = CreateList<IDockable>(casesTool);
        casesDock.ActiveDockable = casesTool;

        var leftDock = CreateProportionalDock();
        leftDock.Orientation = Orientation.Vertical;
        leftDock.Proportion = 0.28;
        leftDock.VisibleDockables = CreateList<IDockable>(
            casesDock,
            CreateProportionalDockSplitter(),
            stagingToolDock);

        var infoTool = new InfoToolViewModel
        {
            Id = "Info",
            Title = "Info",
            Message = "Use the Window Cases tool to open windows with different owner and modal settings."
        };

        var infoDock = CreateToolDock();
        infoDock.Id = "InfoDock";
        infoDock.Title = "Info";
        infoDock.Alignment = Alignment.Right;
        infoDock.VisibleDockables = CreateList<IDockable>(infoTool);
        infoDock.ActiveDockable = infoTool;

        var mainLayout = CreateProportionalDock();
        mainLayout.Orientation = Orientation.Horizontal;
        mainLayout.VisibleDockables = CreateList<IDockable>(
            leftDock,
            CreateProportionalDockSplitter(),
            documentDock,
            CreateProportionalDockSplitter(),
            infoDock);

        rootDock.Id = "Root";
        rootDock.Title = "Root";
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);
        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.Windows = CreateList<IDockWindow>();
        rootDock.LeftPinnedDockables = CreateList<IDockable>();
        rootDock.RightPinnedDockables = CreateList<IDockable>();
        rootDock.TopPinnedDockables = CreateList<IDockable>();
        rootDock.BottomPinnedDockables = CreateList<IDockable>();

        _rootDock = rootDock;
        _documentDock = documentDock;
        _stagingToolDock = stagingToolDock;

        return rootDock;
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);
        if (window is null)
        {
            return null;
        }

        window.Title = $"Window: {dockable.Title}";
        return window;
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () =>
            {
                var hostMode = DockSettings.ResolveFloatingWindowHostMode(layout as IRootDock);
                return hostMode == DockFloatingWindowHostMode.Managed
                    ? new ManagedHostWindow()
                    : new HostWindow();
            }
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"] = () => _rootDock,
            ["Documents"] = () => _documentDock,
            ["StagingTools"] = () => _stagingToolDock
        };

        base.InitLayout(layout);
    }
}
