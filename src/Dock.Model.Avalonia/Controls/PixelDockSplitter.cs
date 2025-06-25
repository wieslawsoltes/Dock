using System.Runtime.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Pixel dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class PixelDockSplitter : DockBase, IPixelDockSplitter
{
    /// <summary>
    /// Initializes new instance of the <see cref="PixelDockSplitter"/> class.
    /// </summary>
    public PixelDockSplitter()
    {
    }
}
