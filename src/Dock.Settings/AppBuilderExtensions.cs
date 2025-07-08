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

        if (options.EnableGlobalDocking != null)
        {
            DockSettings.EnableGlobalDocking = options.EnableGlobalDocking.Value;
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
}
