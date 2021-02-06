using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Splitter dockable.
    /// </summary>
    [DataContract(IsReference = true)]
    public class SplitterDockable : DockableBase, ISplitterDockable
    {
    }
}
