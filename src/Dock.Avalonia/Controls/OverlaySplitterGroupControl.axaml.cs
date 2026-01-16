// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Overlay splitter group control that arranges overlay panels with proportional sizing.
/// </summary>
[TemplatePart("PART_ItemsHost", typeof(ItemsControl))]
[TemplatePart("PART_MoveThumb", typeof(Thumb))]
[TemplatePart("PART_Header", typeof(Control))]
public class OverlaySplitterGroupControl : TemplatedControl
{
    private ItemsControl? _itemsControl;
    private readonly global::Avalonia.Collections.AvaloniaList<object> _items = new();
    private Thumb? _moveThumb;
    private Control? _header;
    private IOverlaySplitterGroup? _group;
    private INotifyCollectionChanged? _panelsNotifier;
    private INotifyCollectionChanged? _splittersNotifier;
    private INotifyPropertyChanged? _groupNotifier;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();

        if (_itemsControl != null)
        {
            _itemsControl.ContainerPrepared -= ItemsControlOnContainerPrepared;
            _itemsControl = null;
        }

        base.OnApplyTemplate(e);

        _itemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsHost");
        _moveThumb = e.NameScope.Find<Thumb>("PART_MoveThumb");
        _header = e.NameScope.Find<Control>("PART_Header");

        if (_moveThumb != null)
        {
            _moveThumb.AddHandler(Thumb.DragDeltaEvent, MoveThumbOnDragDelta, RoutingStrategies.Bubble);
            _moveThumb.AddHandler(Thumb.DragCompletedEvent, MoveThumbOnDragCompleted, RoutingStrategies.Bubble);
        }

        if (_header != null)
        {
            DockProperties.SetIsDragArea(_header, true);
        }

        if (_itemsControl != null)
        {
            _itemsControl.ContainerPrepared += ItemsControlOnContainerPrepared;
            _itemsControl.ItemsSource = _items;
        }

        RefreshGroupBindings();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshGroupBindings();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachTemplateHandlers();
        DetachGroupHandlers();
    }

    private void DetachTemplateHandlers()
    {
        if (_moveThumb != null)
        {
            _moveThumb.RemoveHandler(Thumb.DragDeltaEvent, MoveThumbOnDragDelta);
            _moveThumb.RemoveHandler(Thumb.DragCompletedEvent, MoveThumbOnDragCompleted);
            _moveThumb = null;
        }

        _header = null;
    }

    private void ItemsControlOnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is Control container)
        {
            // Bind proportions for proportional layout.
            container[!ProportionalStackPanel.ProportionProperty] = new Binding(nameof(IDockable.Proportion)) { Mode = BindingMode.TwoWay };
        }
    }

    private void RefreshGroupBindings()
    {
        DetachGroupHandlers();

        _group = DataContext as IOverlaySplitterGroup;
        if (_group is null)
        {
            RebuildItems();
            return;
        }

        _groupNotifier = _group as INotifyPropertyChanged;
        if (_groupNotifier != null)
        {
            _groupNotifier.PropertyChanged += OnGroupPropertyChanged;
        }

        _panelsNotifier = _group.Panels as INotifyCollectionChanged;
        if (_panelsNotifier != null)
        {
            _panelsNotifier.CollectionChanged += OnPanelsChanged;
        }

        _splittersNotifier = _group.Splitters as INotifyCollectionChanged;
        if (_splittersNotifier != null)
        {
            _splittersNotifier.CollectionChanged += OnSplittersChanged;
        }

        RebuildItems();
    }

    private void DetachGroupHandlers()
    {
        if (_groupNotifier != null)
        {
            _groupNotifier.PropertyChanged -= OnGroupPropertyChanged;
            _groupNotifier = null;
        }

        if (_panelsNotifier != null)
        {
            _panelsNotifier.CollectionChanged -= OnPanelsChanged;
            _panelsNotifier = null;
        }

        if (_splittersNotifier != null)
        {
            _splittersNotifier.CollectionChanged -= OnSplittersChanged;
            _splittersNotifier = null;
        }
    }

    private void OnGroupPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IOverlaySplitterGroup.Panels)
            || e.PropertyName == nameof(IOverlaySplitterGroup.Splitters))
        {
            RefreshGroupBindings();
        }
    }

    private void OnPanelsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildItems();
    }

    private void OnSplittersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildItems();
    }

    private void RebuildItems()
    {
        if (_itemsControl is null)
        {
            return;
        }

        _items.Clear();
        var panels = _group?.Panels;
        var splitters = _group?.Splitters;

        if (panels is null || panels.Count == 0)
        {
            if (splitters is not null)
            {
                foreach (var splitter in splitters)
                {
                    if (splitter != null)
                    {
                        _items.Add(splitter);
                    }
                }
            }

            return;
        }

        for (var i = 0; i < panels.Count; i++)
        {
            var panel = panels[i];
            if (panel != null)
            {
                _items.Add(panel);
            }

            if (splitters is null || i >= splitters.Count)
            {
                continue;
            }

            var splitter = splitters[i];
            if (splitter != null)
            {
                _items.Add(splitter);
            }
        }

        if (splitters is not null && splitters.Count > panels.Count)
        {
            for (var i = panels.Count; i < splitters.Count; i++)
            {
                var splitter = splitters[i];
                if (splitter != null)
                {
                    _items.Add(splitter);
                }
            }
        }
    }

    private void MoveThumbOnDragDelta(object? sender, VectorEventArgs e)
    {
        if (DataContext is not IOverlaySplitterGroup group)
        {
            return;
        }

        if (group.IsPositionLocked)
        {
            return;
        }

        if (group.Owner is not IOverlayDock overlayDock || !overlayDock.AllowPanelDragging)
        {
            return;
        }

        group.X += e.Vector.X;
        group.Y += e.Vector.Y;
        group.IsDragging = true;
        BringGroupToFront(overlayDock, group);
    }

    private void MoveThumbOnDragCompleted(object? sender, VectorEventArgs e)
    {
        if (DataContext is IOverlaySplitterGroup group)
        {
            group.IsDragging = false;
        }
    }

    private static void BringGroupToFront(IOverlayDock overlayDock, IOverlaySplitterGroup group)
    {
        if (overlayDock.SplitterGroups is null)
        {
            return;
        }

        var ordered = overlayDock.SplitterGroups.Where(g => g != null).OrderBy(g => g!.ZIndex).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i]!.ZIndex = i;
        }

        var maxZ = ordered.Count == 0 ? 0 : ordered.Max(g => g!.ZIndex);
        group.ZIndex = maxZ + 1;
    }
}
