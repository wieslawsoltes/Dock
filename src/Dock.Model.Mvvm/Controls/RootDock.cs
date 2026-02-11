// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
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
public class RootDock : DockBase, IRootDock, ILocalTarget
{
    private bool _isFocusableRoot = true;
    private IList<IDockable>? _hiddenDockables;
    private IList<IDockable>? _leftPinnedDockables;
    private IList<IDockable>? _rightPinnedDockables;
    private IList<IDockable>? _topPinnedDockables;
    private IList<IDockable>? _bottomPinnedDockables;
    private IDockWindow? _window;
    private IList<IDockWindow>? _windows;
    private IToolDock? _pinnedDock;
    private PinnedDockDisplayMode _pinnedDockDisplayMode = PinnedDockDisplayMode.Overlay;
    private DockFloatingWindowHostMode _floatingWindowHostMode = DockFloatingWindowHostMode.Default;
    private DockCapabilityPolicy? _rootDockCapabilityPolicy;
    private bool _enableAdaptiveGlobalDockTargets;

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
    public IToolDock? PinnedDock
    {
        get => _pinnedDock;
        set => SetProperty(ref _pinnedDock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public PinnedDockDisplayMode PinnedDockDisplayMode
    {
        get => _pinnedDockDisplayMode;
        set => SetProperty(ref _pinnedDockDisplayMode, value);
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
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockFloatingWindowHostMode FloatingWindowHostMode
    {
        get => _floatingWindowHostMode;
        set => SetProperty(ref _floatingWindowHostMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public DockCapabilityPolicy? RootDockCapabilityPolicy
    {
        get => _rootDockCapabilityPolicy;
        set => SetProperty(ref _rootDockCapabilityPolicy, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ShowWindows { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ExitWindows { get; }
    
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableAdaptiveGlobalDockTargets
    {
        get => _enableAdaptiveGlobalDockTargets;
        set => SetProperty(ref _enableAdaptiveGlobalDockTargets, value);
    }
}
