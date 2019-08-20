// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
        private INavigateAdapter _navigateAdapter;
        private bool _canGoBack;
        private bool _canGoForward;

        /// <summary>
        /// Defines the <see cref="VisibleDockables"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<IDockable>> VisibleDockablesProperty =
            AvaloniaProperty.Register<DockBase, IList<IDockable>>(nameof(VisibleDockables));

        /// <summary>
        /// Defines the <see cref="HiddenDockables"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<IDockable>> HiddenDockablesProperty =
            AvaloniaProperty.Register<DockBase, IList<IDockable>>(nameof(HiddenDockables));

        /// <summary>
        /// Defines the <see cref="PinnedDockables"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<IDockable>> PinnedDockablesProperty =
            AvaloniaProperty.Register<DockBase, IList<IDockable>>(nameof(PinnedDockables));

        /// <summary>
        /// Defines the <see cref="AvtiveDockable"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockable> AvtiveDockableProperty =
            AvaloniaProperty.Register<DockBase, IDockable>(nameof(AvtiveDockable));

        /// <summary>
        /// Defines the <see cref="DefaultDockable"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockable> DefaultDockableProperty =
            AvaloniaProperty.Register<DockBase, IDockable>(nameof(DefaultDockable));

        /// <summary>
        /// Defines the <see cref="FocusedDockable"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockable> FocusedDockableProperty =
            AvaloniaProperty.Register<DockBase, IDockable>(nameof(FocusedDockable));

        /// <summary>
        /// Defines the <see cref="Proportion"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ProportionProperty =
            AvaloniaProperty.Register<DockBase, double>(nameof(Proportion), double.NaN);

        /// <summary>
        /// Defines the <see cref="IsActive"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<DockBase, bool>(nameof(IsActive));

        /// <summary>
        /// Defines the <see cref="IsCollapsable"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsCollapsableProperty =
            AvaloniaProperty.Register<DockBase, bool>(nameof(IsCollapsable), true);

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
        /// Defines the <see cref="Windows"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<IDockWindow>> WindowsProperty =
            AvaloniaProperty.Register<DockBase, IList<IDockWindow>>(nameof(Windows));

        /// <summary>
        /// Defines the <see cref="Factory"/> property.
        /// </summary>
        public static readonly StyledProperty<IFactory> FactoryProperty =
            AvaloniaProperty.Register<DockBase, IFactory>(nameof(Factory));

        /// <inheritdoc/>
        [Content]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<IDockable> VisibleDockables
        {
            get { return GetValue(VisibleDockablesProperty); }
            set { SetValue(VisibleDockablesProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<IDockable> HiddenDockables
        {
            get { return GetValue(HiddenDockablesProperty); }
            set { SetValue(HiddenDockablesProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<IDockable> PinnedDockables
        {
            get { return GetValue(PinnedDockablesProperty); }
            set { SetValue(PinnedDockablesProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDockable AvtiveDockable
        {
            get { return GetValue(AvtiveDockableProperty); }
            set
            {
                SetValue(AvtiveDockableProperty, value);
                SetAndRaise(CanGoBackProperty, ref _canGoBack, _navigateAdapter?.CanGoBack ?? false);
                SetAndRaise(CanGoForwardProperty, ref _canGoForward, _navigateAdapter?.CanGoForward ?? false);
                Factory?.SetFocusedDockable(this, value);
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDockable DefaultDockable
        {
            get { return GetValue(DefaultDockableProperty); }
            set { SetValue(DefaultDockableProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IDockable FocusedDockable
        {
            get { return GetValue(FocusedDockableProperty); }
            set { SetValue(FocusedDockableProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public double Proportion
        {
            get { return GetValue(ProportionProperty); }
            set { SetValue(ProportionProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsCollapsable
        {
            get { return GetValue(IsCollapsableProperty); }
            set { SetValue(IsCollapsableProperty, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoBack
        {
            get { return _navigateAdapter?.CanGoBack ?? false; }
            private set { SetAndRaise(CanGoBackProperty, ref _canGoBack, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoForward
        {
            get { return _navigateAdapter?.CanGoForward ?? false; }
            private set { SetAndRaise(CanGoForwardProperty, ref _canGoForward, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<IDockWindow> Windows
        {
            get { return GetValue(WindowsProperty); }
            set { SetValue(WindowsProperty, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IFactory Factory
        {
            get { return GetValue(FactoryProperty); }
            set { SetValue(FactoryProperty, value); }
        }

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        public DockBase()
        {
            _navigateAdapter = new NavigateAdapter(this);
            VisibleDockables = new AvaloniaList<IDockable>();
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
        public virtual void Close()
        {
            _navigateAdapter?.Close();
        }
    }
}
