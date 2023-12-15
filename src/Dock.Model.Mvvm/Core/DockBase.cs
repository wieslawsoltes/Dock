/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Model.Mvvm.Core;

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
    private double _proportion = double.NaN;
    private DockMode _dock = DockMode.Center;
    private bool _isCollapsable = true;
    private bool _isActive;
    private bool _isEmpty;

    /// <summary>
    /// Initializes new instance of the <see cref="DockBase"/> class.
    /// </summary>
    protected DockBase()
    {
        _navigateAdapter = new NavigateAdapter(this);
        GoBack = new RelayCommand(() => _navigateAdapter.GoBack());
        GoForward = new RelayCommand(() => _navigateAdapter.GoForward());
        Navigate = new RelayCommand<object>(root => _navigateAdapter.Navigate(root, true));
        Close = new RelayCommand(() => _navigateAdapter.Close());
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
    public double Proportion
    {
        get => _proportion;
        set => SetProperty(ref _proportion, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockMode Dock
    {
        get => _dock;
        set => SetProperty(ref _dock, value);
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
    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsCollapsable
    {
        get => _isCollapsable;
        set => SetProperty(ref _isCollapsable, value);
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
