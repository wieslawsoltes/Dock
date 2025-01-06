using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Proportional dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class ProportionalDock : DockBase, IProportionalDock
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial Orientation Orientation { get; set; }
}
