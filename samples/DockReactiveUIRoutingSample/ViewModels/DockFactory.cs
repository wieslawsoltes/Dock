using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Navigation.Controls;

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
        var document1 = new DocumentViewModel { Id = "Doc1", Title = "Document 1" };
        var tool1 = new ToolViewModel { Id = "Tool1", Title = "Tool 1" };

        var documentDock = new DocumentDock
        {
            Id = "Documents",
            VisibleDockables = CreateList<IDockable>(document1),
            ActiveDockable = document1,
            CanCreateDocument = false
        };

        var toolDock = new ToolDock
        {
            Id = "Tools",
            VisibleDockables = CreateList<IDockable>(tool1),
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

        return root;
    }
}
