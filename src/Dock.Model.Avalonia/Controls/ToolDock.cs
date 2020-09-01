using System.Runtime.Serialization;
using Avalonia;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ToolDock : DockBase, IToolDock
    {
        /// <summary>
        /// Defines the <see cref="Alignment"/> property.
        /// </summary>
        public static readonly DirectProperty<ToolDock, Alignment> AlignmentProperty =
            AvaloniaProperty.RegisterDirect<ToolDock, Alignment>(nameof(Alignment), o => o.Alignment, (o, v) => o.Alignment = v, Alignment.Unset);

        /// <summary>
        /// Defines the <see cref="IsExpanded"/> property.
        /// </summary>
        public static readonly DirectProperty<ToolDock, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(IsExpanded), o => o.IsExpanded, (o, v) => o.IsExpanded = v, false);

        /// <summary>
        /// Defines the <see cref="AutoHide"/> property.
        /// </summary>
        public static readonly DirectProperty<ToolDock, bool> AutoHideProperty =
            AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(AutoHide), o => o.AutoHide, (o, v) => o.AutoHide = v, true);

        private Alignment _alignment = Alignment.Unset;
        private bool _isExpanded = false;
        private bool _autoHide = true;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public Alignment Alignment
        {
            get => _alignment;
            set => SetAndRaise(AlignmentProperty, ref _alignment, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool AutoHide
        {
            get => _autoHide;
            set => SetAndRaise(AutoHideProperty, ref _autoHide, value);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="ToolDock"/> class.
        /// </summary>
        public ToolDock()
        {
            Id = nameof(IToolDock);
            Title = nameof(IToolDock);
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return CloneHelper.CloneToolDock(this);
        }
    }
}
