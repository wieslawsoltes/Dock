// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Collections;

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
        public static readonly DirectProperty<RootDock, IDockWindow?> WindowProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IDockWindow?>(nameof(Window), o => o.Window, (o, v) => o.Window = v);

        /// <summary>
        /// Defines the <see cref="Windows"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IList<IDockWindow>?> WindowsProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IList<IDockWindow>?>(nameof(Windows), o => o.Windows, (o, v) => o.Windows = v);

        /// <summary>
        /// Defines the <see cref="Top"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock?> TopProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock?>(nameof(Top), o => o.Top, (o, v) => o.Top = v);

        /// <summary>
        /// Defines the <see cref="Bottom"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock?> BottomProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock?>(nameof(Bottom), o => o.Bottom, (o, v) => o.Bottom = v);

        /// <summary>
        /// Defines the <see cref="Left"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock?> LeftProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock?>(nameof(Left), o => o.Left, (o, v) => o.Left = v);

        /// <summary>
        /// Defines the <see cref="Right"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, IPinDock?> RightProperty =
            AvaloniaProperty.RegisterDirect<RootDock, IPinDock?>(nameof(Right), o => o.Right, (o, v) => o.Right = v);

        private IDockWindow? _window;
        private IList<IDockWindow>? _windows;
        private IPinDock? _top;
        private IPinDock? _bottom;
        private IPinDock? _left;
        private IPinDock? _right;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockWindow? Window
        {
            get => _window;
            set => SetAndRaise(WindowProperty, ref _window, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockWindow>? Windows
        {
            get => _windows;
            set => SetAndRaise(WindowsProperty, ref _windows, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Top
        {
            get => _top;
            set => SetAndRaise(TopProperty, ref _top, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Bottom
        {
            get => _bottom;
            set => SetAndRaise(BottomProperty, ref _bottom, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Left
        {
            get => _left;
            set => SetAndRaise(LeftProperty, ref _left, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Right
        {
            get => _right;
            set => SetAndRaise(RightProperty, ref _right, value);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="RootDock"/> class.
        /// </summary>
        public RootDock()
        {
            _windows = new AvaloniaList<IDockWindow>();
            Id = nameof(IRootDock);
            Title = nameof(IRootDock);
        }

        /// <inheritdoc/>
        public virtual void ShowWindows()
        {
            _navigateAdapter?.ShowWindows();
        }

        /// <inheritdoc/>
        public virtual void ExitWindows()
        {
            _navigateAdapter?.ExitWindows();
        }

        /// <inheritdoc/>
        public override IDockable Clone()
        {
            return CloneHelper.CloneRootDock(this);
        }
    }
}
