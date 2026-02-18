// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Arranges visible children in a single horizontal row with a uniform width.
/// Shrinks immediately when constrained and expands after a delay.
/// </summary>
public class DelayedUniformTabPanel : Panel
{
    private const double WidthEpsilon = 0.1;
    private readonly DispatcherTimer _expansionTimer;
    private double _currentTabWidth = double.NaN;
    private double _pendingExpansionWidth = double.NaN;
    private int _lastVisibleCount = -1;

    /// <summary>
    /// Defines the <see cref="MaxTabWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxTabWidthProperty =
        AvaloniaProperty.Register<DelayedUniformTabPanel, double>(nameof(MaxTabWidth), 220d);

    /// <summary>
    /// Defines the <see cref="MinTabWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinTabWidthProperty =
        AvaloniaProperty.Register<DelayedUniformTabPanel, double>(nameof(MinTabWidth), 80d);

    /// <summary>
    /// Defines the <see cref="ItemSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<DelayedUniformTabPanel, double>(nameof(ItemSpacing), 0d);

    /// <summary>
    /// Defines the <see cref="ExpansionDelay"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> ExpansionDelayProperty =
        AvaloniaProperty.Register<DelayedUniformTabPanel, TimeSpan>(nameof(ExpansionDelay), TimeSpan.FromSeconds(1));

    /// <summary>
    /// Defines the <see cref="ResizeAnimationDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> ResizeAnimationDurationProperty =
        AvaloniaProperty.Register<DelayedUniformTabPanel, TimeSpan>(nameof(ResizeAnimationDuration), TimeSpan.Zero);

    /// <summary>
    /// Defines the animated tab width property.
    /// </summary>
    public static readonly StyledProperty<double> AnimatedTabWidthProperty =
        AvaloniaProperty.Register<DelayedUniformTabPanel, double>(nameof(AnimatedTabWidth), double.NaN);

    static DelayedUniformTabPanel()
    {
        AffectsMeasure<DelayedUniformTabPanel>(MaxTabWidthProperty, MinTabWidthProperty, ItemSpacingProperty, AnimatedTabWidthProperty);
        AffectsArrange<DelayedUniformTabPanel>(MaxTabWidthProperty, MinTabWidthProperty, ItemSpacingProperty, AnimatedTabWidthProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedUniformTabPanel"/> class.
    /// </summary>
    public DelayedUniformTabPanel()
    {
        _expansionTimer = new DispatcherTimer { IsEnabled = false };
        _expansionTimer.Tick += OnExpansionTimerTick;
        UpdateResizeTransition();
    }

    /// <summary>
    /// Gets or sets the maximum width used for each tab when enough space is available.
    /// </summary>
    public double MaxTabWidth
    {
        get => GetValue(MaxTabWidthProperty);
        set => SetValue(MaxTabWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum width used for each tab before overflow occurs.
    /// </summary>
    public double MinTabWidth
    {
        get => GetValue(MinTabWidthProperty);
        set => SetValue(MinTabWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets spacing between tabs.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the delay applied before expanding tab widths.
    /// </summary>
    public TimeSpan ExpansionDelay
    {
        get => GetValue(ExpansionDelayProperty);
        set => SetValue(ExpansionDelayProperty, value);
    }

    /// <summary>
    /// Gets or sets animation duration for tab width changes.
    /// </summary>
    public TimeSpan ResizeAnimationDuration
    {
        get => GetValue(ResizeAnimationDurationProperty);
        set => SetValue(ResizeAnimationDurationProperty, value);
    }

    private double AnimatedTabWidth
    {
        get => GetValue(AnimatedTabWidthProperty);
        set => SetValue(AnimatedTabWidthProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var visibleCount = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                visibleCount++;
            }
        }

        if (visibleCount == 0)
        {
            _currentTabWidth = double.NaN;
            _lastVisibleCount = -1;
            AnimatedTabWidth = double.NaN;
            StopExpansionTimer();
            return new Size(0d, 0d);
        }

        var tabWidth = ResolveTabWidth(visibleCount, availableSize);
        var spacing = Math.Max(0d, ItemSpacing);
        var maxHeight = 0d;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                child.Measure(default);
                continue;
            }

            var measureHeight = IsFinite(availableSize.Height) ? availableSize.Height : double.PositiveInfinity;
            child.Measure(new Size(tabWidth, measureHeight));
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        var desiredWidth = (tabWidth * visibleCount) + (spacing * Math.Max(0, visibleCount - 1));
        return new Size(desiredWidth, maxHeight);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleCount = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                visibleCount++;
            }
        }

        if (visibleCount == 0)
        {
            return finalSize;
        }

        var tabWidth = ResolveTabWidth(visibleCount, finalSize);
        var spacing = Math.Max(0d, ItemSpacing);
        var x = 0d;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(default);
                continue;
            }

            child.Arrange(new Rect(x, 0d, tabWidth, finalSize.Height));
            x += tabWidth + spacing;
        }

        return finalSize;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ExpansionDelayProperty && _expansionTimer.IsEnabled)
        {
            RestartExpansionTimer();
        }

        if (change.Property == ResizeAnimationDurationProperty)
        {
            UpdateResizeTransition();
        }

        if (change.Property == AnimatedTabWidthProperty)
        {
            var animatedWidth = change.GetNewValue<double>();
            if (IsFinite(animatedWidth))
            {
                _currentTabWidth = animatedWidth;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopExpansionTimer();
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static bool NearlyEqual(double left, double right)
    {
        return Math.Abs(left - right) <= WidthEpsilon;
    }

    private double ResolveTabWidth(int visibleCount, Size referenceSize)
    {
        var targetWidth = ComputeTargetTabWidth(visibleCount, referenceSize);
        var countChanged = visibleCount != _lastVisibleCount;
        _lastVisibleCount = visibleCount;

        if (!IsFinite(_currentTabWidth))
        {
            _currentTabWidth = targetWidth;
            _pendingExpansionWidth = double.NaN;
            AnimatedTabWidth = targetWidth;
            return _currentTabWidth;
        }

        if (targetWidth < _currentTabWidth - WidthEpsilon)
        {
            StopExpansionTimer();
            _pendingExpansionWidth = double.NaN;
            AnimateToTabWidth(targetWidth);
            return _currentTabWidth;
        }

        if (targetWidth > _currentTabWidth + WidthEpsilon)
        {
            if (countChanged)
            {
                QueueExpansion(targetWidth);
            }
            else
            {
                if (_expansionTimer.IsEnabled &&
                    IsFinite(_pendingExpansionWidth) &&
                    NearlyEqual(_pendingExpansionWidth, targetWidth))
                {
                    return _currentTabWidth;
                }

                StopExpansionTimer();
                _pendingExpansionWidth = double.NaN;
                AnimateToTabWidth(targetWidth);
            }

            return _currentTabWidth;
        }

        return _currentTabWidth;
    }

    private double ComputeTargetTabWidth(int visibleCount, Size referenceSize)
    {
        var maxTabWidth = Math.Max(0d, MaxTabWidth);
        var minimumTabWidth = Math.Max(0d, MinTabWidth);
        var minimumFromChildren = GetChildMinimumTabWidth();
        var effectiveMinWidth = Math.Max(minimumTabWidth, minimumFromChildren);
        var spacing = Math.Max(0d, ItemSpacing);
        var viewportWidth = ResolveViewportWidth(referenceSize);

        if (visibleCount <= 0)
        {
            return maxTabWidth;
        }

        if (!IsFinite(viewportWidth))
        {
            return Math.Max(effectiveMinWidth, maxTabWidth);
        }

        var availableForTabs = Math.Max(0d, viewportWidth - (spacing * Math.Max(0, visibleCount - 1)));
        var perTab = availableForTabs / visibleCount;
        var target = Math.Min(maxTabWidth, perTab);
        return Math.Max(effectiveMinWidth, target);
    }

    private double ResolveViewportWidth(Size referenceSize)
    {
        if (TryGetDockPanelAvailableWidth(out var dockPanelAvailableWidth))
        {
            return dockPanelAvailableWidth;
        }

        if (this.FindAncestorOfType<ScrollViewer>() is { } scrollViewer &&
            IsFinite(scrollViewer.Bounds.Width) &&
            scrollViewer.Bounds.Width > 0d)
        {
            return scrollViewer.Bounds.Width;
        }

        if (IsFinite(referenceSize.Width) && referenceSize.Width > 0d)
        {
            return referenceSize.Width;
        }

        if (IsFinite(Bounds.Width) && Bounds.Width > 0d)
        {
            return Bounds.Width;
        }

        return double.NaN;
    }

    private bool TryGetDockPanelAvailableWidth(out double width)
    {
        width = double.NaN;

        var partPanel = FindAncestorNamed<Panel>("PART_Panel");
        if (partPanel?.Parent is not DockPanel dockPanel)
        {
            return false;
        }

        if (!IsFinite(dockPanel.Bounds.Width) || dockPanel.Bounds.Width <= 0d)
        {
            return false;
        }

        var fixedSiblingsWidth = 0d;
        foreach (var child in dockPanel.Children)
        {
            if (!child.IsVisible || ReferenceEquals(child, partPanel))
            {
                continue;
            }

            if (child.Name is "PART_TrailingFill")
            {
                continue;
            }

            fixedSiblingsWidth += child.Bounds.Width;
            fixedSiblingsWidth += child.Margin.Left + child.Margin.Right;
        }

        width = Math.Max(0d, dockPanel.Bounds.Width - fixedSiblingsWidth);
        return width > 0d;
    }

    private T? FindAncestorNamed<T>(string name)
        where T : StyledElement
    {
        for (var current = this.GetVisualParent(); current is not null; current = current.GetVisualParent())
        {
            if (current is T typed && string.Equals(typed.Name, name, StringComparison.Ordinal))
            {
                return typed;
            }
        }

        return null;
    }

    private double GetChildMinimumTabWidth()
    {
        var minWidth = 0d;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            minWidth = Math.Max(minWidth, child.MinWidth);
        }

        return minWidth;
    }

    private void QueueExpansion(double targetWidth)
    {
        if (_expansionTimer.IsEnabled && IsFinite(_pendingExpansionWidth) && NearlyEqual(_pendingExpansionWidth, targetWidth))
        {
            return;
        }

        var delay = ExpansionDelay;
        if (delay <= TimeSpan.Zero)
        {
            StopExpansionTimer();
            _pendingExpansionWidth = double.NaN;
            AnimateToTabWidth(targetWidth);
            return;
        }

        _pendingExpansionWidth = targetWidth;
        RestartExpansionTimer();
    }

    private void RestartExpansionTimer()
    {
        _expansionTimer.Stop();
        _expansionTimer.Interval = ExpansionDelay <= TimeSpan.Zero ? TimeSpan.Zero : ExpansionDelay;
        _expansionTimer.Start();
    }

    private void StopExpansionTimer()
    {
        _expansionTimer.Stop();
    }

    private void OnExpansionTimerTick(object? sender, EventArgs e)
    {
        _expansionTimer.Stop();

        if (!IsFinite(_pendingExpansionWidth))
        {
            return;
        }

        AnimateToTabWidth(_pendingExpansionWidth);
        _pendingExpansionWidth = double.NaN;
    }

    private void AnimateToTabWidth(double targetWidth)
    {
        if (!IsFinite(targetWidth))
        {
            return;
        }

        if (!IsFinite(_currentTabWidth))
        {
            _currentTabWidth = targetWidth;
            AnimatedTabWidth = targetWidth;
            return;
        }

        if (NearlyEqual(_currentTabWidth, targetWidth))
        {
            _currentTabWidth = targetWidth;
            AnimatedTabWidth = targetWidth;
            return;
        }

        var duration = ResizeAnimationDuration;
        if (duration <= TimeSpan.Zero)
        {
            _currentTabWidth = targetWidth;
            AnimatedTabWidth = targetWidth;
        }
        else
        {
            AnimatedTabWidth = targetWidth;
        }
    }

    private void UpdateResizeTransition()
    {
        if (ResizeAnimationDuration <= TimeSpan.Zero)
        {
            Transitions = null;
            return;
        }

        Transitions =
        [
            new DoubleTransition
            {
                Property = AnimatedTabWidthProperty,
                Duration = ResizeAnimationDuration,
                Easing = new CubicEaseOut()
            }
        ];
    }
}
