// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Root dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class RootDock : DockBase, IRootDock
{
    private bool _enableAdaptiveGlobalDockTargets;
    
    /// <summary>
    /// Initializes new instance of the <see cref="RootDock"/> class.
    /// </summary>
    public RootDock()
    {
        _isFocusableRoot = true;
        ShowWindows = ReactiveCommand.Create(() => _navigateAdapter.ShowWindows());
        ExitWindows = ReactiveCommand.Create(() => _navigateAdapter.ExitWindows());
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsFocusableRoot { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IList<IDockable>? HiddenDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IList<IDockable>? LeftPinnedDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IList<IDockable>? RightPinnedDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IList<IDockable>? TopPinnedDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IList<IDockable>? BottomPinnedDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IToolDock? PinnedDock { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IDockWindow? Window { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IList<IDockWindow>? Windows { get; set; }

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
        set => this.RaiseAndSetIfChanged(ref _enableAdaptiveGlobalDockTargets, value);
    }
}
