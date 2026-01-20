# Overlay Services and Host Resolution

This reference describes the overlay service contracts (busy, dialog, confirmation) and the host-resolution pipeline that binds them to the correct window or dock root. The contracts are defined in `Dock.Model` so view models can use them without Avalonia dependencies. Default implementations live in `Dock.Model.ReactiveUI.Services`.

## Packages and namespaces
- `Dock.Model` (`Dock.Model.Services`): overlay service contracts and request DTOs.
- `Dock.Model.ReactiveUI.Services`: default implementations, host resolution, and DI helpers.
- `Dock.Avalonia.Controls.Overlays`: overlay controls that bind to the services.

## Overlay service contracts
### Per-host services
- `IDockBusyService`:
  - State: `IsBusy`, `Message`, `IsReloadVisible`, `CanReload`.
  - Commands: `ReloadCommand`.
  - API: `Begin`, `RunAsync`, `UpdateMessage`, `SetReloadHandler`.
- `IDockDialogService`:
  - State: `Dialogs`, `ActiveDialog`, `HasDialogs`.
  - API: `ShowAsync`, `Close`, `CancelAll`.
- `IDockConfirmationService`:
  - State: `Confirmations`, `ActiveConfirmation`, `HasConfirmations`.
  - API: `ConfirmAsync`, `Close`, `CancelAll`.

### Global services
Global services signal to other windows that a busy state or modal is active elsewhere.
- `IDockGlobalBusyService`: `IsBusy`, `Message`, `Begin`.
- `IDockGlobalDialogService`: `IsDialogOpen`, `Message`, `Begin`.
- `IDockGlobalConfirmationService`: `IsConfirmationOpen`, `Message`, `Begin`.

### Request models and dialog content
- `DialogRequest`: wraps `Content`, `Title`, a `Close` command, and a completion task.
- `ConfirmationRequest`: wraps `Title`, `Message`, button labels, confirm/cancel commands, and a completion task.
- `IDockDialogContent`: dialog content can accept a close action via `SetCloseAction`.

## Host overlay services facade
`IHostOverlayServices` provides a single entry point to all per-host and global services:
- `Busy`, `Dialogs`, `Confirmations` for per-host overlays.
- `GlobalBusyService`, `GlobalDialogService`, `GlobalConfirmationService` for cross-window overlays.

Use this interface as the backing type for view models or overlay bindings instead of injecting individual services.

## Host resolution pipeline
### Contracts
- `IHostServiceResolver` resolves services scoped to the current `IScreen`.
- `IHostOverlayServicesProvider` returns the `IHostOverlayServices` instance for a host screen.

### Default resolver
`OwnerChainHostServiceResolver` (in `Dock.Model.ReactiveUI.Services`) resolves services in this order:
1. If the screen is an `IDockable`, scan host windows in the factory and return the layout-root service for the window containing the dockable.
2. Walk the dockable owner chain and return the last matching service (outermost root).
3. Scan factory roots and floating window layouts for a layout that contains the dockable.
4. Scan the router navigation stack (last-in-first-checked) for a matching service.
5. If the screen is a `IRoutableViewModel`, follow `HostScreen` and repeat.

This keeps overlays bound to the correct host window when dockables are moved or floated.

### Provider behavior
`HostOverlayServicesProvider` uses the resolver and falls back to a cached per-screen instance when no host service is found. This keeps overlays functional in screens that are not attached to a dock layout yet.

## Overlay controls integration
Overlay controls bind directly to the service contracts:
- `BusyOverlayControl`: `BusyService`, `GlobalBusyService`.
- `DialogOverlayControl`: `DialogService`, `GlobalDialogService`.
- `ConfirmationOverlayControl`: `ConfirmationService`, `GlobalConfirmationService`.

Use `VisualTreeLifecycleBehavior.IsEnabled="True"` on the root container so overlays rebind on attach/detach and do not hold stale service references.

## Dependency registration
Splat:
```csharp
Locator.CurrentMutable.RegisterDockOverlayServices();
```

Microsoft.Extensions.DependencyInjection:
```csharp
services.AddDockOverlayServices();
```

Both registration helpers register global services, per-host adapters, the default resolver, and lifecycle helpers. Replace `IHostServiceResolver` if you need custom host lookup logic.

## Custom host resolution
Provide your own resolver when integrating with external windowing systems or non-dock hosts:
```csharp
services.AddSingleton<IHostServiceResolver, MyHostServiceResolver>();
```

Your resolver should map the current `IScreen` to the host layout or window that owns the overlay services.

## See also
- [Overlay customization](dock-overlay-customization.md)
