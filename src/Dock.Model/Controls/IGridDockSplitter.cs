using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Grid splitter dock contract.
/// </summary>
public interface IGridDockSplitter : IDockable
{
    /// <summary>
    /// Gets or sets resize direction.
    /// </summary>
    GridResizeDirection ResizeDirection { get; set; }
}
