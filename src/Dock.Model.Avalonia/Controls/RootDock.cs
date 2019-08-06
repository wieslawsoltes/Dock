// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using Avalonia;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Root dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class RootDock : DockBase, IRootDock
    {
        /// <summary>
        /// Defines the <see cref="Window"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockWindow> WindowProperty =
            AvaloniaProperty.Register<RootDock, IDockWindow>(nameof(WindowProperty));

        /// <summary>
        /// Defines the <see cref="Top"/> property.
        /// </summary>
        public static readonly StyledProperty<IPinDock> TopProperty =
            AvaloniaProperty.Register<RootDock, IPinDock>(nameof(TopProperty));

        /// <summary>
        /// Defines the <see cref="Bottom"/> property.
        /// </summary>
        public static readonly StyledProperty<IPinDock> BottomProperty =
            AvaloniaProperty.Register<RootDock, IPinDock>(nameof(BottomProperty));

        /// <summary>
        /// Defines the <see cref="Left"/> property.
        /// </summary>
        public static readonly StyledProperty<IPinDock> LeftProperty =
            AvaloniaProperty.Register<RootDock, IPinDock>(nameof(LeftProperty));

        /// <summary>
        /// Defines the <see cref="Right"/> property.
        /// </summary>
        public static readonly StyledProperty<IPinDock> RightProperty =
            AvaloniaProperty.Register<RootDock, IPinDock>(nameof(RightProperty));

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDockWindow Window
        {
            get { return GetValue(WindowProperty); }
            set { SetValue(WindowProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Top
        {
            get { return GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Bottom
        {
            get { return GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Left
        {
            get { return GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Right
        {
            get { return GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }
    }
}
