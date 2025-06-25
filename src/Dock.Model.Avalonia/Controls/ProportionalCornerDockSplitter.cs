using System.Runtime.Serialization;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Corner splitter that adjusts two <see cref="IProportionalDock"/> orientations at once.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalCornerDockSplitter : DockBase, IProportionalCornerDockSplitter
{
}
