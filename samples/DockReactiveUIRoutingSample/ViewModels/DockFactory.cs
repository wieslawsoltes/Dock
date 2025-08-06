using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUIRoutingSample.ViewModels.Documents;
using DockReactiveUIRoutingSample.ViewModels.Tools;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels;

public class DockFactory : Factory
{
    private readonly IScreen _host;

    public DockFactory(IScreen host)
    {
        _host = host;
    }

    public override IRootDock CreateLayout()
    {
        var document1 = new DocumentViewModel(_host) { Id = "Doc1", Title = "Document 1" };
        var document2 = new DocumentViewModel(_host) { Id = "Doc2", Title = "Document 2" };
        var tool1 = new ToolViewModel(_host) { Id = "Tool1", Title = "Tool 1" };
        var tool2 = new ToolViewModel(_host) { Id = "Tool2", Title = "Tool 2" };

        document1.InitNavigation(document2, tool1, tool2);
        document2.InitNavigation(document1, tool1, tool2);
        tool1.InitNavigation(document1, document2, tool2);
        tool2.InitNavigation(document1, document2, tool1);

        var documentDock = new DocumentDock
        {
            Id = "Documents",
            VisibleDockables = CreateList<IDockable>(document1, document2),
            ActiveDockable = document1,
            CanCreateDocument = false
        };

        var toolDock = new ToolDock
        {
            Id = "Tools",
            VisibleDockables = CreateList<IDockable>(tool1, tool2),
            ActiveDockable = tool1,
            Alignment = Alignment.Left,
            Proportion = 0.25
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(toolDock, new ProportionalDockSplitter(), documentDock),
            ActiveDockable = documentDock
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
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}
