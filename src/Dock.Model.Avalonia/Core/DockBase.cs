// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;

namespace Dock.Model
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
        public static readonly DirectProperty<DockBase, IList<IDockable>> VisibleDockablesProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IList<IDockable>>(nameof(VisibleDockables), o => o.VisibleDockables, (o, v) => o.VisibleDockables = v);

        /// <summary>
        /// Defines the <see cref="HiddenDockables"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IList<IDockable>> HiddenDockablesProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IList<IDockable>>(nameof(HiddenDockables), o => o.HiddenDockables, (o, v) => o.HiddenDockables = v);

        /// <summary>
        /// Defines the <see cref="PinnedDockables"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IList<IDockable>> PinnedDockablesProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IList<IDockable>>(nameof(PinnedDockables), o => o.PinnedDockables, (o, v) => o.PinnedDockables = v);

        /// <summary>
        /// Defines the <see cref="ActiveDockable"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IDockable> ActiveDockableProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IDockable>(nameof(ActiveDockable), o => o.ActiveDockable, (o, v) => o.ActiveDockable = v);

        /// <summary>
        /// Defines the <see cref="DefaultDockable"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IDockable> DefaultDockableProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IDockable>(nameof(DefaultDockable), o => o.DefaultDockable, (o, v) => o.DefaultDockable = v);

        /// <summary>
        /// Defines the <see cref="FocusedDockable"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IDockable> FocusedDockableProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IDockable>(nameof(FocusedDockable), o => o.FocusedDockable, (o, v) => o.FocusedDockable = v);

        /// <summary>
        /// Defines the <see cref="Proportion"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, double> ProportionProperty =
            AvaloniaProperty.RegisterDirect<DockBase, double>(nameof(Proportion), o => o.Proportion, (o, v) => o.Proportion = v, double.NaN);

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

        /// <summary>
        /// Defines the <see cref="Factory"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, IFactory> FactoryProperty =
            AvaloniaProperty.RegisterDirect<DockBase, IFactory>(nameof(Factory), o => o.Factory, (o, v) => o.Factory = v);

        internal INavigateAdapter _navigateAdapter;
        private IList<IDockable> _visibleDockables;
        private IList<IDockable> _hiddenDockables;
        private IList<IDockable> _pinnedDockables;
        private IDockable _activeDockable;
        private IDockable _defaultDockable;
        private IDockable _focusedDockable;
        private double _proportion = double.NaN;
        private bool _isActive;
        private bool _isCollapsable = true;
        private bool _canGoBack;
        private bool _canGoForward;
        private IFactory _factory;

        /// <inheritdoc/>
        [Content]
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable> VisibleDockables
        {
            get => _visibleDockables;
            set => SetAndRaise(VisibleDockablesProperty, ref _visibleDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable> HiddenDockables
        {
            get => _hiddenDockables;
            set => SetAndRaise(HiddenDockablesProperty, ref _hiddenDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IList<IDockable> PinnedDockables
        {
            get => _pinnedDockables;
            set => SetAndRaise(PinnedDockablesProperty, ref _pinnedDockables, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable ActiveDockable
        {
            get => _activeDockable;
            set
            {
                SetAndRaise(ActiveDockableProperty, ref _activeDockable, value);
                if (value != null)
                {
                    Factory?.UpdateDockable(value, this);
                    value.OnSelected();
                }
                Factory?.SetFocusedDockable(this, value);
                SetAndRaise(CanGoBackProperty, ref _canGoBack, _navigateAdapter?.CanGoBack ?? false);
                SetAndRaise(CanGoForwardProperty, ref _canGoForward, _navigateAdapter?.CanGoForward ?? false);
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable DefaultDockable
        {
            get => _defaultDockable;
            set => SetAndRaise(DefaultDockableProperty, ref _defaultDockable, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public IDockable FocusedDockable
        {
            get => _focusedDockable;
            set => SetAndRaise(FocusedDockableProperty, ref _focusedDockable, value);
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
        public IFactory Factory
        {
            get => _factory;
            set => SetAndRaise(FactoryProperty, ref _factory, value);
        }

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        public DockBase()
        {
            _navigateAdapter = new NavigateAdapter(this);
            _visibleDockables = new AvaloniaList<IDockable>();
            _hiddenDockables = new AvaloniaList<IDockable>();
        }

        /// <inheritdoc/>
        public virtual void GoBack()
        {
            _navigateAdapter?.GoBack();
        }

        /// <inheritdoc/>
        public virtual void GoForward()
        {
            _navigateAdapter?.GoForward();
        }

        /// <inheritdoc/>
        public virtual void Navigate(object root)
        {
            _navigateAdapter.Navigate(root, true);
        }

        /// <inheritdoc/>
        public virtual void Close()
        {
            _navigateAdapter?.Close();
        }
    }
}
