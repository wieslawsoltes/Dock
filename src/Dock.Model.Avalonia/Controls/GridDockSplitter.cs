using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Grid dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class GridDockSplitter : DockableBase, IGridDockSplitter
{
    /// <summary>
    /// Defines the <see cref="Column"/> property.
    /// </summary>
    public static readonly DirectProperty<GridDockSplitter, int> ColumnProperty =
        AvaloniaProperty.RegisterDirect<GridDockSplitter, int>(nameof(Column), o => o.Column, (o, v) => o.Column = v);

    /// <summary>
    /// Defines the <see cref="Row"/> property.
    /// </summary>
    public static readonly DirectProperty<GridDockSplitter, int> RowProperty =
        AvaloniaProperty.RegisterDirect<GridDockSplitter, int>(nameof(Row), o => o.Row, (o, v) => o.Row = v);

    /// <summary>
    /// Defines the <see cref="ResizeDirection"/> property.
    /// </summary>
    public static readonly DirectProperty<GridDockSplitter, GridResizeDirection> ResizeDirectionProperty =
        AvaloniaProperty.RegisterDirect<GridDockSplitter, GridResizeDirection>(nameof(ResizeDirection), o => o.ResizeDirection, (o, v) => o.ResizeDirection = v);

    private int _column;
    private int _row;
    private GridResizeDirection _resizeDirection;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Column")]
    public int Column
    {
        get => _column;
        set => SetAndRaise(ColumnProperty, ref _column, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Row")]
    public int Row
    {
        get => _row;
        set => SetAndRaise(RowProperty, ref _row, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ResizeDirection")]
    public GridResizeDirection ResizeDirection
    {
        get => _resizeDirection;
        set => SetAndRaise(ResizeDirectionProperty, ref _resizeDirection, value);
    }
}
