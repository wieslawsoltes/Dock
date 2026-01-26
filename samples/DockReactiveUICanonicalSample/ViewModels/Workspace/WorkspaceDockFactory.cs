using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Settings;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Services.Overlays.Controls;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class WorkspaceDockFactory : Factory
{
    private readonly IScreen _host;
    private readonly Func<IHostOverlayServices> _hostServicesFactory;
    private readonly IWindowLifecycleService _windowLifecycleService;

    public WorkspaceDockFactory(
        IScreen host,
        Func<IHostOverlayServices> hostServicesFactory,
        IWindowLifecycleService windowLifecycleService)
    {
        _host = host;
        _hostServicesFactory = hostServicesFactory;
        _windowLifecycleService = windowLifecycleService;

        WindowLifecycleServices.Add(_windowLifecycleService);
    }

    public override IRootDock CreateRootDock()
        => new BusyRootDock(
            _host,
            _hostServicesFactory)
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
            [nameof(IDockWindow)] = () => DockSettings.UseManagedWindows ? new ManagedHostWindow() : new HostWindow()
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
