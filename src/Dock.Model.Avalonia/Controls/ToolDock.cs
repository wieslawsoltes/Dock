using System.Runtime.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls
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
            AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(IsExpanded), o => o.IsExpanded, (o, v) => o.IsExpanded = v);

        /// <summary>
        /// Defines the <see cref="AutoHide"/> property.
        /// </summary>
        public static readonly DirectProperty<ToolDock, bool> AutoHideProperty =
            AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(AutoHide), o => o.AutoHide, (o, v) => o.AutoHide = v, true);

        /// <summary>
        /// Defines the <see cref="GripMode"/> property.
        /// </summary>
        public static readonly DirectProperty<ToolDock, GripMode> GripModeProperty =
            AvaloniaProperty.RegisterDirect<ToolDock, GripMode>(nameof(GripMode), o => o.GripMode, (o, v) => o.GripMode = v);

        private Alignment _alignment = Alignment.Unset;
        private bool _isExpanded;
        private bool _autoHide = true;
        private GripMode _gripMode = GripMode.Visible;

        /// <summary>
        /// Initializes new instance of the <see cref="ToolDock"/> class.
        /// </summary>
        public ToolDock()
        {
            Id = nameof(IToolDock);
            Title = nameof(IToolDock);
        }

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

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public GripMode GripMode
        {
            get => _gripMode;
            set => SetAndRaise(GripModeProperty, ref _gripMode, value);
        }
    }
}
