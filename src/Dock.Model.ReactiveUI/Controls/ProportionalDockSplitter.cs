using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Proportional dock splitter.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
    {
    }
}
