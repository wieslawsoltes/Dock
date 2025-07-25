// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Reactive;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStrip custom control.
/// </summary>
[PseudoClasses(":create")]
public class ToolTabStrip : TabStrip
{
    private IDisposable? _dispose1;
    private IDisposable? _dispose2;

    /// <summary>
    /// Defines the <see cref="CanCreateItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanCreateItemProperty =
        AvaloniaProperty.Register<ToolTabStrip, bool>(nameof(CanCreateItem));

    /// <summary>
    /// Gets or sets if tab strop dock can create new items.
    /// </summary>
    public bool CanCreateItem
    {
        get => GetValue(CanCreateItemProperty);
        set => SetValue(CanCreateItemProperty, value);
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

        var borderLeftFill = e.NameScope.Find<Border>("PART_BorderLeftFill");
        var borderRightFill = e.NameScope.Find<Border>("PART_BorderRightFill");
        var itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");

        itemsPresenter?.GetObservable(Border.BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(bounds =>
            {
                UpdateBorders(SelectedIndex, bounds, borderLeftFill, borderRightFill);
            }));

        this.GetObservable(SelectedItemProperty)
            .Subscribe(new AnonymousObserver<object?>(selectedItem =>
            {
                UpdateBorders(SelectedIndex, itemsPresenter?.Bounds ?? new Rect(), borderLeftFill, borderRightFill);

                _dispose1?.Dispose();
                _dispose2?.Dispose();

                if (selectedItem is null)
                {
                    return;
                }

                // TODO: Use generator events instead of SelectedItemProperty to get containers and hook observers.
                var selectedTabStripItem = ContainerFromItem(selectedItem) as ToolTabStripItem;
                _dispose1 = selectedTabStripItem?.GetObservable(Control.RenderTransformProperty)
                    .Subscribe(new AnonymousObserver<ITransform?>(_ =>
                    {
                        UpdateBorders(SelectedIndex, itemsPresenter?.Bounds ?? new Rect(), borderLeftFill, borderRightFill);
                    }));
                _dispose2 = selectedTabStripItem?.GetObservable(Control.BoundsProperty)
                    .Subscribe(new AnonymousObserver<Rect>(_ =>
                    {
                        UpdateBorders(SelectedIndex, itemsPresenter?.Bounds ?? new Rect(), borderLeftFill, borderRightFill);
                    }));
            }));

        this.GetObservable(BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(_ =>
            {
                UpdateBorders(SelectedIndex, itemsPresenter?.Bounds ?? new Rect(), borderLeftFill, borderRightFill);
            }));
    }

    private void UpdateBorders(int selectedIndex, Rect bounds, Border? borderLeftFill, Border? borderRightFill)
    {
        if (selectedIndex >= 0 && selectedIndex < Items.Count && Items.Count != 1)
        {
            var selectedTabStripItem = ContainerFromIndex(selectedIndex) as ToolTabStripItem;

            var width = selectedTabStripItem?.Bounds.Width ?? 0.0;
            var x = selectedTabStripItem?.Bounds.X ?? 0.0;
            
            var translateX = selectedTabStripItem?.RenderTransform?.Value.M31 ?? 0.0;

            x += translateX;

            var leftMargin = new Thickness(0, 0, Bounds.Width - x, 0);
            var rightMargin = new Thickness(x + width, 0, 0, 0);
            borderLeftFill?.SetCurrentValue(Border.MarginProperty, leftMargin);
            borderRightFill?.SetCurrentValue(Border.MarginProperty, rightMargin);

        }
        else
        {
            var leftMargin = new Thickness(0, 0, bounds.Width, 0);
            var rightMargin = new Thickness(bounds.Width, 0, 0, 0);
            borderLeftFill?.SetCurrentValue(Border.MarginProperty, leftMargin);
            borderRightFill?.SetCurrentValue(Border.MarginProperty, rightMargin);
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
