using System.Runtime.Serialization;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Splitter dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class SplitterDockable : DockBase, ISplitterDockable
    {
        /// <summary>
        /// Initializes new instance of the <see cref="SplitterDockable"/> class.
        /// </summary>
        public SplitterDockable()
        {
            Id = nameof(ISplitterDockable);
            Title = nameof(ISplitterDockable);
        }
    }
}
