using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Tool.
    /// </summary>
    [DataContract(IsReference = true)]
    public class Tool : DockableBase, ITool, IDocument
    {
    }
}
