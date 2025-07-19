using Dock.Settings;
using Microsoft.Extensions.Options;

namespace Dock.Model.Extensions.DependencyInjection;

/// <summary>
/// Applies <see cref="DockSettingsOptions"/> values to <see cref="DockSettings"/>.
/// </summary>
internal sealed class DockSettingsOptionsPostConfigurator : IPostConfigureOptions<DockSettingsOptions>
{
    /// <inheritdoc />
    public void PostConfigure(string? name, DockSettingsOptions options)
    {
        if (options.MinimumHorizontalDragDistance != null)
        {
            DockSettings.MinimumHorizontalDragDistance = options.MinimumHorizontalDragDistance.Value;
        }

        if (options.MinimumVerticalDragDistance != null)
        {
            DockSettings.MinimumVerticalDragDistance = options.MinimumVerticalDragDistance.Value;
        }

        if (options.EnableWindowMagnetism != null)
        {
            DockSettings.EnableWindowMagnetism = options.EnableWindowMagnetism.Value;
        }

        if (options.WindowMagnetDistance != null)
        {
            DockSettings.WindowMagnetDistance = options.WindowMagnetDistance.Value;
        }
    }
}

