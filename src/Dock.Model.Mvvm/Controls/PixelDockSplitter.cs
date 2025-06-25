using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Pixel dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class PixelDockSplitter : DockableBase, IPixelDockSplitter
{
}
