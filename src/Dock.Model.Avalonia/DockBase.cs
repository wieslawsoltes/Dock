// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Metadata;

namespace Dock.Model
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class DockBase : ViewBase, IDock
    {
        private INavigateAdapter _navigateAdapter;
        private bool _canGoBack;
        private bool _canGoForward;

        /// <summary>
        /// Defines the <see cref="Views"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<IView>> ViewsProperty =
            AvaloniaProperty.Register<DockBase, IList<IView>>(nameof(ViewsProperty));

        /// <summary>
        /// Defines the <see cref="CurrentView"/> property.
        /// </summary>
        public static readonly StyledProperty<IView> CurrentViewProperty =
            AvaloniaProperty.Register<DockBase, IView>(nameof(CurrentViewProperty));

        /// <summary>
        /// Defines the <see cref="DefaultView"/> property.
        /// </summary>
        public static readonly StyledProperty<IView> DefaultViewProperty =
            AvaloniaProperty.Register<DockBase, IView>(nameof(DefaultViewProperty));

        /// <summary>
        /// Defines the <see cref="FocusedView"/> property.
        /// </summary>
        public static readonly StyledProperty<IView> FocusedViewProperty =
            AvaloniaProperty.Register<DockBase, IView>(nameof(FocusedViewProperty));

        /// <summary>
        /// Defines the <see cref="Proportion"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ProportionProperty =
            AvaloniaProperty.Register<DockBase, double>(nameof(ProportionProperty), double.NaN);

        /// <summary>
        /// Defines the <see cref="IsActive"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<DockBase, bool>(nameof(IsActiveProperty));

        /// <summary>
        /// Defines the <see cref="IsCollapsable"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsCollapsableProperty =
            AvaloniaProperty.Register<DockBase, bool>(nameof(IsCollapsableProperty), true);

        /// <summary>
        /// Defines the <see cref="CanGoBack"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, bool> CanGoBackProperty =
            AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(CanGoBackProperty), (o) => o.CanGoBack);

        /// <summary>
        /// Defines the <see cref="CanGoForward"/> property.
        /// </summary>
        public static readonly DirectProperty<DockBase, bool> CanGoForwardProperty =
            AvaloniaProperty.RegisterDirect<DockBase, bool>(nameof(CanGoForwardProperty), (o) => o.CanGoForward);

        /// <summary>
        /// Defines the <see cref="Windows"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<IDockWindow>> WindowsProperty =
            AvaloniaProperty.Register<DockBase, IList<IDockWindow>>(nameof(WindowsProperty));

        /// <summary>
        /// Defines the <see cref="Factory"/> property.
        /// </summary>
        public static readonly StyledProperty<IDockFactory> FactoryProperty =
            AvaloniaProperty.Register<DockBase, IDockFactory>(nameof(FactoryProperty));

        /// <inheritdoc/>
        [Content]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<IView> Views
        {
            get { return GetValue(ViewsProperty); }
            set { SetValue(ViewsProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IView CurrentView
        {
            get { return GetValue(CurrentViewProperty); }
            set
            {
                SetValue(CurrentViewProperty, value);
                SetAndRaise(CanGoBackProperty, ref _canGoBack, _navigateAdapter.CanGoBack);
                SetAndRaise(CanGoForwardProperty, ref _canGoForward, _navigateAdapter.CanGoForward);
                Factory?.SetFocusedView(this, value);
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IView DefaultView
        {
            get { return GetValue(DefaultViewProperty); }
            set { SetValue(DefaultViewProperty, value); }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IView FocusedView
        {
            get { return GetValue(FocusedViewProperty); }
            set { SetValue(FocusedViewProperty, value); }
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
            get { return _navigateAdapter.CanGoBack; }
            private set { SetAndRaise(CanGoBackProperty, ref _canGoBack, value); }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoForward
        {
            get { return _navigateAdapter.CanGoForward; }
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
        public IDockFactory Factory
        {
            get { return GetValue(FactoryProperty); }
            set { SetValue(FactoryProperty, value); }
        }

        static DockBase()
        {
        }

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        public DockBase()
        {
            _navigateAdapter = new NavigateAdapter(this);
        }

        /// <inheritdoc/>
        public virtual void GoBack()
        {
            _navigateAdapter.GoBack();
        }

        /// <inheritdoc/>
        public virtual void GoForward()
        {
            _navigateAdapter.GoForward();
        }

        /// <inheritdoc/>
        public virtual void Navigate(object root)
        {
            _navigateAdapter.Navigate(root, true);
        }

        /// <inheritdoc/>
        public virtual void ShowWindows()
        {
            _navigateAdapter.ShowWindows();
        }

        /// <inheritdoc/>
        public virtual void ExitWindows()
        {
            _navigateAdapter.ExitWindows();
        }

        /// <inheritdoc/>
        public virtual void Close()
        {
            _navigateAdapter.Close();
        }
    }
}
