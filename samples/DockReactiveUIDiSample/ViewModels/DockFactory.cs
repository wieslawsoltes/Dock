using System;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DockReactiveUIDiSample.ViewModels;

public class DockFactory : Factory
{
    private readonly IServiceProvider _provider;

    public DockFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public override IRootDock CreateLayout()
    {
        var document = _provider.GetRequiredService<DocumentViewModel>();
        document.Id = "Document1";
        document.Title = "Document";

        var tool = _provider.GetRequiredService<ToolViewModel>();
        tool.Id = "Tool1";
        tool.Title = "Tool";

        var proportionalDock = CreateProportionalDock();
        proportionalDock.Orientation = Orientation.Horizontal;
        proportionalDock.VisibleDockables = CreateList<IDockable>(
            new DocumentDock
            {
                VisibleDockables = CreateList<IDockable>(document),
                ActiveDockable = document
            },
            CreateProportionalDockSplitter(),
            new ToolDock
            {
                VisibleDockables = CreateList<IDockable>(tool),
                ActiveDockable = tool
            });

        var root = CreateRootDock();
        root.VisibleDockables = CreateList<IDockable>(proportionalDock);
        root.ActiveDockable = proportionalDock;
        root.DefaultDockable = proportionalDock;

        return root;
    }
}
