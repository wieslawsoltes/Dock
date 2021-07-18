using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Adapters;
using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Core
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class DockBase : DockableBase, IDock
    {
        internal readonly INavigateAdapter _navigateAdapter;
        private IList<IDockable>? _visibleDockables;
        private IList<IDockable>? _hiddenDockables;
        private IList<IDockable>? _pinnedDockables;
        private IDockable? _activeDockable;
        private IDockable? _defaultDockable;
        private IDockable? _focusedDockable;
        private double _proportion = double.NaN;
        private DockMode _dock = DockMode.Center;
        private bool _isCollapsable = true;
        private bool _isActive;

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        protected DockBase()
        {
            _navigateAdapter = new NavigateAdapter(this);
            GoBack = ReactiveCommand.Create(() => _navigateAdapter.GoBack());
            GoForward = ReactiveCommand.Create(() => _navigateAdapter.GoForward());
            Navigate = ReactiveCommand.Create<object>(root => _navigateAdapter.Navigate(root, true));
            Close = ReactiveCommand.Create(() => _navigateAdapter.Close());
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable>? VisibleDockables
        {
            get => _visibleDockables;
            set => this.RaiseAndSetIfChanged(ref _visibleDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable>? HiddenDockables
        {
            get => _hiddenDockables;
            set => this.RaiseAndSetIfChanged(ref _hiddenDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable>? PinnedDockables
        {
            get => _pinnedDockables;
            set => this.RaiseAndSetIfChanged(ref _pinnedDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable? ActiveDockable
        {
            get => _activeDockable;
            set
            {
                this.RaiseAndSetIfChanged(ref _activeDockable, value);
                Factory?.OnActiveDockableChanged(value);
                if (value is { })
                {
                    Factory?.UpdateDockable(value, this);
                    value.OnSelected();
                }
                if (value is { })
                {
                    Factory?.SetFocusedDockable(this, value);
                }
                this.RaisePropertyChanged(nameof(CanGoBack));
                this.RaisePropertyChanged(nameof(CanGoForward));
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable? DefaultDockable
        {
            get => _defaultDockable;
            set => this.RaiseAndSetIfChanged(ref _defaultDockable, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable? FocusedDockable
        {
            get => _focusedDockable;
            set
            {
                this.RaiseAndSetIfChanged(ref _focusedDockable, value);
                Factory?.OnFocusedDockableChanged(value);
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public double Proportion
        {
            get => _proportion;
            set => this.RaiseAndSetIfChanged(ref _proportion, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public DockMode Dock
        {
            get => _dock;
            set => this.RaiseAndSetIfChanged(ref _dock, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsCollapsable
        {
            get => _isCollapsable;
            set => this.RaiseAndSetIfChanged(ref _isCollapsable, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoBack => _navigateAdapter.CanGoBack;

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoForward => _navigateAdapter.CanGoForward;

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand GoBack { get; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand GoForward { get; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand Navigate { get; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public ICommand Close { get; }
    }
}
