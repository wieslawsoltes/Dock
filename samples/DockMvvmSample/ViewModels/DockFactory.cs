using System;
using System.Collections.Generic;
using DockMvvmSample.Models.Documents;
using DockMvvmSample.Models.Tools;
using DockMvvmSample.ViewModels.Docks;
using DockMvvmSample.ViewModels.Documents;
using DockMvvmSample.ViewModels.Tools;
using DockMvvmSample.ViewModels.Views;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace DockMvvmSample.ViewModels;

public class DockFactory : Factory
{
    private readonly object _context;
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    public DockFactory(object context)
    {
        _context = context;
    }

    public override IDocumentDock CreateDocumentDock() => new CustomDocumentDock();

    public override IRootDock CreateLayout()
    {
        var document1 = new DocumentViewModel {Id = "Document1", Title = "Document1"};
        var document2 = new DocumentViewModel {Id = "Document2", Title = "Document2"};
        var document3 = new DocumentViewModel {Id = "Document3", Title = "Document3", CanClose = true};
        var tool1 = new Tool1ViewModel {Id = "Tool1", Title = "Tool1"};
        var tool2 = new Tool2ViewModel {Id = "Tool2", Title = "Tool2"};
        var tool3 = new Tool3ViewModel {Id = "Tool3", Title = "Tool3", CanDrag = false };
        var tool4 = new Tool4ViewModel {Id = "Tool4", Title = "Tool4", CanDrag = false };
        var tool5 = new Tool5ViewModel {Id = "Tool5", Title = "Tool5" };
        var tool6 = new Tool6ViewModel {Id = "Tool6", Title = "Tool6", CanClose = true, CanPin = true};
        var tool7 = new Tool7ViewModel {Id = "Tool7", Title = "Tool7", CanClose = false, CanPin = false};
        var tool8 = new Tool8ViewModel {Id = "Tool8", Title = "Tool8", CanClose = false, CanPin = true};

        var leftDock = new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    ActiveDockable = tool1,
                    VisibleDockables = CreateList<IDockable>(tool1, tool2),
                    Alignment = Alignment.Left,
                    // CanDrop = false
                },
                new ProportionalDockSplitter { CanResize = true },
                new ToolDock
                {
                    ActiveDockable = tool3,
                    VisibleDockables = CreateList<IDockable>(tool3, tool4),
                    Alignment = Alignment.Bottom,
                    CanDrag = false,
                    CanDrop = false
                }
            ),
            // CanDrop = false
        };

        var rightDock = new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    ActiveDockable = tool5,
                    VisibleDockables = CreateList<IDockable>(tool5, tool6),
                    Alignment = Alignment.Top,
                    GripMode = GripMode.Hidden
                },
                new ProportionalDockSplitter(),
                new ToolDock
                {
                    ActiveDockable = tool7,
                    VisibleDockables = CreateList<IDockable>(tool7, tool8),
                    Alignment = Alignment.Right,
                    GripMode = GripMode.AutoHide
                }
            ),
            // CanDrop = false
        };

        var documentDock = new CustomDocumentDock
        {
            IsCollapsable = false,
            ActiveDockable = document1,
            VisibleDockables = CreateList<IDockable>(document1, document2, document3),
            CanCreateDocument = true,
            // CanDrop = false,
            EnableWindowDrag = true,
            // CanCloseLastDockable = false,
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                leftDock,
                new ProportionalDockSplitter(),
                documentDock,
                new ProportionalDockSplitter(),
                rightDock
            )
        };

        var dashboardView = new DashboardViewModel
        {
            Id = "Dashboard",
            Title = "Dashboard"
        };

        var homeView = new HomeViewModel
        {
            Id = "Home",
            Title = "Home",
            ActiveDockable = mainLayout,
            VisibleDockables = CreateList<IDockable>(mainLayout)
        };

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = dashboardView;
        rootDock.DefaultDockable = homeView;
        rootDock.VisibleDockables = CreateList<IDockable>(dashboardView, homeView);

        _documentDock = documentDock;
        _rootDock = rootDock;
            
        return rootDock;
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);

        if (window != null)
        {
            window.Title = "Dock Avalonia Demo";
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
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}
