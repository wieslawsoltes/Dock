using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Pixel dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public partial class PixelDockSplitter : DockableBase, IPixelDockSplitter
{
}
