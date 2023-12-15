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
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Root dock.
/// </summary>
[DataContract(IsReference = true)]
public class RootDock : DockBase, IRootDock
{
    private bool _isFocusableRoot = true;
    private IList<IDockable>? _hiddenDockables;
    private IList<IDockable>? _leftPinnedDockables;
    private IList<IDockable>? _rightPinnedDockables;
    private IList<IDockable>? _topPinnedDockables;
    private IList<IDockable>? _bottomPinnedDockables;
    private IDockWindow? _window;
    private IList<IDockWindow>? _windows;

    /// <summary>
    /// Initializes new instance of the <see cref="RootDock"/> class.
    /// </summary>
    public RootDock()
    {
        ShowWindows = new RelayCommand(() => _navigateAdapter.ShowWindows());
        ExitWindows = new RelayCommand(() => _navigateAdapter.ExitWindows());
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsFocusableRoot
    {
        get => _isFocusableRoot;
        set => SetProperty(ref _isFocusableRoot, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? HiddenDockables
    {
        get => _hiddenDockables;
        set => SetProperty(ref _hiddenDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? LeftPinnedDockables
    {
        get => _leftPinnedDockables;
        set => SetProperty(ref _leftPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? RightPinnedDockables
    {
        get => _rightPinnedDockables;
        set => SetProperty(ref _rightPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? TopPinnedDockables
    {
        get => _topPinnedDockables;
        set => SetProperty(ref _topPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? BottomPinnedDockables
    {
        get => _bottomPinnedDockables;
        set => SetProperty(ref _bottomPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockWindow? Window
    {
        get => _window;
        set => SetProperty(ref _window, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockWindow>? Windows
    {
        get => _windows;
        set => SetProperty(ref _windows, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ShowWindows { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ExitWindows { get; }
}
