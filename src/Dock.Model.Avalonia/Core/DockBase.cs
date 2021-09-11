using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using Dock.Model.Adapters;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Core
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class DockBase : DockableBase, IDock
    {
        /// <summary>
        /// Defines the <see cref="VisibleDockables"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IList<IDockable>?> VisibleDockablesProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IList<IDockable>?>(nameof(VisibleDockables), o => o.VisibleDockables, (o, v) => o.VisibleDockables = v);

        /// <summary>
        /// Defines the <see cref="HiddenDockables"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IList<IDockable>?> HiddenDockablesProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IList<IDockable>?>(nameof(HiddenDockables), o => o.HiddenDockables, (o, v) => o.HiddenDockables = v);

        /// <summary>
        /// Defines the <see cref="PinnedDockables"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IList<IDockable>?> PinnedDockablesProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IList<IDockable>?>(nameof(PinnedDockables), o => o.PinnedDockables, (o, v) => o.PinnedDockables = v);

        /// <summary>
        /// Defines the <see cref="ActiveDockable"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IDockable?> ActiveDockableProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IDockable?>(nameof(ActiveDockable), o => o.ActiveDockable, (o, v) => o.ActiveDockable = v);

        /// <summary>
        /// Defines the <see cref="DefaultDockable"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IDockable?> DefaultDockableProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IDockable?>(nameof(DefaultDockable), o => o.DefaultDockable, (o, v) => o.DefaultDockable = v);

        /// <summary>
        /// Defines the <see cref="FocusedDockable"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IDockable?> FocusedDockableProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IDockable?>(nameof(FocusedDockable), o => o.FocusedDockable, (o, v) => o.FocusedDockable = v);

        /// <summary>
        /// Defines the <see cref="Proportion"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, double> ProportionProperty =
            AvaloniaProperty.RegisterDirect<DockBase, double>(nameof(Proportion), o => o.Proportion, (o, v) => o.Proportion = v, double.NaN);

        /// <summary>
        /// Defines the <see cref="Proportion"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, DockMode> DockProperty =
            AvaloniaProperty.RegisterDirect<DockBase, DockMode>(nameof(Dock), o => o.Dock, (o, v) => o.Dock = v);

        /// <summary>
        /// Defines the <see cref="IsActive"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, bool> IsActiveProperty =
            AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(IsActive), o => o.IsActive, (o, v) => o.IsActive = v);

        /// <summary>
        /// Defines the <see cref="IsCollapsable"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, bool> IsCollapsableProperty =
            AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(IsCollapsable), o => o.IsCollapsable, (o, v) => o.IsCollapsable = v, true);

        /// <summary>
        /// Defines the <see cref="CanGoBack"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, bool> CanGoBackProperty =
            AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(CanGoBack), (o) => o.CanGoBack);

        /// <summary>
        /// Defines the <see cref="CanGoForward"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, bool> CanGoForwardProperty =
            AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(CanGoForward), (o) => o.CanGoForward);

        internal INavigateAdapter _navigateAdapter;
        private IList<IDockable>? _visibleDockables;
        private IList<IDockable>? _hiddenDockables;
        private IList<IDockable>? _pinnedDockables;
        private IDockable? _activeDockable;
        private IDockable? _defaultDockable;
        private IDockable? _focusedDockable;
        private double _proportion = double.NaN;
        private DockMode _dock = DockMode.Center;
        private bool _isActive;
        private bool _isCollapsable = true;
        private bool _canGoBack;
        private bool _canGoForward;

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        public DockBase()
        {
            _navigateAdapter = new NavigateAdapter(this);
            _visibleDockables = new AvaloniaList<IDockable>();
            _hiddenDockables = new AvaloniaList<IDockable>();
            GoBack = Command.Create(() => _navigateAdapter.GoBack());
            GoForward = Command.Create(() => _navigateAdapter.GoForward());
            Navigate = Command.Create<object>(root => _navigateAdapter.Navigate(root, true));
            Close = Command.Create(() => _navigateAdapter.Close());
        }

        /// <inheritdoc/>
        [Content]
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable>? VisibleDockables
        {
            get => _visibleDockables;
            set => SetAndRaise(VisibleDockablesProperty, ref _visibleDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable>? HiddenDockables
        {
            get => _hiddenDockables;
            set => SetAndRaise(HiddenDockablesProperty, ref _hiddenDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable>? PinnedDockables
        {
            get => _pinnedDockables;
            set => SetAndRaise(PinnedDockablesProperty, ref _pinnedDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable? ActiveDockable
        {
            get => _activeDockable;
            set
            {
                SetAndRaise(ActiveDockableProperty, ref _activeDockable, value);
                Factory?.OnActiveDockableChanged(value);
                if (value != null)
                {
                    Factory?.UpdateDockable(value, this);
                    value.OnSelected();
                }
                if (value != null)
                {
                    Factory?.SetFocusedDockable(this, value);
                }
                SetAndRaise(CanGoBackProperty, ref _canGoBack, _navigateAdapter?.CanGoBack ?? false);
                SetAndRaise(CanGoForwardProperty, ref _canGoForward, _navigateAdapter?.CanGoForward ?? false);
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable? DefaultDockable
        {
            get => _defaultDockable;
            set => SetAndRaise(DefaultDockableProperty, ref _defaultDockable, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable? FocusedDockable
        {
            get => _focusedDockable;
            set
            {
                SetAndRaise(FocusedDockableProperty, ref _focusedDockable, value);
                Factory?.OnFocusedDockableChanged(value);
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public double Proportion
        {
            get => _proportion;
            set => SetAndRaise(ProportionProperty, ref _proportion, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public DockMode Dock
        {
            get => _dock;
            set => SetAndRaise(DockProperty, ref _dock, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsActive
        {
            get => _isActive;
            set => SetAndRaise(IsActiveProperty, ref _isActive, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool IsCollapsable
        {
            get => _isCollapsable;
            set => SetAndRaise(IsCollapsableProperty, ref _isCollapsable, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoBack
        {
            get => _navigateAdapter?.CanGoBack ?? false;
            private set => SetAndRaise(CanGoBackProperty, ref _canGoBack, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoForward
        {
            get => _navigateAdapter?.CanGoForward ?? false;
            private set => SetAndRaise(CanGoForwardProperty, ref _canGoForward, value);
        }

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
