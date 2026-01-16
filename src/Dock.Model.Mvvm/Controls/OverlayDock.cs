// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Overlay dock.
/// </summary>
[DataContract(IsReference = true)]
public class OverlayDock : DockBase, IOverlayDock
{
    private static readonly IOverlayAdapter s_overlayAdapter = new OverlayAdapter();

    private IOverlayAdapter Overlay => Factory?.OverlayAdapter ?? s_overlayAdapter;

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
    private readonly Dictionary<IOverlaySplitterGroup, INotifyCollectionChanged?> _groupPanelNotifiers = new();
    private INotifyCollectionChanged? _visibleDockablesNotifier;
    private INotifyCollectionChanged? _splitterGroupsNotifier;

    /// <summary>
    /// Initializes new instance of the <see cref="OverlayDock"/> class.
    /// </summary>
    public OverlayDock()
    {
        PropertyChanged += OnOverlayDockPropertyChanged;
        AttachVisibleDockables(VisibleDockables);
        AttachSplitterGroups(SplitterGroups);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDockable? BackgroundDockable
    {
        get => Overlay.GetBackground(this);
        set
        {
            if (Factory is { } factory)
            {
                factory.SetOverlayDockBackground(this, value);
            }
            else
            {
                s_overlayAdapter.SetBackground(this, value, () => new List<IDockable>());
            }
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IList<IOverlayPanel>? OverlayPanels
    {
        get => Overlay.GetOverlayPanels(this);
        set
        {
            if (Factory is { } factory)
            {
                factory.SetOverlayDockOverlayPanels(this, value);
            }
            else
            {
                s_overlayAdapter.SetPanels(this, value, () => new List<IDockable>());
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IOverlaySplitterGroup>? SplitterGroups
    {
        get => _splitterGroups;
        set
        {
            SetProperty(ref _splitterGroups, value);
            if (Factory is { } factory)
            {
                factory.SetOverlayDockSplitterGroups(this, value);
            }
            else
            {
                s_overlayAdapter.SetSplitterGroupsOwner(this, value);
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AllowPanelDragging
    {
        get => _allowPanelDragging;
        set => SetProperty(ref _allowPanelDragging, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AllowPanelResizing
    {
        get => _allowPanelResizing;
        set => SetProperty(ref _allowPanelResizing, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableSnapToEdge
    {
        get => _enableSnapToEdge;
        set => SetProperty(ref _enableSnapToEdge, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double SnapThreshold
    {
        get => _snapThreshold;
        set => SetProperty(ref _snapThreshold, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableSnapToPanel
    {
        get => _enableSnapToPanel;
        set => SetProperty(ref _enableSnapToPanel, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double DefaultPanelOpacity
    {
        get => _defaultPanelOpacity;
        set => SetProperty(ref _defaultPanelOpacity, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableBackdropBlur
    {
        get => _enableBackdropBlur;
        set => SetProperty(ref _enableBackdropBlur, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowAlignmentGrid
    {
        get => _showAlignmentGrid;
        set => SetProperty(ref _showAlignmentGrid, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double AlignmentGridSize
    {
        get => _alignmentGridSize;
        set => SetProperty(ref _alignmentGridSize, value);
    }

    private void OnOverlayDockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VisibleDockables))
        {
            AttachVisibleDockables(VisibleDockables);
            NotifyOverlayPanelsChanged();
        }
        else if (e.PropertyName == nameof(SplitterGroups))
        {
            AttachSplitterGroups(SplitterGroups);
            NotifyOverlayPanelsChanged();
        }
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
        NotifyOverlayPanelsChanged();
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
        NotifyOverlayPanelsChanged();
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
        NotifyOverlayPanelsChanged();
    }

    private void NotifyOverlayPanelsChanged()
    {
        OnPropertyChanged(nameof(OverlayPanels));
        OnPropertyChanged(nameof(BackgroundDockable));
    }
}
