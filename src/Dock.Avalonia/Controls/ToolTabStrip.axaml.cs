// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Dock.Avalonia.Internal;
using Avalonia.Reactive;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStrip custom control.
/// </summary>
[PseudoClasses(":create")]
public class ToolTabStrip : TabStrip
{
    private readonly Dictionary<ToolTabStripItem, IDisposable[]> _containerObservables = new();
    private Border? _borderLeftFill;
    private Border? _borderRightFill;
    private ItemsPresenter? _itemsPresenter;
    private ScrollViewer? _scrollViewer;

    /// <summary>
    /// Defines the <see cref="CanCreateItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanCreateItemProperty =
        AvaloniaProperty.Register<ToolTabStrip, bool>(nameof(CanCreateItem));

    /// <summary>
    /// Defines the <see cref="MouseWheelScrollOrientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> MouseWheelScrollOrientationProperty =
        AvaloniaProperty.Register<ToolTabStrip, Orientation>(
            nameof(MouseWheelScrollOrientation),
            defaultValue: Orientation.Horizontal);

    /// <summary>
    /// Gets or sets if tab strop dock can create new items.
    /// </summary>
    public bool CanCreateItem
    {
        get => GetValue(CanCreateItemProperty);
        set => SetValue(CanCreateItemProperty, value);
    }

    /// <summary>
    /// Gets or sets orientation used for mouse wheel scrolling in the tab strip.
    /// </summary>
    public Orientation MouseWheelScrollOrientation
    {
        get => GetValue(MouseWheelScrollOrientationProperty);
        set => SetValue(MouseWheelScrollOrientationProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToolTabStrip);

    /// <summary>
    /// Initializes new instance of the <see cref="ToolTabStrip"/> class.
    /// </summary>
    public ToolTabStrip()
    {
        UpdatePseudoClasses(CanCreateItem);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _borderLeftFill = e.NameScope.Find<Border>("PART_BorderLeftFill");
        _borderRightFill = e.NameScope.Find<Border>("PART_BorderRightFill");
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");

        _itemsPresenter?.GetObservable(Border.BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(_ => UpdateBorders()));

        _scrollViewer?.GetObservable(ScrollViewer.OffsetProperty)
            .Subscribe(new AnonymousObserver<Vector>(_ => UpdateBorders()));

        this.GetObservable(SelectedItemProperty)
            .Subscribe(new AnonymousObserver<object?>(_ => UpdateBorders()));

        this.GetObservable(BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(_ => UpdateBorders()));

        UpdateBorders();
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (!e.Handled
            && TabStripMouseWheelScrollHelper.TryHandle(_scrollViewer, MouseWheelScrollOrientation, e.Delta))
        {
            e.Handled = true;
            return;
        }

        base.OnPointerWheelChanged(e);
    }

    private void OnContainerAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is ToolTabStripItem tabStripItem)
        {
            if (_containerObservables.TryGetValue(tabStripItem, out var existingDisposables))
            {
                foreach (var disposable in existingDisposables)
                {
                    disposable.Dispose();
                }
                _containerObservables.Remove(tabStripItem);
            }

            var renderTransformDisposable = tabStripItem.GetObservable(Control.RenderTransformProperty)
                .Subscribe(new AnonymousObserver<ITransform?>(_ => UpdateBorders()));
            
            var boundsDisposable = tabStripItem.GetObservable(Control.BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ => UpdateBorders()));

            _containerObservables[tabStripItem] = [renderTransformDisposable, boundsDisposable];
        }
    }

    private void OnContainerDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is ToolTabStripItem tabStripItem)
        {
            if (_containerObservables.TryGetValue(tabStripItem, out var disposables))
            {
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
                _containerObservables.Remove(tabStripItem);
            }

            tabStripItem.AttachedToVisualTree -= OnContainerAttachedToVisualTree;
            tabStripItem.DetachedFromVisualTree -= OnContainerDetachedFromVisualTree;
        }
    }

    private void UpdateBorders()
    {
        var selectedIndex = SelectedIndex;
        var bounds = _itemsPresenter?.Bounds ?? new Rect();
        var scrollOffset = _scrollViewer?.Offset.X ?? 0.0;
        
        if (selectedIndex >= 0 && selectedIndex < Items.Count && Items.Count != 1)
        {
            var selectedTabStripItem = ContainerFromIndex(selectedIndex) as ToolTabStripItem;
            var width = selectedTabStripItem?.Bounds.Width ?? 0.0;
            var translateX = selectedTabStripItem?.RenderTransform?.Value.M31 ?? 0.0;
            var x = (selectedTabStripItem?.Bounds.X ?? 0.0) + translateX - scrollOffset;
            var leftMargin = new Thickness(0, 0, Bounds.Width - x, 0);
            var rightMargin = new Thickness(x + width, 0, 0, 0);
            _borderLeftFill?.SetCurrentValue(Border.MarginProperty, leftMargin);
            _borderRightFill?.SetCurrentValue(Border.MarginProperty, rightMargin);
        }
        else
        {
            var leftMargin = new Thickness(0, 0, bounds.Width, 0);
            var rightMargin = new Thickness(bounds.Width, 0, 0, 0);
            _borderLeftFill?.SetCurrentValue(Border.MarginProperty, leftMargin);
            _borderRightFill?.SetCurrentValue(Border.MarginProperty, rightMargin);
        }
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is ToolTabStripItem tabStripItem)
        {
            tabStripItem.AttachedToVisualTree += OnContainerAttachedToVisualTree;
            tabStripItem.DetachedFromVisualTree += OnContainerDetachedFromVisualTree;
        }
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ToolTabStripItem();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ToolTabStripItem>(item, out recycleKey);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCreateItemProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>());
        }
    }

    private void UpdatePseudoClasses(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
    }
}
