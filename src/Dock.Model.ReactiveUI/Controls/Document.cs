using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Document.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class Document : DockableBase, IDocument
    {
    }
}
