﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
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
        public static readonly DirectProperty<RootDock, IDockWindow> WindowProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IDockWindow>(nameof(Window), o => o.Window, (o, v) => o.Window = v);

        /// <summary>
        /// Defines the <see cref="Top"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock> TopProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock>(nameof(Top), o => o.Top, (o, v) => o.Top = v);

        /// <summary>
        /// Defines the <see cref="Bottom"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock> BottomProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock>(nameof(Bottom), o => o.Bottom, (o, v) => o.Bottom = v);

        /// <summary>
        /// Defines the <see cref="Left"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock> LeftProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock>(nameof(Left), o => o.Left, (o, v) => o.Left = v);

        /// <summary>
        /// Defines the <see cref="Right"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock> RightProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock>(nameof(Right), o => o.Right, (o, v) => o.Right = v);

        private IDockWindow _window;
        private IPinDock _top;
        private IPinDock _bottom;
        private IPinDock _left;
        private IPinDock _right;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDockWindow Window
        {
            get => _window;
            set => SetAndRaise(WindowProperty, ref _window, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Top
        {
            get => _top;
            set => SetAndRaise(TopProperty, ref _top, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Bottom
        {
            get => _bottom;
            set => SetAndRaise(BottomProperty, ref _bottom, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Left
        {
            get => _left;
            set => SetAndRaise(LeftProperty, ref _left, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IPinDock Right
        {
            get => _right;
            set => SetAndRaise(RightProperty, ref _right, value);
        }

        /// <inheritdoc/>
        public override IDockable Clone()
        {
            return CloneHelper.CloneRootDock(this);
        }
    }
}
