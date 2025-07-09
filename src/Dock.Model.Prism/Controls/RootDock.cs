// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Prism.Commands;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Prism.Core;

namespace Dock.Model.Prism.Controls;

/// <summary>
/// Root dock.
/// </summary>
[DataContract(IsReference = true)]
public class RootDock : DockBase, IRootDock
{
    private bool _isFocusableRoot = true;
    private IList<IDockable>? _hiddenDockables;
    private IList<IDockable>? _leftTopPinnedDockables;
    private IList<IDockable>? _leftBottomPinnedDockables;
    private IList<IDockable>? _rightTopPinnedDockables;
    private IList<IDockable>? _rightBottomPinnedDockables;
    private IList<IDockable>? _topLeftPinnedDockables;
    private IList<IDockable>? _topRightPinnedDockables;
    private IList<IDockable>? _bottomLeftPinnedDockables;
    private IList<IDockable>? _bottomRightPinnedDockables;
    private Alignment _leftPinnedDockablesAlignment = Alignment.Top;
    private Alignment _rightPinnedDockablesAlignment = Alignment.Top;
    private Alignment _topPinnedDockablesAlignment = Alignment.Left;
    private Alignment _bottomPinnedDockablesAlignment = Alignment.Left;
    private IDockWindow? _window;
    private IList<IDockWindow>? _windows;
    private IToolDock? _pinnedDock;

    /// <summary>
    /// Initializes new instance of the <see cref="RootDock"/> class.
    /// </summary>
    public RootDock()
    {
        ShowWindows = new DelegateCommand(() => _navigateAdapter.ShowWindows());
        ExitWindows = new DelegateCommand(() => _navigateAdapter.ExitWindows());
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
    public IList<IDockable>? LeftTopPinnedDockables
    {
        get => _leftTopPinnedDockables;
        set => SetProperty(ref _leftTopPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? LeftBottomPinnedDockables
    {
        get => _leftBottomPinnedDockables;
        set => SetProperty(ref _leftBottomPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? RightTopPinnedDockables
    {
        get => _rightTopPinnedDockables;
        set => SetProperty(ref _rightTopPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? RightBottomPinnedDockables
    {
        get => _rightBottomPinnedDockables;
        set => SetProperty(ref _rightBottomPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? TopLeftPinnedDockables
    {
        get => _topLeftPinnedDockables;
        set => SetProperty(ref _topLeftPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? TopRightPinnedDockables
    {
        get => _topRightPinnedDockables;
        set => SetProperty(ref _topRightPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? BottomLeftPinnedDockables
    {
        get => _bottomLeftPinnedDockables;
        set => SetProperty(ref _bottomLeftPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? BottomRightPinnedDockables
    {
        get => _bottomRightPinnedDockables;
        set => SetProperty(ref _bottomRightPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Alignment LeftPinnedDockablesAlignment
    {
        get => _leftPinnedDockablesAlignment;
        set => SetProperty(ref _leftPinnedDockablesAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Alignment RightPinnedDockablesAlignment
    {
        get => _rightPinnedDockablesAlignment;
        set => SetProperty(ref _rightPinnedDockablesAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Alignment TopPinnedDockablesAlignment
    {
        get => _topPinnedDockablesAlignment;
        set => SetProperty(ref _topPinnedDockablesAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Alignment BottomPinnedDockablesAlignment
    {
        get => _bottomPinnedDockablesAlignment;
        set => SetProperty(ref _bottomPinnedDockablesAlignment, value);
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
