// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// SplitView dock implementation for ReactiveUI.
/// </summary>
public partial class SplitViewDock : DockBase, ISplitViewDock
{
    /// <summary>
    /// Initializes new instance of the <see cref="SplitViewDock"/> class.
    /// </summary>
    public SplitViewDock()
    {
        _compactPaneLength = 48.0;
        _displayMode = SplitViewDisplayMode.Overlay;
        _openPaneLength = 320.0;
        _panePlacement = SplitViewPanePlacement.Left;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double CompactPaneLength { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial SplitViewDisplayMode DisplayMode { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsPaneOpen { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double OpenPaneLength { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial SplitViewPanePlacement PanePlacement { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool UseLightDismissOverlayMode { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IDockable? PaneDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial IDockable? ContentDockable { get; set; }

    /// <inheritdoc/>
    public virtual void OpenPane()
    {
        IsPaneOpen = true;
    }

    /// <inheritdoc/>
    public virtual void ClosePane()
    {
        IsPaneOpen = false;
    }

    /// <inheritdoc/>
    public virtual void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}
