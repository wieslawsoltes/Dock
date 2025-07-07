using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Prism.Core;
using Dock.Model.Core;

namespace Dock.Model.Prism.Controls;

/// <summary>
/// Grid dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class GridDockSplitter : DockableBase, IGridDockSplitter
{
    private GridResizeDirection _resizeDirection;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public GridResizeDirection ResizeDirection
    {
        get => _resizeDirection;
        set => SetProperty(ref _resizeDirection, value);
    }
}
