// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// SplitView dock implementation for Caliburn.Micro.
/// </summary>
public class SplitViewDock : DockBase, ISplitViewDock
{
    private double _compactPaneLength = 48.0;
    private SplitViewDisplayMode _displayMode = SplitViewDisplayMode.Overlay;
    private bool _isPaneOpen;
    private double _openPaneLength = 320.0;
    private SplitViewPanePlacement _panePlacement = SplitViewPanePlacement.Left;
    private bool _useLightDismissOverlayMode;
    private IDockable? _paneDockable;
    private IDockable? _contentDockable;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double CompactPaneLength
    {
        get => _compactPaneLength;
        set => Set(ref _compactPaneLength, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public SplitViewDisplayMode DisplayMode
    {
        get => _displayMode;
        set => Set(ref _displayMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => Set(ref _isPaneOpen, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double OpenPaneLength
    {
        get => _openPaneLength;
        set => Set(ref _openPaneLength, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public SplitViewPanePlacement PanePlacement
    {
        get => _panePlacement;
        set => Set(ref _panePlacement, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool UseLightDismissOverlayMode
    {
        get => _useLightDismissOverlayMode;
        set => Set(ref _useLightDismissOverlayMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? PaneDockable
    {
        get => _paneDockable;
        set => Set(ref _paneDockable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? ContentDockable
    {
        get => _contentDockable;
        set => Set(ref _contentDockable, value);
    }

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
