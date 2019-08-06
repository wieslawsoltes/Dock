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
        public static readonly StyledProperty<Alignment> AlignmentProperty =
            AvaloniaProperty.Register<PinDock, Alignment>(nameof(AlignmentProperty), Alignment.Unset);

        /// <summary>
        /// Defines the <see cref="IsExpanded"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsExpandedProperty =
            AvaloniaProperty.Register<PinDock, bool>(nameof(IsExpandedProperty), false);

        /// <summary>
        /// Defines the <see cref="AutoHide"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> AutoHideProperty =
            AvaloniaProperty.Register<PinDock, bool>(nameof(AutoHideProperty), true);

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Alignment Alignment
        {
            get { return GetValue(AlignmentProperty); }
            set { SetValue(AlignmentProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsExpanded
        {
            get { return GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool AutoHide
        {
            get { return GetValue(AutoHideProperty); }
            set { SetValue(AutoHideProperty, value); }
        }
    }
}
