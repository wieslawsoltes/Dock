// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Reactive;
using Dock.Avalonia.Automation.Peers;
using Dock.Avalonia.Internal;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStrip custom control.
/// </summary>
[PseudoClasses(":create")]
public class ToolTabStrip : TabStrip
{
    private readonly Dictionary<ToolTabStripItem, IDisposable[]> _containerObservables = new();
    private readonly List<IDisposable> _templateObservables = new();
    private Border? _borderLeftFill;
    private Border? _borderRightFill;
    private ItemsPresenter? _itemsPresenter;
    private ScrollViewer? _scrollViewer;
    private IDisposable? _scrollViewerWheelSubscription;

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
    /// Defines the <see cref="IconTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconTemplateProperty =
        AvaloniaProperty.Register<ToolTabStrip, object?>(nameof(IconTemplate));

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<ToolTabStrip, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Defines the <see cref="ModifiedTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ModifiedTemplateProperty =
        AvaloniaProperty.Register<ToolTabStrip, IDataTemplate?>(nameof(ModifiedTemplate));

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

    /// <summary>
    /// Gets or sets tab icon template.
    /// </summary>
    public object? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab header template.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab modified template.
    /// </summary>
    public IDataTemplate? ModifiedTemplate
    {
        get => GetValue(ModifiedTemplateProperty);
        set => SetValue(ModifiedTemplateProperty, value);
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
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ToolTabStripAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        AttachScrollViewerWheel(null);
        ClearTemplateObservables();

        _borderLeftFill = e.NameScope.Find<Border>("PART_BorderLeftFill");
        _borderRightFill = e.NameScope.Find<Border>("PART_BorderRightFill");
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        AttachScrollViewerWheel(_scrollViewer);

        if (_itemsPresenter is not null)
        {
            _templateObservables.Add(_itemsPresenter.GetObservable(Border.BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ => UpdateBorders())));
        }

        if (_scrollViewer is not null)
        {
            _templateObservables.Add(_scrollViewer.GetObservable(ScrollViewer.OffsetProperty)
                .Subscribe(new AnonymousObserver<Vector>(_ => UpdateBorders())));
        }

        _templateObservables.Add(this.GetObservable(SelectedItemProperty)
            .Subscribe(new AnonymousObserver<object?>(_ => UpdateBorders())));

        _templateObservables.Add(this.GetObservable(BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(_ => UpdateBorders())));

        UpdateBorders();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        AttachScrollViewerWheel(null);
        ClearTemplateObservables();
        ClearContainerObservables();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachScrollViewerWheel(_scrollViewer);
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
            tabStripItem.AttachedToVisualTree -= OnContainerAttachedToVisualTree;
            tabStripItem.DetachedFromVisualTree -= OnContainerDetachedFromVisualTree;
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

        if (change.Property == MouseWheelScrollOrientationProperty)
        {
            AttachScrollViewerWheel(_scrollViewer);
        }
    }

    private void UpdatePseudoClasses(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
    }

    private void AttachScrollViewerWheel(ScrollViewer? scrollViewer)
    {
        _scrollViewerWheelSubscription?.Dispose();
        _scrollViewerWheelSubscription = ScrollViewerMouseWheelHookHelper.Attach(scrollViewer, MouseWheelScrollOrientation);
    }

    private void ClearTemplateObservables()
    {
        foreach (var observable in _templateObservables)
        {
            observable.Dispose();
        }

        _templateObservables.Clear();
    }

    private void ClearContainerObservables()
    {
        if (_containerObservables.Count == 0)
        {
            return;
        }

        var entries = new List<KeyValuePair<ToolTabStripItem, IDisposable[]>>(_containerObservables);
        foreach (var entry in entries)
        {
            foreach (var disposable in entry.Value)
            {
                disposable.Dispose();
            }

            entry.Key.AttachedToVisualTree -= OnContainerAttachedToVisualTree;
            entry.Key.DetachedFromVisualTree -= OnContainerDetachedFromVisualTree;
        }

        _containerObservables.Clear();
    }
}
