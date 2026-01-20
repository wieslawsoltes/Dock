using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class WorkspaceDockFactory : Factory
{
    private readonly IScreen _host;
    private readonly IBusyServiceFactory _busyServiceFactory;
    private readonly IDockGlobalBusyService _globalBusyService;
    private readonly IDialogServiceFactory _dialogServiceFactory;
    private readonly IDockGlobalDialogService _globalDialogService;
    private readonly IConfirmationServiceFactory _confirmationServiceFactory;
    private readonly IDockGlobalConfirmationService _globalConfirmationService;
    private readonly IWindowLifecycleService _windowLifecycleService;

    public WorkspaceDockFactory(
        IScreen host,
        IBusyServiceFactory busyServiceFactory,
        IDockGlobalBusyService globalBusyService,
        IDialogServiceFactory dialogServiceFactory,
        IDockGlobalDialogService globalDialogService,
        IConfirmationServiceFactory confirmationServiceFactory,
        IDockGlobalConfirmationService globalConfirmationService,
        IWindowLifecycleService windowLifecycleService)
    {
        _host = host;
        _busyServiceFactory = busyServiceFactory;
        _globalBusyService = globalBusyService;
        _dialogServiceFactory = dialogServiceFactory;
        _globalDialogService = globalDialogService;
        _confirmationServiceFactory = confirmationServiceFactory;
        _globalConfirmationService = globalConfirmationService;
        _windowLifecycleService = windowLifecycleService;

        WindowLifecycleServices.Add(_windowLifecycleService);
    }

    public override IRootDock CreateRootDock()
        => new BusyRootDock(
            _host,
            _busyServiceFactory.Create(),
            _globalBusyService,
            _dialogServiceFactory.Create(),
            _globalDialogService,
            _confirmationServiceFactory.Create(),
            _globalConfirmationService)
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
