using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;
using Dock.Model.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Grid dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public partial class GridDockSplitter : DockableBase, IGridDockSplitter
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int Column { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int Row { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial GridResizeDirection ResizeDirection { get; set; }
}
