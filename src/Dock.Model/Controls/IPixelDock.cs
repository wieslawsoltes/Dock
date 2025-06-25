using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Dock defined by absolute pixel size.
/// </summary>
public interface IPixelDock : IDock
{
    /// <summary>
    /// Gets or sets layout orientation.
    /// </summary>
    Orientation Orientation { get; set; }
}
