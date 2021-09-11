using System.Runtime.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Docking panel dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DockDock : DockBase, IDockDock
    {        
        /// <summary>
        /// Defines the <see cref="LastChildFill"/> property.
        /// </summary>
        public static readonly DirectProperty<DockDock, bool> LastChildFillProperty =
            AvaloniaProperty.RegisterDirect<DockDock, bool>(nameof(LastChildFill), o => o.LastChildFill, (o, v) => o.LastChildFill = v, true);

        private bool _lastChildFill = true;

        /// <summary>
        /// Initializes new instance of the <see cref="DockDock"/> class.
        /// </summary>
        public DockDock()
        {
            Id = nameof(IDockDock);
            Title = nameof(IDockDock);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool LastChildFill
        {
            get => _lastChildFill;
            set => SetAndRaise(LastChildFillProperty, ref _lastChildFill, value);
        }
    }
}
