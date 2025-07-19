using Dock.Model.Core;
using Dock.Model.Plugins;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Controls;

namespace PluginSample;

/// <summary>
/// Example implementation of <see cref="IDockModule"/>.
/// </summary>
public class SampleModule : IDockModule
{
    /// <inheritdoc/>
    public void Register(IFactory factory)
    {
        factory.DockableLocator ??= new Dictionary<string, Func<IDockable?>>();
        factory.DockableLocator["PluginDocument"] = () => new PluginDocument
        {
            Id = "PluginDocument",
            Title = "Plugin Document"
        };
    }
}

