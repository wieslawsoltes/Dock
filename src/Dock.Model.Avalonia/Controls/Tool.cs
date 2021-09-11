using System.Runtime.Serialization;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Tool.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class Tool : DockableBase, ITool, IDocument
    {
        /// <summary>
        /// Initializes new instance of the <see cref="Tool"/> class.
        /// </summary>
        public Tool()
        {
            Id = nameof(ITool);
            Title = nameof(ITool);
        }
    }
}
