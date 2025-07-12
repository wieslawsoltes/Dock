// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;

namespace Dock.Settings;

/// <summary>
/// Extension methods for <see cref="AppBuilder"/>.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Configures <see cref="DockSettings"/> values using the Avalonia app builder fluent pattern.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="options">The settings to apply.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder WithDockSettings(
        this AppBuilder builder,
        DockSettingsOptions? options)
    {
        if (options == null)
        {
            return builder;
        }

        if (options.MinimumHorizontalDragDistance != null)
        {
            DockSettings.MinimumHorizontalDragDistance = options.MinimumHorizontalDragDistance.Value;
        }

        if (options.MinimumVerticalDragDistance != null)
        {
            DockSettings.MinimumVerticalDragDistance = options.MinimumVerticalDragDistance.Value;
        }

        if (options.UseFloatingDockAdorner != null)
        {
            DockSettings.UseFloatingDockAdorner = options.UseFloatingDockAdorner.Value;
        }

        if (options.UsePinnedDockWindow != null)
        {
            DockSettings.UsePinnedDockWindow = options.UsePinnedDockWindow.Value;
        }

        if (options.EnableGlobalDocking != null)
        {
            DockSettings.EnableGlobalDocking = options.EnableGlobalDocking.Value;
        }

        if (options.UseOwnerForFloatingWindows != null)
        {
            DockSettings.UseOwnerForFloatingWindows = options.UseOwnerForFloatingWindows.Value;
        }

        if (options.EnableWindowDrag != null)
        {
            DockSettings.EnableWindowDrag = options.EnableWindowDrag.Value;
        }

        if (options.ShowToolOptionsButton != null)
        {
            DockSettings.ShowToolOptionsButton = options.ShowToolOptionsButton.Value;
        }

        if (options.ShowToolPinButton != null)
        {
            DockSettings.ShowToolPinButton = options.ShowToolPinButton.Value;
        }

        if (options.ShowToolCloseButton != null)
        {
            DockSettings.ShowToolCloseButton = options.ShowToolCloseButton.Value;
        }

        if (options.ShowDocumentCloseButton != null)
        {
            DockSettings.ShowDocumentCloseButton = options.ShowDocumentCloseButton.Value;
        }

        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.UseFloatingDockAdorner"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="enable">Whether to use floating dock adorners.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UseFloatingDockAdorner(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.UseFloatingDockAdorner = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.UsePinnedDockWindow"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="enable">Whether to use floating pinned dock window.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UsePinnedDockWindow(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.UsePinnedDockWindow = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.EnableGlobalDocking"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="enable">Whether to allow docking between controls.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder EnableGlobalDocking(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.EnableGlobalDocking = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.UseOwnerForFloatingWindows"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="enable">Whether floating windows should be owned by the main window.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UseOwnerForFloatingWindows(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.UseOwnerForFloatingWindows = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.EnableWindowDrag"/> to the given value.
    /// </summary>
    public static AppBuilder EnableWindowDrag(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.EnableWindowDrag = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.ShowToolOptionsButton"/> to the given value.
    /// </summary>
    public static AppBuilder ShowToolOptionsButton(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.ShowToolOptionsButton = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.ShowToolPinButton"/> to the given value.
    /// </summary>
    public static AppBuilder ShowToolPinButton(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.ShowToolPinButton = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.ShowToolCloseButton"/> to the given value.
    /// </summary>
    public static AppBuilder ShowToolCloseButton(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.ShowToolCloseButton = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.ShowDocumentCloseButton"/> to the given value.
    /// </summary>
    public static AppBuilder ShowDocumentCloseButton(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.ShowDocumentCloseButton = enable;
        return builder;
    }
}
