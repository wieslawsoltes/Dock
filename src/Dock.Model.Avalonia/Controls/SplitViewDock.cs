// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// SplitView dock implementation for Avalonia.
/// </summary>
public class SplitViewDock : DockBase, ISplitViewDock
{
    /// <summary>
    /// Defines the <see cref="CompactPaneLength"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, double> CompactPaneLengthProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, double>(
            nameof(CompactPaneLength),
            o => o.CompactPaneLength,
            (o, v) => o.CompactPaneLength = v,
            48.0);

    /// <summary>
    /// Defines the <see cref="DisplayMode"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, SplitViewDisplayMode> DisplayModeProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, SplitViewDisplayMode>(
            nameof(DisplayMode),
            o => o.DisplayMode,
            (o, v) => o.DisplayMode = v,
            SplitViewDisplayMode.Overlay);

    /// <summary>
    /// Defines the <see cref="IsPaneOpen"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, bool> IsPaneOpenProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, bool>(
            nameof(IsPaneOpen),
            o => o.IsPaneOpen,
            (o, v) => o.IsPaneOpen = v);

    /// <summary>
    /// Defines the <see cref="OpenPaneLength"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, double> OpenPaneLengthProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, double>(
            nameof(OpenPaneLength),
            o => o.OpenPaneLength,
            (o, v) => o.OpenPaneLength = v,
            320.0);

    /// <summary>
    /// Defines the <see cref="PanePlacement"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, SplitViewPanePlacement> PanePlacementProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, SplitViewPanePlacement>(
            nameof(PanePlacement),
            o => o.PanePlacement,
            (o, v) => o.PanePlacement = v,
            SplitViewPanePlacement.Left);

    /// <summary>
    /// Defines the <see cref="UseLightDismissOverlayMode"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, bool> UseLightDismissOverlayModeProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, bool>(
            nameof(UseLightDismissOverlayMode),
            o => o.UseLightDismissOverlayMode,
            (o, v) => o.UseLightDismissOverlayMode = v);

    /// <summary>
    /// Defines the <see cref="PaneDockable"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, IDockable?> PaneDockableProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, IDockable?>(
            nameof(PaneDockable),
            o => o.PaneDockable,
            (o, v) => o.PaneDockable = v);

    /// <summary>
    /// Defines the <see cref="ContentDockable"/> property.
    /// </summary>
    public static readonly DirectProperty<SplitViewDock, IDockable?> ContentDockableProperty =
        AvaloniaProperty.RegisterDirect<SplitViewDock, IDockable?>(
            nameof(ContentDockable),
            o => o.ContentDockable,
            (o, v) => o.ContentDockable = v);

    private double _compactPaneLength = 48.0;
    private SplitViewDisplayMode _displayMode = SplitViewDisplayMode.Overlay;
    private bool _isPaneOpen;
    private double _openPaneLength = 320.0;
    private SplitViewPanePlacement _panePlacement = SplitViewPanePlacement.Left;
    private bool _useLightDismissOverlayMode;
    private IDockable? _paneDockable;
    private IDockable? _contentDockable;

    /// <summary>
    /// Initializes new instance of the <see cref="SplitViewDock"/> class.
    /// </summary>
    public SplitViewDock()
    {
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CompactPaneLength")]
    public double CompactPaneLength
    {
        get => _compactPaneLength;
        set => SetAndRaise(CompactPaneLengthProperty, ref _compactPaneLength, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("DisplayMode")]
    public SplitViewDisplayMode DisplayMode
    {
        get => _displayMode;
        set => SetAndRaise(DisplayModeProperty, ref _displayMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsPaneOpen")]
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => SetAndRaise(IsPaneOpenProperty, ref _isPaneOpen, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("OpenPaneLength")]
    public double OpenPaneLength
    {
        get => _openPaneLength;
        set => SetAndRaise(OpenPaneLengthProperty, ref _openPaneLength, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("PanePlacement")]
    public SplitViewPanePlacement PanePlacement
    {
        get => _panePlacement;
        set => SetAndRaise(PanePlacementProperty, ref _panePlacement, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("UseLightDismissOverlayMode")]
    public bool UseLightDismissOverlayMode
    {
        get => _useLightDismissOverlayMode;
        set => SetAndRaise(UseLightDismissOverlayModeProperty, ref _useLightDismissOverlayMode, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("PaneDockable")]
    public IDockable? PaneDockable
    {
        get => _paneDockable;
        set => SetAndRaise(PaneDockableProperty, ref _paneDockable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ContentDockable")]
    public IDockable? ContentDockable
    {
        get => _contentDockable;
        set => SetAndRaise(ContentDockableProperty, ref _contentDockable, value);
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
