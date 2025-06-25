using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Corner splitter that adjusts two <see cref="IProportionalDock"/> orientations at once.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalCornerDockSplitter : DockableBase, IProportionalCornerDockSplitter
{
}
