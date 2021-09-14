using System.Runtime.Serialization;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Proportional dock splitter.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ProportionalDockSplitter : DockBase, IProportionalDockSplitter
    {
        /// <summary>
        /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
        /// </summary>
        public ProportionalDockSplitter()
        {
            Id = nameof(IProportionalDockSplitter);
            Title = nameof(IProportionalDockSplitter);
        }
    }
}
