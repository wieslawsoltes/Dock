using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Grid splitter dock contract.
/// </summary>
public interface IGridDockSplitter : IDockable
{
    /// <summary>
    /// Gets or sets grid column.
    /// </summary>
    int Column { get; set; }

    /// <summary>
    /// Gets or sets grid row.
    /// </summary>
    int Row { get; set; }

    /// <summary>
    /// Gets or sets resize direction.
    /// </summary>
    GridResizeDirection ResizeDirection { get; set; }
}
