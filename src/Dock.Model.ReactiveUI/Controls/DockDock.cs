using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Docking panel dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DockDock : DockBase, IDockDock
    {        
        private bool _lastChildFill = true;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool LastChildFill
        {
            get => _lastChildFill;
            set => this.RaiseAndSetIfChanged(ref _lastChildFill, value);
        }
    }
}
