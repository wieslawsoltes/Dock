using System;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Services;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.ViewModels;

/// <summary>
/// View model that owns the dock layout and exposes overlay services.
/// </summary>
public class DockViewModel : ReactiveObject, IRoutableViewModel, IHostOverlayServices
{
    private readonly IFactory _factory;
    private IRootDock? _layout;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockViewModel"/> class.
    /// </summary>
    /// <param name="hostScreen">The host screen for routing.</param>
    /// <param name="factory">The dock factory used to create the layout.</param>
    public DockViewModel(IScreen hostScreen, IFactory factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        HostScreen = hostScreen;
        _factory = factory;

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
        }

        Layout = layout;
    }

    /// <inheritdoc />
    public string UrlPathSegment { get; } = "dock";

    /// <inheritdoc />
    public IScreen HostScreen { get; }

    /// <summary>
    /// Gets or sets the current layout.
    /// </summary>
    public IRootDock? Layout
    {
        get => _layout;
        set
        => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    private IHostOverlayServices OverlayServices
        => Layout as IHostOverlayServices
           ?? throw new InvalidOperationException("Overlay services are not available.");

    IDockBusyService IHostOverlayServices.Busy => OverlayServices.Busy;

    IDockDialogService IHostOverlayServices.Dialogs => OverlayServices.Dialogs;

    IDockConfirmationService IHostOverlayServices.Confirmations => OverlayServices.Confirmations;

    IDockGlobalBusyService IHostOverlayServices.GlobalBusyService => OverlayServices.GlobalBusyService;

    IDockGlobalDialogService IHostOverlayServices.GlobalDialogService => OverlayServices.GlobalDialogService;

    IDockGlobalConfirmationService IHostOverlayServices.GlobalConfirmationService => OverlayServices.GlobalConfirmationService;
}
