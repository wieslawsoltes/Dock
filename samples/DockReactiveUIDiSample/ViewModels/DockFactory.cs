using System;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using DockReactiveUIDiSample.Models;
using System.Collections.Generic;
using DockReactiveUIDiSample.ViewModels.Documents;
using DockReactiveUIDiSample.ViewModels.Tools;
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
        document.Context = _provider.GetRequiredService<DemoData>();

        var tool = _provider.GetRequiredService<ToolViewModel>();
        tool.Id = "Tool1";
        tool.Title = "Tool";
        tool.Context = _provider.GetRequiredService<DemoData>();

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

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Document1"] = () =>
            {
                var vm = _provider.GetRequiredService<DocumentViewModel>();
                vm.Id = "Document1";
                vm.Title = "Document";
                vm.Context = _provider.GetRequiredService<DemoData>();
                return vm;
            },
            ["Tool1"] = () =>
            {
                var vm = _provider.GetRequiredService<ToolViewModel>();
                vm.Id = "Tool1";
                vm.Title = "Tool";
                vm.Context = _provider.GetRequiredService<DemoData>();
                return vm;
            }
        };

        base.InitLayout(layout);
    }
}
