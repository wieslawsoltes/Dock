using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls
{
    /// <summary>
    /// Root dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class RootDock : DockBase, IRootDock
    {
        /// <summary>
        /// Defines the <see cref="IsFocusableRoot"/> property.
        /// </summary>
        public static readonly DirectProperty<RootDock, bool> IsFocusableRootProperty =
            AvaloniaProperty.RegisterDirect<RootDock, bool>(nameof(IsFocusableRoot), o => o.IsFocusableRoot, (o, v) => o.IsFocusableRoot = v, true);

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

        private bool _isFocusableRoot;
        private IDockWindow? _window;
        private IList<IDockWindow>? _windows;

        /// <summary>
        /// Initializes new instance of the <see cref="RootDock"/> class.
        /// </summary>
        public RootDock()
        {
            _isFocusableRoot = true;
            _windows = new AvaloniaList<IDockWindow>();
            ShowWindows = Command.Create(() => _navigateAdapter.ShowWindows());
            ExitWindows = Command.Create(() => _navigateAdapter.ExitWindows());
            Id = nameof(IRootDock);
            Title = nameof(IRootDock);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsFocusableRoot
        {
            get => _isFocusableRoot;
            set => SetAndRaise(IsFocusableRootProperty, ref _isFocusableRoot, value);
        }

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
        [IgnoreDataMember]
        public ICommand ShowWindows { get; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand ExitWindows { get; }
    }
}
