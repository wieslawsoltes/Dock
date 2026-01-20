using System;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Services;
using Dock.Model.Services;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.Services;

/// <summary>
/// Provides helper methods for navigation and dockable operations.
/// </summary>
public static class DockNavigationHelpers
{
    /// <summary>
    /// Resolves the overlay services for the specified host screen.
    /// </summary>
    /// <param name="hostScreen">The host screen.</param>
    /// <param name="provider">The overlay services provider.</param>
    /// <returns>The overlay services instance.</returns>
    public static IHostOverlayServices GetOverlayServices(
        IScreen hostScreen,
        IHostOverlayServicesProvider provider)
    {
        if (hostScreen is null)
        {
            throw new ArgumentNullException(nameof(hostScreen));
        }

        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        return provider.GetServices(hostScreen);
    }

    /// <summary>
    /// Gets the busy service for the specified host screen.
    /// </summary>
    /// <param name="hostScreen">The host screen.</param>
    /// <param name="provider">The overlay services provider.</param>
    /// <returns>The busy service instance.</returns>
    public static IDockBusyService GetBusyService(
        IScreen hostScreen,
        IHostOverlayServicesProvider provider)
        => GetOverlayServices(hostScreen, provider).Busy;

    /// <summary>
    /// Gets the dialog service for the specified host screen.
    /// </summary>
    /// <param name="hostScreen">The host screen.</param>
    /// <param name="provider">The overlay services provider.</param>
    /// <returns>The dialog service instance.</returns>
    public static IDockDialogService GetDialogService(
        IScreen hostScreen,
        IHostOverlayServicesProvider provider)
        => GetOverlayServices(hostScreen, provider).Dialogs;

    /// <summary>
    /// Gets the confirmation service for the specified host screen.
    /// </summary>
    /// <param name="hostScreen">The host screen.</param>
    /// <param name="provider">The overlay services provider.</param>
    /// <returns>The confirmation service instance.</returns>
    public static IDockConfirmationService GetConfirmationService(
        IScreen hostScreen,
        IHostOverlayServicesProvider provider)
        => GetOverlayServices(hostScreen, provider).Confirmations;

    /// <summary>
    /// Attempts to navigate back using the host screen router.
    /// </summary>
    /// <param name="hostScreen">The host screen.</param>
    /// <returns>True when navigation was executed.</returns>
    public static bool TryNavigateBack(IScreen hostScreen)
    {
        if (hostScreen is null)
        {
            throw new ArgumentNullException(nameof(hostScreen));
        }

        if (hostScreen.Router.NavigationStack.Count <= 1)
        {
            return false;
        }

        hostScreen.Router.NavigateBack.Execute()
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
        return true;
    }

    /// <summary>
    /// Attempts to close the specified dockable by locating its factory.
    /// </summary>
    /// <param name="dockable">The dockable to close.</param>
    /// <returns>True when the dockable was closed.</returns>
    public static bool TryCloseDockable(IDockable dockable)
    {
        if (dockable is null)
        {
            throw new ArgumentNullException(nameof(dockable));
        }

        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return false;
        }

        factory.CloseDockable(dockable);
        return true;
    }

    /// <summary>
    /// Finds the nearest dock factory for a dockable by walking the owner chain.
    /// </summary>
    /// <param name="dockable">The dockable to resolve.</param>
    /// <returns>The factory instance or null.</returns>
    public static IFactory? FindFactory(IDockable dockable)
    {
        if (dockable is null)
        {
            throw new ArgumentNullException(nameof(dockable));
        }

        if (dockable.Factory is { } directFactory)
        {
            return directFactory;
        }

        IDockable? current = dockable;
        while (current is not null)
        {
            if (current.Factory is { } factory)
            {
                return factory;
            }

            current = current.Owner;
        }

        return null;
    }
}
