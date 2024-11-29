using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Docking panel dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class DockDock : DockBase, IDockDock
{
    /// <summary>
    /// Initializes new instance of the <see cref="DockDock"/> class.
    /// </summary>
    public DockDock()
    {
        _lastChildFill = true;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool LastChildFill { get; set; }
}
