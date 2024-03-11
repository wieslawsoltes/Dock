using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls;

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
    private IToolDock? _pinnedDock;
    private IDockWindow? _window;
    private IList<IDockWindow>? _windows;

    /// <summary>
    /// Initializes new instance of the <see cref="RootDock"/> class.
    /// </summary>
    public RootDock()
    {
        ShowWindows = ReactiveCommand.Create(() => _navigateAdapter.ShowWindows());
        ExitWindows = ReactiveCommand.Create(() => _navigateAdapter.ExitWindows());
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsFocusableRoot
    {
        get => _isFocusableRoot;
        set => this.RaiseAndSetIfChanged(ref _isFocusableRoot, value);
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
    public IList<IDockable>? LeftPinnedDockables
    {
        get => _leftPinnedDockables;
        set => this.RaiseAndSetIfChanged(ref _leftPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? RightPinnedDockables
    {
        get => _rightPinnedDockables;
        set => this.RaiseAndSetIfChanged(ref _rightPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? TopPinnedDockables
    {
        get => _topPinnedDockables;
        set => this.RaiseAndSetIfChanged(ref _topPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? BottomPinnedDockables
    {
        get => _bottomPinnedDockables;
        set => this.RaiseAndSetIfChanged(ref _bottomPinnedDockables, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IToolDock? PinnedDock
    {
        get => _pinnedDock;
        set => this.RaiseAndSetIfChanged(ref _pinnedDock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockWindow? Window
    {
        get => _window;
        set => this.RaiseAndSetIfChanged(ref _window, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockWindow>? Windows
    {
        get => _windows;
        set => this.RaiseAndSetIfChanged(ref _windows, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ShowWindows { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ExitWindows { get; }
}
