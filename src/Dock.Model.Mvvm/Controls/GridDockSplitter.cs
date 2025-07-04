using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;
using Dock.Model.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Grid dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class GridDockSplitter : DockableBase, IGridDockSplitter
{
    private int _column;
    private int _row;
    private GridResizeDirection _resizeDirection;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public GridResizeDirection ResizeDirection
    {
        get => _resizeDirection;
        set => SetProperty(ref _resizeDirection, value);
    }
}
