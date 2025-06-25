using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Corner splitter that adjusts two <see cref="IProportionalDock"/> orientations at once.
/// </summary>
[DataContract(IsReference = true)]
public partial class ProportionalCornerDockSplitter : DockableBase, IProportionalCornerDockSplitter
{
}
