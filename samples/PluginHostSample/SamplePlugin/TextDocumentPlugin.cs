using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using PluginContracts;

namespace SamplePlugin;

public class TextDocumentPlugin : IPlugin
{
    public IDockable CreateDockable()
    {
        return new Document { Id = "PluginDoc", Title = "Plugin Document" };
    }
}

