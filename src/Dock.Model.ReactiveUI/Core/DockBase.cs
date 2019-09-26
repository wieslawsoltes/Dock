// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Runtime.Serialization;
using ReactiveUI;

namespace Dock.Model
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class DockBase : DockableBase, IDock
    {
        internal INavigateAdapter _navigateAdapter;
        private IList<IDockable>? _visibleDockables;
        private IList<IDockable>? _hiddenDockables;
        private IList<IDockable>? _pinnedDockables;
        private IDockable? _activeDockable;
        private IDockable? _defaultDockable;
        private IDockable? _focusedDockable;
        private double _proportion = double.NaN;
        private bool _isCollapsable = true;
        private bool _isActive;

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        public DockBase()
        {
            _navigateAdapter = new NavigateAdapter(this);
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
                if (value != null)
                {
                    Factory?.UpdateDockable(value, this);
                    value.OnSelected();
                }
                if (value != null)
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
            set => this.RaiseAndSetIfChanged(ref _focusedDockable, value);
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
        public bool CanGoBack => _navigateAdapter?.CanGoBack ?? false;

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoForward => _navigateAdapter?.CanGoForward ?? false;

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
            _navigateAdapter?.Navigate(root, true);
        }

        /// <inheritdoc/>
        public virtual void Close()
        {
            _navigateAdapter?.Close();
        }
    }
}
