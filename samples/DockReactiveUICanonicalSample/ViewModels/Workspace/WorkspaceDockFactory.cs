using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Services;
using Dock.Model.Services;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class WorkspaceDockFactory : Factory
{
    private readonly IScreen _host;
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;
    private readonly IWindowLifecycleService _windowLifecycleService;

    public WorkspaceDockFactory(
        IScreen host,
        IHostOverlayServicesProvider overlayServicesProvider,
        IWindowLifecycleService windowLifecycleService)
    {
        _host = host;
        _overlayServicesProvider = overlayServicesProvider;
        _windowLifecycleService = windowLifecycleService;

        WindowLifecycleServices.Add(_windowLifecycleService);
    }

    public override IRootDock CreateRootDock()
        => new BusyRootDock(
            _host,
            _overlayServicesProvider.GetServices(_host))
        {
            LeftPinnedDockables = CreateList<IDockable>(),
            RightPinnedDockables = CreateList<IDockable>(),
            TopPinnedDockables = CreateList<IDockable>(),
            BottomPinnedDockables = CreateList<IDockable>()
        };

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);

        if (window is not null)
        {
            window.Title = dockable.Title ?? "Dock Workspace";
        }

        return window;
    }

}
