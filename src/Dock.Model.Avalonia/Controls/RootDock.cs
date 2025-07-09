// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Root dock.
/// </summary>
[DataContract(IsReference = true)]
public class RootDock : DockBase, IRootDock
{
    /// <summary>
    /// Defines the <see cref="IsFocusableRoot"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, bool> IsFocusableRootProperty =
        AvaloniaProperty.RegisterDirect<RootDock, bool>(
            nameof(IsFocusableRoot), 
            o => o.IsFocusableRoot, 
            (o, v) => o.IsFocusableRoot = v, 
            true);

    /// <summary>
    /// Defines the <see cref="HiddenDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> HiddenDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(HiddenDockables), 
            o => o.HiddenDockables, 
            (o, v) => o.HiddenDockables = v);

    public static readonly DirectProperty<RootDock, IToolDock?> PinnedDockProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IToolDock?>(
            nameof(PinnedDock), o => o.PinnedDock,
            (o, v) => o.PinnedDock = v);


    /// <summary>
    /// Defines the <see cref="LeftTopPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> LeftTopPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(LeftTopPinnedDockables), o => o.LeftTopPinnedDockables,
            (o, v) => o.LeftTopPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="LeftBottomPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> LeftBottomPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(LeftBottomPinnedDockables), o => o.LeftBottomPinnedDockables,
            (o, v) => o.LeftBottomPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="RightTopPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> RightTopPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(RightTopPinnedDockables), o => o.RightTopPinnedDockables,
            (o, v) => o.RightTopPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="RightBottomPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> RightBottomPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(RightBottomPinnedDockables), o => o.RightBottomPinnedDockables,
            (o, v) => o.RightBottomPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="TopLeftPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> TopLeftPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(TopLeftPinnedDockables), o => o.TopLeftPinnedDockables,
            (o, v) => o.TopLeftPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="TopRightPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> TopRightPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(TopRightPinnedDockables), o => o.TopRightPinnedDockables,
            (o, v) => o.TopRightPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="BottomLeftPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> BottomLeftPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(BottomLeftPinnedDockables), o => o.BottomLeftPinnedDockables,
            (o, v) => o.BottomLeftPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="BottomRightPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> BottomRightPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(BottomRightPinnedDockables), o => o.BottomRightPinnedDockables,
            (o, v) => o.BottomRightPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="LeftPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, Alignment> LeftPinnedDockablesAlignmentProperty =
        AvaloniaProperty.RegisterDirect<RootDock, Alignment>(
            nameof(LeftPinnedDockablesAlignment), o => o.LeftPinnedDockablesAlignment,
            (o, v) => o.LeftPinnedDockablesAlignment = v, Alignment.Top);

    /// <summary>
    /// Defines the <see cref="RightPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, Alignment> RightPinnedDockablesAlignmentProperty =
        AvaloniaProperty.RegisterDirect<RootDock, Alignment>(
            nameof(RightPinnedDockablesAlignment), o => o.RightPinnedDockablesAlignment,
            (o, v) => o.RightPinnedDockablesAlignment = v, Alignment.Top);

    /// <summary>
    /// Defines the <see cref="TopPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, Alignment> TopPinnedDockablesAlignmentProperty =
        AvaloniaProperty.RegisterDirect<RootDock, Alignment>(
            nameof(TopPinnedDockablesAlignment), o => o.TopPinnedDockablesAlignment,
            (o, v) => o.TopPinnedDockablesAlignment = v, Alignment.Left);

    /// <summary>
    /// Defines the <see cref="BottomPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, Alignment> BottomPinnedDockablesAlignmentProperty =
        AvaloniaProperty.RegisterDirect<RootDock, Alignment>(
            nameof(BottomPinnedDockablesAlignment), o => o.BottomPinnedDockablesAlignment,
            (o, v) => o.BottomPinnedDockablesAlignment = v, Alignment.Left);

    /// <summary>
    /// Defines the <see cref="Window"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IDockWindow?> WindowProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IDockWindow?>(
            nameof(Window), 
            o => o.Window, 
            (o, v) => o.Window = v);

    /// <summary>
    /// Defines the <see cref="Windows"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockWindow>?> WindowsProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockWindow>?>(
            nameof(Windows), 
            o => o.Windows, 
            (o, v) => o.Windows = v);

    private bool _isFocusableRoot;
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
    private IToolDock? _pinnedDock;
    private IDockWindow? _window;
    private IList<IDockWindow>? _windows;

    /// <summary>
    /// Initializes new instance of the <see cref="RootDock"/> class.
    /// </summary>
    public RootDock()
    {
        _isFocusableRoot = true;
        _hiddenDockables = new AvaloniaList<IDockable>();
        _leftTopPinnedDockables = new AvaloniaList<IDockable>();
        _leftBottomPinnedDockables = new AvaloniaList<IDockable>();
        _rightTopPinnedDockables = new AvaloniaList<IDockable>();
        _rightBottomPinnedDockables = new AvaloniaList<IDockable>();
        _topLeftPinnedDockables = new AvaloniaList<IDockable>();
        _topRightPinnedDockables = new AvaloniaList<IDockable>();
        _bottomLeftPinnedDockables = new AvaloniaList<IDockable>();
        _bottomRightPinnedDockables = new AvaloniaList<IDockable>();
        _windows = new AvaloniaList<IDockWindow>();
        ShowWindows = Command.Create(() => _navigateAdapter.ShowWindows());
        ExitWindows = Command.Create(() => _navigateAdapter.ExitWindows());
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsFocusableRoot")]
    public bool IsFocusableRoot
    {
        get => _isFocusableRoot;
        set => SetAndRaise(IsFocusableRootProperty, ref _isFocusableRoot, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("HiddenDockables")]
    public IList<IDockable>? HiddenDockables
    {
        get => _hiddenDockables;
        set => SetAndRaise(HiddenDockablesProperty, ref _hiddenDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("PinnedDock")]
    public IToolDock? PinnedDock
    {
        get => _pinnedDock;
        set => SetAndRaise(PinnedDockProperty, ref _pinnedDock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("LeftTopPinnedDockables")]
    public IList<IDockable>? LeftTopPinnedDockables
    {
        get => _leftTopPinnedDockables;
        set => SetAndRaise(LeftTopPinnedDockablesProperty, ref _leftTopPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("LeftBottomPinnedDockables")]
    public IList<IDockable>? LeftBottomPinnedDockables
    {
        get => _leftBottomPinnedDockables;
        set => SetAndRaise(LeftBottomPinnedDockablesProperty, ref _leftBottomPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("RightTopPinnedDockables")]
    public IList<IDockable>? RightTopPinnedDockables
    {
        get => _rightTopPinnedDockables;
        set => SetAndRaise(RightTopPinnedDockablesProperty, ref _rightTopPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("RightBottomPinnedDockables")]
    public IList<IDockable>? RightBottomPinnedDockables
    {
        get => _rightBottomPinnedDockables;
        set => SetAndRaise(RightBottomPinnedDockablesProperty, ref _rightBottomPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("TopLeftPinnedDockables")]
    public IList<IDockable>? TopLeftPinnedDockables
    {
        get => _topLeftPinnedDockables;
        set => SetAndRaise(TopLeftPinnedDockablesProperty, ref _topLeftPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("TopRightPinnedDockables")]
    public IList<IDockable>? TopRightPinnedDockables
    {
        get => _topRightPinnedDockables;
        set => SetAndRaise(TopRightPinnedDockablesProperty, ref _topRightPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("BottomLeftPinnedDockables")]
    public IList<IDockable>? BottomLeftPinnedDockables
    {
        get => _bottomLeftPinnedDockables;
        set => SetAndRaise(BottomLeftPinnedDockablesProperty, ref _bottomLeftPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("BottomRightPinnedDockables")]
    public IList<IDockable>? BottomRightPinnedDockables
    {
        get => _bottomRightPinnedDockables;
        set => SetAndRaise(BottomRightPinnedDockablesProperty, ref _bottomRightPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("LeftPinnedDockablesAlignment")]
    public Alignment LeftPinnedDockablesAlignment
    {
        get => _leftPinnedDockablesAlignment;
        set => SetAndRaise(LeftPinnedDockablesAlignmentProperty, ref _leftPinnedDockablesAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("RightPinnedDockablesAlignment")]
    public Alignment RightPinnedDockablesAlignment
    {
        get => _rightPinnedDockablesAlignment;
        set => SetAndRaise(RightPinnedDockablesAlignmentProperty, ref _rightPinnedDockablesAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("TopPinnedDockablesAlignment")]
    public Alignment TopPinnedDockablesAlignment
    {
        get => _topPinnedDockablesAlignment;
        set => SetAndRaise(TopPinnedDockablesAlignmentProperty, ref _topPinnedDockablesAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("BottomPinnedDockablesAlignment")]
    public Alignment BottomPinnedDockablesAlignment
    {
        get => _bottomPinnedDockablesAlignment;
        set => SetAndRaise(BottomPinnedDockablesAlignmentProperty, ref _bottomPinnedDockablesAlignment, value);
    }

    /// <inheritdoc/>
    [ResolveByName]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Window")]
    public IDockWindow? Window
    {
        get => _window;
        set => SetAndRaise(WindowProperty, ref _window, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Windows")]
    public IList<IDockWindow>? Windows
    {
        get => _windows;
        set => SetAndRaise(WindowsProperty, ref _windows, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand ShowWindows { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public ICommand ExitWindows { get; }
}
