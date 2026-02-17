// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Dock.Model.Core;

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

        if (options.UseManagedWindows != null)
        {
            DockSettings.UseManagedWindows = options.UseManagedWindows.Value;
        }

        if (options.FloatingWindowHostMode != null)
        {
            DockSettings.FloatingWindowHostMode = options.FloatingWindowHostMode.Value;
        }

        if (options.UseOwnerForFloatingWindows != null)
        {
            DockSettings.UseOwnerForFloatingWindows = options.UseOwnerForFloatingWindows.Value;
        }

        if (options.FloatingWindowOwnerPolicy != null)
        {
            DockSettings.FloatingWindowOwnerPolicy = options.FloatingWindowOwnerPolicy.Value;
        }

        if (options.DefaultFloatingWindowOwnerMode != null)
        {
            DockSettings.DefaultFloatingWindowOwnerMode = options.DefaultFloatingWindowOwnerMode.Value;
        }

        if (options.EnableWindowMagnetism != null)
        {
            DockSettings.EnableWindowMagnetism = options.EnableWindowMagnetism.Value;
        }

        if (options.WindowMagnetDistance != null)
        {
            DockSettings.WindowMagnetDistance = options.WindowMagnetDistance.Value;
        }

        if (options.BringWindowsToFrontOnDrag != null)
        {
            DockSettings.BringWindowsToFrontOnDrag = options.BringWindowsToFrontOnDrag.Value;
        }

        if (options.CloseFloatingWindowsOnMainWindowClose != null)
        {
            DockSettings.CloseFloatingWindowsOnMainWindowClose = options.CloseFloatingWindowsOnMainWindowClose.Value;
        }

        if (options.ShowDockablePreviewOnDrag != null)
        {
            DockSettings.ShowDockablePreviewOnDrag = options.ShowDockablePreviewOnDrag.Value;
        }

        if (options.DragPreviewOpacity != null)
        {
            DockSettings.DragPreviewOpacity = options.DragPreviewOpacity.Value;
        }

        if (options.UpdateItemsSourceOnUnregister != null)
        {
            DockSettings.UpdateItemsSourceOnUnregister = options.UpdateItemsSourceOnUnregister.Value;
        }

        if (options.SelectorEnabled != null)
        {
            DockSettings.SelectorEnabled = options.SelectorEnabled.Value;
        }

        if (options.DocumentSelectorKeyGesture != null)
        {
            DockSettings.DocumentSelectorKeyGesture = options.DocumentSelectorKeyGesture;
        }

        if (options.ToolSelectorKeyGesture != null)
        {
            DockSettings.ToolSelectorKeyGesture = options.ToolSelectorKeyGesture;
        }

        if (options.CommandBarMergingEnabled != null)
        {
            DockSettings.CommandBarMergingEnabled = options.CommandBarMergingEnabled.Value;
        }

        if (options.CommandBarMergingScope != null)
        {
            DockSettings.CommandBarMergingScope = options.CommandBarMergingScope.Value;
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
    /// Sets <see cref="DockSettings.UseManagedWindows"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="enable">Whether to use managed windows for floating windows.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UseManagedWindows(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.UseManagedWindows = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.FloatingWindowHostMode"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="mode">The floating window host mode.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UseFloatingWindowHostMode(
        this AppBuilder builder,
        DockFloatingWindowHostMode mode)
    {
        DockSettings.FloatingWindowHostMode = mode;
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
    /// Sets <see cref="DockSettings.FloatingWindowOwnerPolicy"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="policy">The floating window owner policy.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UseFloatingWindowOwnerPolicy(
        this AppBuilder builder,
        DockFloatingWindowOwnerPolicy policy)
    {
        DockSettings.FloatingWindowOwnerPolicy = policy;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.DefaultFloatingWindowOwnerMode"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="ownerMode">The default floating window owner mode.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UseDefaultFloatingWindowOwnerMode(
        this AppBuilder builder,
        DockWindowOwnerMode ownerMode)
    {
        DockSettings.DefaultFloatingWindowOwnerMode = ownerMode;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.EnableWindowMagnetism"/> to the given value.
    /// </summary>
    public static AppBuilder EnableWindowMagnetism(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.EnableWindowMagnetism = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.WindowMagnetDistance"/> to the given value.
    /// </summary>
    public static AppBuilder SetWindowMagnetDistance(
        this AppBuilder builder,
        double distance)
    {
        DockSettings.WindowMagnetDistance = distance;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.BringWindowsToFrontOnDrag"/> to the given value.
    /// </summary>
    public static AppBuilder BringWindowsToFrontOnDrag(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.BringWindowsToFrontOnDrag = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.CloseFloatingWindowsOnMainWindowClose"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="enable">Whether floating windows should close with the main window.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder CloseFloatingWindowsOnMainWindowClose(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.CloseFloatingWindowsOnMainWindowClose = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.ShowDockablePreviewOnDrag"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="enable">Whether to show dockable previews while dragging.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder ShowDockablePreviewOnDrag(
        this AppBuilder builder,
        bool enable = true)
    {
        DockSettings.ShowDockablePreviewOnDrag = enable;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.DragPreviewOpacity"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="opacity">Opacity from 0.0 to 1.0.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder SetDragPreviewOpacity(
        this AppBuilder builder,
        double opacity)
    {
        DockSettings.DragPreviewOpacity = opacity;
        return builder;
    }

    /// <summary>
    /// Sets <see cref="DockSettings.UpdateItemsSourceOnUnregister"/> to the given value.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="update">Whether source collections should be updated when generated items unregister.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder UpdateItemsSourceOnUnregister(
        this AppBuilder builder,
        bool update = true)
    {
        DockSettings.UpdateItemsSourceOnUnregister = update;
        return builder;
    }

}
