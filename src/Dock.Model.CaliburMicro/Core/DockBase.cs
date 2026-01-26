// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Model.CaliburMicro.Core;

/// <summary>
/// Dock base class.
/// </summary>
public abstract class DockBase : DockableBase, IDock
{
    internal readonly INavigateAdapter _navigateAdapter;
    private bool _enableGlobalDocking = true;
    private IList<IDockable>? _visibleDockables;
    private IDockable? _activeDockable;
    private IDockable? _defaultDockable;
    private IDockable? _focusedDockable;
    private bool _isActive;
    private int _openedDockablesCount;
    private bool _canCloseLastDockable = true;

    /// <summary>
    /// Initializes new instance of the <see cref="DockBase"/> class.
    /// </summary>
    protected DockBase()
    {
        _navigateAdapter = new NavigateAdapter(this);
        
        GoBack = new RelayCommand(() => _navigateAdapter.GoBack(), () => CanGoBack);
        GoForward = new RelayCommand(() => _navigateAdapter.GoForward(), () => CanGoForward);
        Navigate = new RelayCommand<object>(root => _navigateAdapter.Navigate(root, true));
        Close = new RelayCommand(() => _navigateAdapter.Close());
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? VisibleDockables
    {
        get => _visibleDockables;
        set => Set(ref _visibleDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? ActiveDockable
    {
        get => _activeDockable;
        set
        {
            if (Set(ref _activeDockable, value))
            {
                Factory?.InitActiveDockable(value, this);
                NotifyOfPropertyChange(() => CanGoBack);
                NotifyOfPropertyChange(() => CanGoForward);
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? DefaultDockable
    {
        get => _defaultDockable;
        set => Set(ref _defaultDockable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? FocusedDockable
    {
        get => _focusedDockable;
        set
        {
            if (Set(ref _focusedDockable, value))
            {
                Factory?.OnFocusedDockableChanged(value);
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsActive
    {
        get => _isActive;
        set => Set(ref _isActive, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int OpenedDockablesCount
    {
        get => _openedDockablesCount;
        set => Set(ref _openedDockablesCount, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanCloseLastDockable
    {
        get => _canCloseLastDockable;
        set => Set(ref _canCloseLastDockable, value);
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
        set => Set(ref _enableGlobalDocking, value);
    }
}
