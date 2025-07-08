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
    /// <param name="minimumHorizontalDragDistance">Optional horizontal drag threshold.</param>
    /// <param name="minimumVerticalDragDistance">Optional vertical drag threshold.</param>
    /// <param name="useFloatingDockAdorner">Optional floating adorner flag.</param>
    /// <param name="enableGlobalDocking">Optional global docking flag.</param>
    /// <returns>The app builder instance.</returns>
    public static AppBuilder WithDockSettings(
        this AppBuilder builder,
        double? minimumHorizontalDragDistance = null,
        double? minimumVerticalDragDistance = null,
        bool? useFloatingDockAdorner = null,
        bool? enableGlobalDocking = null)
    {
        if (minimumHorizontalDragDistance != null)
        {
            DockSettings.MinimumHorizontalDragDistance = minimumHorizontalDragDistance.Value;
        }

        if (minimumVerticalDragDistance != null)
        {
            DockSettings.MinimumVerticalDragDistance = minimumVerticalDragDistance.Value;
        }

        if (useFloatingDockAdorner != null)
        {
            DockSettings.UseFloatingDockAdorner = useFloatingDockAdorner.Value;
        }

        if (enableGlobalDocking != null)
        {
            DockSettings.EnableGlobalDocking = enableGlobalDocking.Value;
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
