using System.Runtime.Serialization;
using Avalonia;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Proportional dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ProportionalDock : DockBase, IProportionalDock
    {
        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly DirectProperty<ProportionalDock, Orientation> OrientationProperty =
            AvaloniaProperty.RegisterDirect<ProportionalDock, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

        private Orientation _orientation;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public Orientation Orientation
        {
            get => _orientation;
            set => SetAndRaise(OrientationProperty, ref _orientation, value);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="ProportionalDock"/> class.
        /// </summary>
        public ProportionalDock()
        {
            Id = nameof(IProportionalDock);
            Title = nameof(IProportionalDock);
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return CloneHelper.CloneProportionalDock(this);
        }
    }
}
