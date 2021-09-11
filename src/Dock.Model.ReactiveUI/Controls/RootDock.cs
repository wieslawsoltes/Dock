using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls
{
    /// <summary>
    /// Root dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class RootDock : DockBase, IRootDock
    {
        private bool _isFocusableRoot = true;
        private IDockWindow? _window;
        private IList<IDockWindow>? _windows;

        /// <summary>
        /// Initializes new instance of the <see cref="RootDock"/> class.
        /// </summary>
        public RootDock()
        {
            ShowWindows = ReactiveCommand.Create(() => _navigateAdapter.ShowWindows());
            ExitWindows = ReactiveCommand.Create(() => _navigateAdapter.ExitWindows());
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsFocusableRoot
        {
            get => _isFocusableRoot;
            set => this.RaiseAndSetIfChanged(ref _isFocusableRoot, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockWindow? Window
        {
            get => _window;
            set => this.RaiseAndSetIfChanged(ref _window, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockWindow>? Windows
        {
            get => _windows;
            set => this.RaiseAndSetIfChanged(ref _windows, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand ShowWindows { get; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand ExitWindows { get; }
    }
}
