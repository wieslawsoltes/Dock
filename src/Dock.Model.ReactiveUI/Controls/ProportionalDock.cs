using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Proportional dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ProportionalDock : DockBase, IProportionalDock
    {
        private Orientation _orientation;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public Orientation Orientation
        {
            get => _orientation;
            set => this.RaiseAndSetIfChanged(ref _orientation, value);
        }
    }
}
