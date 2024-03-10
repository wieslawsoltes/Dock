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

    /// <summary>
    /// Defines the <see cref="LeftPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> LeftPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(LeftPinnedDockables), o => o.LeftPinnedDockables, 
            (o, v) => o.LeftPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="PinnedDock"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IToolDock?> PinnedDockProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IToolDock?>(
            nameof(PinnedDock), o => o.PinnedDock,
            (o, v) => o.PinnedDock = v);

    /// <summary>
    /// Defines the <see cref="RightPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> RightPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(RightPinnedDockables), o => o.RightPinnedDockables, 
            (o, v) => o.RightPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="TopPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> TopPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(TopPinnedDockables), o => o.TopPinnedDockables, 
            (o, v) => o.TopPinnedDockables = v);

    /// <summary>
    /// Defines the <see cref="BottomPinnedDockables"/> property.
    /// </summary>
    public static readonly DirectProperty<RootDock, IList<IDockable>?> BottomPinnedDockablesProperty =
        AvaloniaProperty.RegisterDirect<RootDock, IList<IDockable>?>(
            nameof(BottomPinnedDockables), o => o.BottomPinnedDockables, 
            (o, v) => o.BottomPinnedDockables = v);

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
    private IList<IDockable>? _leftPinnedDockables;
    private IList<IDockable>? _rightPinnedDockables;
    private IList<IDockable>? _topPinnedDockables;
    private IList<IDockable>? _bottomPinnedDockables;
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
        _leftPinnedDockables = new AvaloniaList<IDockable>();
        _rightPinnedDockables = new AvaloniaList<IDockable>();
        _topPinnedDockables = new AvaloniaList<IDockable>();
        _bottomPinnedDockables = new AvaloniaList<IDockable>();
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
    [JsonPropertyName("LeftPinnedDockables")]
    public IList<IDockable>? LeftPinnedDockables
    {
        get => _leftPinnedDockables;
        set => SetAndRaise(LeftPinnedDockablesProperty, ref _leftPinnedDockables, value);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IToolDock? PinnedDock
    {
        get => _pinnedDock;
        set => SetAndRaise(PinnedDockProperty, ref _pinnedDock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("RightPinnedDockables")]
    public IList<IDockable>? RightPinnedDockables
    {
        get => _rightPinnedDockables;
        set => SetAndRaise(RightPinnedDockablesProperty, ref _rightPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("TopPinnedDockables")]
    public IList<IDockable>? TopPinnedDockables
    {
        get => _topPinnedDockables;
        set => SetAndRaise(TopPinnedDockablesProperty, ref _topPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("BottomPinnedDockables")]
    public IList<IDockable>? BottomPinnedDockables
    {
        get => _bottomPinnedDockables;
        set => SetAndRaise(BottomPinnedDockablesProperty, ref _bottomPinnedDockables, value);
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
