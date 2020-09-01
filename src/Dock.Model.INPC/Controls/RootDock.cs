using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dock.Model.Controls
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
        private IPinDock? _top;
        private IPinDock? _bottom;
        private IPinDock? _left;
        private IPinDock? _right;

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
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Top
        {
            get => _top;
            set => this.RaiseAndSetIfChanged(ref _top, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Bottom
        {
            get => _bottom;
            set => this.RaiseAndSetIfChanged(ref _bottom, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Left
        {
            get => _left;
            set => this.RaiseAndSetIfChanged(ref _left, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IPinDock? Right
        {
            get => _right;
            set => this.RaiseAndSetIfChanged(ref _right, value);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="RootDock"/> class.
        /// </summary>
        public RootDock()
        {
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
        public override IDockable? Clone()
        {
            return CloneHelper.CloneRootDock(this);
        }
    }
}
