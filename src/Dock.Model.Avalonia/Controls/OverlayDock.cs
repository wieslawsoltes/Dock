// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Reactive;
using Dock.Model.Adapters;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Overlay dock.
/// </summary>
[DataContract(IsReference = true)]
public class OverlayDock : DockBase, IOverlayDock
{
    /// <summary>
    /// Defines the <see cref="BackgroundDockable"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, IDockable?> BackgroundDockableProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, IDockable?>(nameof(BackgroundDockable), o => o.BackgroundDockable, (o, v) => o.BackgroundDockable = v);

    /// <summary>
    /// Defines the <see cref="OverlayPanels"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, IList<IOverlayPanel>?> OverlayPanelsProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, IList<IOverlayPanel>?>(nameof(OverlayPanels), o => o.OverlayPanels, (o, v) => o.OverlayPanels = v);

    /// <summary>
    /// Defines the <see cref="SplitterGroups"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, IList<IOverlaySplitterGroup>?> SplitterGroupsProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, IList<IOverlaySplitterGroup>?>(nameof(SplitterGroups), o => o.SplitterGroups, (o, v) => o.SplitterGroups = v);

    /// <summary>
    /// Defines the <see cref="AllowPanelDragging"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, bool> AllowPanelDraggingProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, bool>(nameof(AllowPanelDragging), o => o.AllowPanelDragging, (o, v) => o.AllowPanelDragging = v, true);

    /// <summary>
    /// Defines the <see cref="AllowPanelResizing"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, bool> AllowPanelResizingProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, bool>(nameof(AllowPanelResizing), o => o.AllowPanelResizing, (o, v) => o.AllowPanelResizing = v, true);

    /// <summary>
    /// Defines the <see cref="EnableSnapToEdge"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, bool> EnableSnapToEdgeProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, bool>(nameof(EnableSnapToEdge), o => o.EnableSnapToEdge, (o, v) => o.EnableSnapToEdge = v, true);

    /// <summary>
    /// Defines the <see cref="SnapThreshold"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, double> SnapThresholdProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, double>(nameof(SnapThreshold), o => o.SnapThreshold, (o, v) => o.SnapThreshold = v);

    /// <summary>
    /// Defines the <see cref="EnableSnapToPanel"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, bool> EnableSnapToPanelProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, bool>(nameof(EnableSnapToPanel), o => o.EnableSnapToPanel, (o, v) => o.EnableSnapToPanel = v);

    /// <summary>
    /// Defines the <see cref="DefaultPanelOpacity"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, double> DefaultPanelOpacityProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, double>(nameof(DefaultPanelOpacity), o => o.DefaultPanelOpacity, (o, v) => o.DefaultPanelOpacity = v);

    /// <summary>
    /// Defines the <see cref="EnableBackdropBlur"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, bool> EnableBackdropBlurProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, bool>(nameof(EnableBackdropBlur), o => o.EnableBackdropBlur, (o, v) => o.EnableBackdropBlur = v);

    /// <summary>
    /// Defines the <see cref="ShowAlignmentGrid"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, bool> ShowAlignmentGridProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, bool>(nameof(ShowAlignmentGrid), o => o.ShowAlignmentGrid, (o, v) => o.ShowAlignmentGrid = v);

    /// <summary>
    /// Defines the <see cref="AlignmentGridSize"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayDock, double> AlignmentGridSizeProperty =
        AvaloniaProperty.RegisterDirect<OverlayDock, double>(nameof(AlignmentGridSize), o => o.AlignmentGridSize, (o, v) => o.AlignmentGridSize = v);

    private static readonly IOverlayAdapter s_overlayAdapter = new OverlayAdapter();

    private IOverlayAdapter Overlay => Factory?.OverlayAdapter ?? s_overlayAdapter;

    private readonly Dictionary<IOverlaySplitterGroup, INotifyCollectionChanged?> _groupPanelNotifiers = new();
    private IDisposable? _visibleDockablesSubscription;
    private INotifyCollectionChanged? _visibleDockablesNotifier;
    private INotifyCollectionChanged? _splitterGroupsNotifier;
    private IDockable? _backgroundDockable;
    private IList<IOverlayPanel>? _overlayPanels;
    private IList<IOverlaySplitterGroup>? _splitterGroups;
    private bool _allowPanelDragging = true;
    private bool _allowPanelResizing = true;
    private bool _enableSnapToEdge = true;
    private double _snapThreshold = 8.0;
    private bool _enableSnapToPanel;
    private double _defaultPanelOpacity = 1.0;
    private bool _enableBackdropBlur;
    private bool _showAlignmentGrid;
    private double _alignmentGridSize = 8.0;

    /// <summary>
    /// Initializes new instance of the <see cref="OverlayDock"/> class.
    /// </summary>
    public OverlayDock()
    {
        _splitterGroups = new AvaloniaList<IOverlaySplitterGroup>();
        _visibleDockablesSubscription = this.GetObservable(VisibleDockablesProperty)
            .Subscribe(new AnonymousObserver<IList<IDockable>?>(OnVisibleDockablesChanged));
        AttachVisibleDockables(VisibleDockables);
        AttachSplitterGroups(_splitterGroups);
        RefreshOverlayCaches();
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IDockable? BackgroundDockable
    {
        get => _backgroundDockable;
        set
        {
            if (Factory is { } factory)
            {
                factory.SetOverlayDockBackground(this, value);
            }
            else
            {
                s_overlayAdapter.SetBackground(this, value, () => new AvaloniaList<IDockable>());
            }
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IList<IOverlayPanel>? OverlayPanels
    {
        get => _overlayPanels;
        set
        {
            if (Factory is { } factory)
            {
                factory.SetOverlayDockOverlayPanels(this, value);
            }
            else
            {
                s_overlayAdapter.SetPanels(this, value, () => new AvaloniaList<IDockable>());
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("SplitterGroups")]
    public IList<IOverlaySplitterGroup>? SplitterGroups
    {
        get => _splitterGroups;
        set
        {
            SetAndRaise(SplitterGroupsProperty, ref _splitterGroups, value);
            if (Factory is { } factory)
            {
                factory.SetOverlayDockSplitterGroups(this, value);
            }
            else
            {
                s_overlayAdapter.SetSplitterGroupsOwner(this, value);
            }

            AttachSplitterGroups(value);
            RefreshOverlayCaches();
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AllowPanelDragging")]
    public bool AllowPanelDragging
    {
        get => _allowPanelDragging;
        set => SetAndRaise(AllowPanelDraggingProperty, ref _allowPanelDragging, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AllowPanelResizing")]
    public bool AllowPanelResizing
    {
        get => _allowPanelResizing;
        set => SetAndRaise(AllowPanelResizingProperty, ref _allowPanelResizing, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("EnableSnapToEdge")]
    public bool EnableSnapToEdge
    {
        get => _enableSnapToEdge;
        set => SetAndRaise(EnableSnapToEdgeProperty, ref _enableSnapToEdge, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("SnapThreshold")]
    public double SnapThreshold
    {
        get => _snapThreshold;
        set => SetAndRaise(SnapThresholdProperty, ref _snapThreshold, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("EnableSnapToPanel")]
    public bool EnableSnapToPanel
    {
        get => _enableSnapToPanel;
        set => SetAndRaise(EnableSnapToPanelProperty, ref _enableSnapToPanel, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("DefaultPanelOpacity")]
    public double DefaultPanelOpacity
    {
        get => _defaultPanelOpacity;
        set => SetAndRaise(DefaultPanelOpacityProperty, ref _defaultPanelOpacity, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("EnableBackdropBlur")]
    public bool EnableBackdropBlur
    {
        get => _enableBackdropBlur;
        set => SetAndRaise(EnableBackdropBlurProperty, ref _enableBackdropBlur, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ShowAlignmentGrid")]
    public bool ShowAlignmentGrid
    {
        get => _showAlignmentGrid;
        set => SetAndRaise(ShowAlignmentGridProperty, ref _showAlignmentGrid, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AlignmentGridSize")]
    public double AlignmentGridSize
    {
        get => _alignmentGridSize;
        set => SetAndRaise(AlignmentGridSizeProperty, ref _alignmentGridSize, value);
    }

    private void OnVisibleDockablesChanged(IList<IDockable>? dockables)
    {
        AttachVisibleDockables(dockables);
        RefreshOverlayCaches();
    }

    private void AttachVisibleDockables(IList<IDockable>? dockables)
    {
        if (_visibleDockablesNotifier != null)
        {
            _visibleDockablesNotifier.CollectionChanged -= OnVisibleDockablesCollectionChanged;
            _visibleDockablesNotifier = null;
        }

        _visibleDockablesNotifier = dockables as INotifyCollectionChanged;
        if (_visibleDockablesNotifier != null)
        {
            _visibleDockablesNotifier.CollectionChanged += OnVisibleDockablesCollectionChanged;
        }
    }

    private void OnVisibleDockablesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshOverlayCaches();
    }

    private void AttachSplitterGroups(IList<IOverlaySplitterGroup>? splitterGroups)
    {
        if (_splitterGroupsNotifier != null)
        {
            _splitterGroupsNotifier.CollectionChanged -= OnSplitterGroupsCollectionChanged;
            _splitterGroupsNotifier = null;
        }

        _splitterGroupsNotifier = splitterGroups as INotifyCollectionChanged;
        if (_splitterGroupsNotifier != null)
        {
            _splitterGroupsNotifier.CollectionChanged += OnSplitterGroupsCollectionChanged;
        }

        RebuildGroupPanelNotifiers(splitterGroups);
    }

    private void OnSplitterGroupsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildGroupPanelNotifiers(SplitterGroups);
        RefreshOverlayCaches();
    }

    private void RebuildGroupPanelNotifiers(IList<IOverlaySplitterGroup>? splitterGroups)
    {
        foreach (var notifier in _groupPanelNotifiers.Values)
        {
            notifier.CollectionChanged -= OnGroupPanelsCollectionChanged;
        }

        _groupPanelNotifiers.Clear();

        if (splitterGroups is null)
        {
            return;
        }

        foreach (var group in splitterGroups)
        {
            if (group?.Panels is INotifyCollectionChanged panelsNotifier)
            {
                panelsNotifier.CollectionChanged += OnGroupPanelsCollectionChanged;
                _groupPanelNotifiers[group] = panelsNotifier;
            }
        }
    }

    private void OnGroupPanelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshOverlayCaches();
    }

    private void RefreshOverlayCaches()
    {
        var background = Overlay.GetBackground(this);
        var panels = Overlay.GetOverlayPanels(this);

        SetAndRaise(BackgroundDockableProperty, ref _backgroundDockable, background);
        SetAndRaise(OverlayPanelsProperty, ref _overlayPanels, panels);
    }
}
