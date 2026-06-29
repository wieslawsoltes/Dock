using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DockReactiveUIFlatSample.Models.Documents;
using DockReactiveUIFlatSample.Models.Tools;
using DockReactiveUIFlatSample.ViewModels.Docks;
using DockReactiveUIFlatSample.ViewModels.Documents;
using DockReactiveUIFlatSample.ViewModels.Tools;
using DockReactiveUIFlatSample.ViewModels.Views;
using Dock.Avalonia.Controls;
using Dock.Settings;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;

namespace DockReactiveUIFlatSample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for CustomDocumentDock.")]
[RequiresDynamicCode("Requires unreferenced code for CustomDocumentDock.")]
public class DockFactory : Factory
{
    private readonly object _context;
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    public DockFactory(object context)
    {
        _context = context;
    }

    public override IRootDock CreateRootDock() => DockingDefaults.EnableDocking(base.CreateRootDock());

    public override IProportionalDock CreateProportionalDock() => DockingDefaults.EnableDocking(base.CreateProportionalDock());

    public override IDockDock CreateDockDock() => DockingDefaults.EnableDocking(base.CreateDockDock());

    public override IStackDock CreateStackDock() => DockingDefaults.EnableDocking(base.CreateStackDock());

    public override IGridDock CreateGridDock() => DockingDefaults.EnableDocking(base.CreateGridDock());

    public override IWrapDock CreateWrapDock() => DockingDefaults.EnableDocking(base.CreateWrapDock());

    public override IUniformGridDock CreateUniformGridDock() => DockingDefaults.EnableDocking(base.CreateUniformGridDock());

    public override IProportionalDockSplitter CreateProportionalDockSplitter() => DockingDefaults.EnableDocking(base.CreateProportionalDockSplitter());

    public override IGridDockSplitter CreateGridDockSplitter() => DockingDefaults.EnableDocking(base.CreateGridDockSplitter());

    public override IToolDock CreateToolDock() => DockingDefaults.EnableDocking(base.CreateToolDock());

    public override IDocumentDock CreateDocumentDock() => DockingDefaults.EnableDocking(new CustomDocumentDock());

    public override ISplitViewDock CreateSplitViewDock() => DockingDefaults.EnableDocking(base.CreateSplitViewDock());

    public override IDocument CreateDocument() => DockingDefaults.EnableDocking(base.CreateDocument());

    public override ITool CreateTool() => DockingDefaults.EnableDocking(base.CreateTool());

    public override void AddDockable(IDock dock, IDockable dockable)
    {
        EnableDocking(dock);
        DockingDefaults.EnableDockingTree(dockable);
        base.AddDockable(dock, dockable);
    }

    public override void InsertDockable(IDock dock, IDockable dockable, int index)
    {
        EnableDocking(dock);
        DockingDefaults.EnableDockingTree(dockable);
        base.InsertDockable(dock, dockable, index);
    }

    public override IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation)
    {
        EnableDocking(dock);
        DockingDefaults.EnableDockingTree(dockable);
        return DockingDefaults.EnableDockingTree(base.CreateSplitLayout(dock, dockable, operation));
    }

    public override void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        EnableDocking(dock);
        DockingDefaults.EnableDockingTree(dockable);
        base.SplitToDock(dock, dockable, operation);
    }

    public override IRootDock CreateLayout()
    {
        var document1 = EnableDocking(new DocumentViewModel {Id = "Document1", Title = "Document1"});
        var document2 = EnableDocking(new DocumentViewModel {Id = "Document2", Title = "Document2"});
        var document3 = EnableDocking(new DocumentViewModel {Id = "Document3", Title = "Document3", CanClose = true});
        var tool1 = EnableDocking(new Tool1ViewModel {Id = "Tool1", Title = "Tool1", KeepPinnedDockableVisible = true});
        var tool2 = EnableDocking(new Tool2ViewModel {Id = "Tool2", Title = "Tool2", KeepPinnedDockableVisible = true});
        var tool3 = EnableDocking(new Tool3ViewModel {Id = "Tool3", Title = "Tool3"});
        var tool4 = EnableDocking(new Tool4ViewModel {Id = "Tool4", Title = "Tool4"});
        var tool5 = EnableDocking(new Tool5ViewModel {Id = "Tool5", Title = "Tool5"});
        var tool6 = EnableDocking(new Tool6ViewModel {Id = "Tool6", Title = "Tool6", CanClose = true, CanPin = true});
        var tool7 = EnableDocking(new Tool7ViewModel {Id = "Tool7", Title = "Tool7", CanClose = false, CanPin = false});
        var tool8 = EnableDocking(new Tool8ViewModel {Id = "Tool8", Title = "Tool8", CanClose = false, CanPin = true});

        var leftDock = EnableDocking(new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                EnableDocking(new ToolDock
                {
                    ActiveDockable = tool1,
                    VisibleDockables = CreateList<IDockable>(tool1, tool2),
                    Alignment = Alignment.Left
                }),
                CreateProportionalDockSplitter(),
                EnableDocking(new ToolDock
                {
                    ActiveDockable = tool3,
                    VisibleDockables = CreateList<IDockable>(tool3, tool4),
                    Alignment = Alignment.Bottom
                })
            )
        });

        var rightDock = EnableDocking(new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                EnableDocking(new ToolDock
                {
                    ActiveDockable = tool5,
                    VisibleDockables = CreateList<IDockable>(tool5, tool6),
                    Alignment = Alignment.Top,
                    GripMode = GripMode.Visible
                }),
                CreateProportionalDockSplitter(),
                EnableDocking(new ToolDock
                {
                    ActiveDockable = tool7,
                    VisibleDockables = CreateList<IDockable>(tool7, tool8),
                    Alignment = Alignment.Right,
                    GripMode = GripMode.Visible
                })
            )
        });

        var documentDock = EnableDocking(new CustomDocumentDock
        {
            IsCollapsable = false,
            ActiveDockable = document1,
            VisibleDockables = CreateList<IDockable>(document1, document2, document3),
            CanCreateDocument = true,
            EnableWindowDrag = true
        });

        var mainLayout = EnableDocking(new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                leftDock,
                CreateProportionalDockSplitter(),
                documentDock,
                CreateProportionalDockSplitter(),
                rightDock
            )
        });

        var dashboardView = EnableDocking(new DashboardViewModel
        {
            Id = "Dashboard",
            Title = "Dashboard"
        });

        var homeView = EnableDocking(new HomeViewModel
        {
            Id = "Home",
            Title = "Home",
            ActiveDockable = mainLayout,
            VisibleDockables = CreateList<IDockable>(mainLayout)
        });

        var rootDock = EnableDocking(CreateRootDock());

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = dashboardView;
        rootDock.DefaultDockable = homeView;
        rootDock.VisibleDockables = CreateList<IDockable>(dashboardView, homeView);

        rootDock.LeftPinnedDockables = CreateList<IDockable>();
        rootDock.RightPinnedDockables = CreateList<IDockable>();
        rootDock.TopPinnedDockables = CreateList<IDockable>();
        rootDock.BottomPinnedDockables = CreateList<IDockable>();

        rootDock.PinnedDock = null;

        _documentDock = documentDock;
        _rootDock = rootDock;
            
        return DockingDefaults.EnableDockingTree(rootDock);
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);

        if (window != null)
        {
            window.Title = "Dock Avalonia Demo";
            if (window.Layout is { } layout)
            {
                DockingDefaults.EnableDockingTree(layout);
            }
        }
        return window;
    }

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            ["Document1"] = () => new DemoDocument(),
            ["Document2"] = () => new DemoDocument(),
            ["Document3"] = () => new DemoDocument(),
            ["Tool1"] = () => new Tool1(),
            ["Tool2"] = () => new Tool2(),
            ["Tool3"] = () => new Tool3(),
            ["Tool4"] = () => new Tool4(),
            ["Tool5"] = () => new Tool5(),
            ["Tool6"] = () => new Tool6(),
            ["Tool7"] = () => new Tool7(),
            ["Tool8"] = () => new Tool8(),
            ["Dashboard"] = () => layout,
            ["Home"] = () => _context
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            ["Root"] = () => _rootDock,
            ["Documents"] = () => _documentDock
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => DockSettings.UseManagedWindows ? new ManagedHostWindow() : new HostWindow()
        };

        base.InitLayout(layout);
        DockingDefaults.EnableDockingTree(layout);
    }

    public override void InitDockWindow(IDockWindow window, IDockable? owner)
    {
        InitDockWindow(window, owner, GetHostWindow(window.Id));
    }

    public override void InitDockWindow(IDockWindow window, IDockable? owner, IHostWindow? hostWindow)
    {
        EnableDockWindowDocking(window, owner);
        base.InitDockWindow(window, owner, hostWindow);
    }

    private static void EnableDockWindowDocking(IDockWindow window, IDockable? owner)
    {
        if (owner is not null)
        {
            EnableDocking(owner);
        }

        if (window.Layout is { } layout)
        {
            DockingDefaults.EnableDockingTree(layout);
        }
    }

    private static T EnableDocking<T>(T dockable) where T : IDockable
    {
        return DockingDefaults.EnableDocking(dockable);
    }
}
