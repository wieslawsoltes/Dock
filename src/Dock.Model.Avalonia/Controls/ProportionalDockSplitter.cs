using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
[JsonSerializable(typeof(ProportionalDockSplitter), GenerationMode = JsonSourceGenerationMode.Metadata)]
public class ProportionalDockSplitter : DockBase, IProportionalDockSplitter
{
    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    [JsonConstructor]
    public ProportionalDockSplitter()
    {
    }
}
