// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using Avalonia;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Pin dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class PinDock : DockBase, IPinDock
    {
        /// <summary>
        /// Defines the <see cref="Alignment"/> property.
        /// </summary>
        public static readonly DirectProperty<PinDock, Alignment> AlignmentProperty =
            AvaloniaProperty.RegisterDirect<PinDock, Alignment>(nameof(Alignment), o => o.Alignment, (o, v) => o.Alignment = v, Alignment.Unset);

        /// <summary>
        /// Defines the <see cref="IsExpanded"/> property.
        /// </summary>
        public static readonly DirectProperty<PinDock, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<PinDock, bool>(nameof(IsExpanded), o => o.IsExpanded, (o, v) => o.IsExpanded = v, false);

        /// <summary>
        /// Defines the <see cref="AutoHide"/> property.
        /// </summary>
        public static readonly DirectProperty<PinDock, bool> AutoHideProperty =
            AvaloniaProperty.RegisterDirect<PinDock, bool>(nameof(AutoHide), o => o.AutoHide, (o, v) => o.AutoHide = v, true);

        private Alignment _alignment = Alignment.Unset;
        private bool _isExpanded = false;
        private bool _autoHide = true;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Alignment Alignment
        {
            get => _alignment;
            set => SetAndRaise(AlignmentProperty, ref _alignment, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool AutoHide
        {
            get => _autoHide;
            set => SetAndRaise(AutoHideProperty, ref _autoHide, value);
        }

        /// <inheritdoc/>
        public override IDockable Clone()
        {
            return CloneHelper.ClonePinDock(this);
        }
    }
}
