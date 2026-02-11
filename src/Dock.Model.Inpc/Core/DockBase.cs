// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Model.Inpc.Core;

/// <summary>
/// Dock base class.
/// </summary>
[DataContract(IsReference = true)]
public abstract class DockBase : DockableBase, IDock
{
    internal readonly INavigateAdapter _navigateAdapter;
    private IList<IDockable>? _visibleDockables;
    private IDockable? _activeDockable;
    private IDockable? _defaultDockable;
    private IDockable? _focusedDockable;
    private int _openedDockablesCount = 0;
    private bool _isActive;
    private bool _canCloseLastDockable = true;
    private DockCapabilityPolicy? _dockCapabilityPolicy;
    private bool _enableGlobalDocking = true;

    /// <summary>
    /// Initializes new instance of the <see cref="DockBase"/> class.
    /// </summary>
    protected DockBase()
    {
        _navigateAdapter = new NavigateAdapter(this);
        GoBack = new SimpleCommand(() => _navigateAdapter.GoBack());
        GoForward = new SimpleCommand(() => _navigateAdapter.GoForward());
        Navigate = new SimpleCommand<object>(root => _navigateAdapter.Navigate(root, true));
        Close = new SimpleCommand(() => _navigateAdapter.Close());

        this.WhenPropertyChanged(x => x.ActiveDockable)
            .Subscribe(x =>
            {
                Factory?.InitActiveDockable(x, this);
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
            });

        this.WhenPropertyChanged(x => x.FocusedDockable)
            .Subscribe(x => Factory?.OnFocusedDockableChanged(x));
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? VisibleDockables
    {
        get => _visibleDockables;
        set => SetProperty(ref _visibleDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? ActiveDockable
    {
        get => _activeDockable;
        set
        {
            SetProperty(ref _activeDockable, value);
            Factory?.InitActiveDockable(value, this);
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? DefaultDockable
    {
        get => _defaultDockable;
        set => SetProperty(ref _defaultDockable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? FocusedDockable
    {
        get => _focusedDockable;
        set
        {
            SetProperty(ref _focusedDockable, value);
            Factory?.OnFocusedDockableChanged(value);
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int OpenedDockablesCount
    {
        get => _openedDockablesCount;
        set => SetProperty(ref _openedDockablesCount, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanCloseLastDockable
    {
        get => _canCloseLastDockable;
        set => SetProperty(ref _canCloseLastDockable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public DockCapabilityPolicy? DockCapabilityPolicy
    {
        get => _dockCapabilityPolicy;
        set => SetProperty(ref _dockCapabilityPolicy, value);
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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableGlobalDocking
    {
        get => _enableGlobalDocking;
        set => SetProperty(ref _enableGlobalDocking, value);
    }
}
