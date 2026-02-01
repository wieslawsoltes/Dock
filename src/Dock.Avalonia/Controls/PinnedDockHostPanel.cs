// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Hosts main content and pinned dock previews, arranging them inline or overlay.
/// </summary>
public class PinnedDockHostPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="PinnedDockDisplayMode"/> property.
    /// </summary>
    public static readonly StyledProperty<PinnedDockDisplayMode> PinnedDockDisplayModeProperty =
        AvaloniaProperty.Register<PinnedDockHostPanel, PinnedDockDisplayMode>(nameof(PinnedDockDisplayMode), PinnedDockDisplayMode.Overlay);

    /// <summary>
    /// Defines the <see cref="PinnedDockAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Alignment> PinnedDockAlignmentProperty =
        AvaloniaProperty.Register<PinnedDockHostPanel, Alignment>(nameof(PinnedDockAlignment), Alignment.Unset);

    static PinnedDockHostPanel()
    {
        AffectsMeasure<PinnedDockHostPanel>(PinnedDockDisplayModeProperty, PinnedDockAlignmentProperty);
        AffectsArrange<PinnedDockHostPanel>(PinnedDockDisplayModeProperty, PinnedDockAlignmentProperty);
    }

    /// <summary>
    /// Gets or sets pinned dock display mode.
    /// </summary>
    public PinnedDockDisplayMode PinnedDockDisplayMode
    {
        get => GetValue(PinnedDockDisplayModeProperty);
        set => SetValue(PinnedDockDisplayModeProperty, value);
    }

    /// <summary>
    /// Gets or sets pinned dock alignment.
    /// </summary>
    public Alignment PinnedDockAlignment
    {
        get => GetValue(PinnedDockAlignmentProperty);
        set => SetValue(PinnedDockAlignmentProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
        {
            return new Size();
        }

        var (main, pinned) = ResolveChildren();
        var alignment = GetEffectiveAlignment(pinned);
        if (GetEffectiveDisplayMode() != PinnedDockDisplayMode.Inline
            || pinned is null
            || alignment == Alignment.Unset
            || !HasPinnedDockable(pinned))
        {
            return MeasureOverlay(availableSize);
        }

        pinned.Measure(availableSize);
        var pinnedSize = GetPinnedSize(pinned);

        var remainingSize = GetRemainingSize(availableSize, pinnedSize, alignment);
        main?.Measure(remainingSize);

        var mainSize = main?.DesiredSize ?? new Size();
        return GetDesiredSize(mainSize, pinnedSize, alignment);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0)
        {
            return finalSize;
        }

        var (main, pinned) = ResolveChildren();
        var alignment = GetEffectiveAlignment(pinned);
        if (GetEffectiveDisplayMode() != PinnedDockDisplayMode.Inline
            || pinned is null
            || alignment == Alignment.Unset
            || !HasPinnedDockable(pinned))
        {
            ArrangeOverlay(finalSize);
            return finalSize;
        }

        var pinnedSize = GetPinnedSize(pinned);
        var pinnedWidth = Math.Min(pinnedSize.Width, finalSize.Width);
        var pinnedHeight = Math.Min(pinnedSize.Height, finalSize.Height);

        var remainingWidth = Math.Max(0, finalSize.Width - pinnedWidth);
        var remainingHeight = Math.Max(0, finalSize.Height - pinnedHeight);

        Rect pinnedRect;
        Rect mainRect;

        switch (alignment)
        {
            case Alignment.Left:
                pinnedRect = new Rect(0, 0, pinnedWidth, finalSize.Height);
                mainRect = new Rect(pinnedWidth, 0, remainingWidth, finalSize.Height);
                break;
            case Alignment.Right:
                pinnedRect = new Rect(finalSize.Width - pinnedWidth, 0, pinnedWidth, finalSize.Height);
                mainRect = new Rect(0, 0, remainingWidth, finalSize.Height);
                break;
            case Alignment.Top:
                pinnedRect = new Rect(0, 0, finalSize.Width, pinnedHeight);
                mainRect = new Rect(0, pinnedHeight, finalSize.Width, remainingHeight);
                break;
            case Alignment.Bottom:
                pinnedRect = new Rect(0, finalSize.Height - pinnedHeight, finalSize.Width, pinnedHeight);
                mainRect = new Rect(0, 0, finalSize.Width, remainingHeight);
                break;
            default:
                ArrangeOverlay(finalSize);
                return finalSize;
        }

        pinned.Arrange(pinnedRect);
        main?.Arrange(mainRect);

        return finalSize;
    }

    private (Control? Main, Control? Pinned) ResolveChildren()
    {
        Control? pinned = null;
        Control? main = null;

        foreach (var child in Children)
        {
            if (pinned is null && child is PinnedDockControl)
            {
                pinned = child;
                continue;
            }

            if (main is null)
            {
                main = child;
            }
        }

        if (main is null && pinned is not null)
        {
            foreach (var child in Children)
            {
                if (child != pinned)
                {
                    main = child;
                    break;
                }
            }
        }

        if (main is null)
        {
            main = pinned;
        }

        return (main, pinned);
    }

    private bool HasPinnedDockable(Control pinned)
    {
        if (!pinned.IsVisible)
        {
            return false;
        }

        if (DataContext is not IRootDock rootDock)
        {
            return true;
        }

        var pinnedDock = rootDock.PinnedDock;
        if (pinnedDock is null)
        {
            return false;
        }

        var visibleDockables = pinnedDock.VisibleDockables;
        return visibleDockables is not null && visibleDockables.Count > 0;
    }

    private PinnedDockDisplayMode GetEffectiveDisplayMode()
    {
        if (DataContext is IRootDock rootDock)
        {
            var dockable = GetPinnedDockable(rootDock);
            if (dockable?.PinnedDockDisplayModeOverride is { } overrideMode)
            {
                return overrideMode;
            }
        }

        return PinnedDockDisplayMode;
    }

    private static IDockable? GetPinnedDockable(IRootDock rootDock)
    {
        var pinnedDock = rootDock.PinnedDock;
        if (pinnedDock is null)
        {
            return null;
        }

        if (pinnedDock.ActiveDockable is { } activeDockable
            && activeDockable is not ISplitter
            && pinnedDock.VisibleDockables?.Contains(activeDockable) == true)
        {
            return activeDockable;
        }

        return pinnedDock.VisibleDockables?.FirstOrDefault(dockable => dockable is not ISplitter);
    }

    private Alignment GetEffectiveAlignment(Control? pinned)
    {
        if (pinned is PinnedDockControl pinnedControl && pinnedControl.PinnedDockAlignment != Alignment.Unset)
        {
            return pinnedControl.PinnedDockAlignment;
        }

        if (DataContext is IRootDock rootDock && rootDock.PinnedDock is { } pinnedDock)
        {
            if (pinnedDock.Alignment != Alignment.Unset)
            {
                return pinnedDock.Alignment;
            }
        }

        return PinnedDockAlignment;
    }

    private Size MeasureOverlay(Size availableSize)
    {
        var desired = new Size();
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            desired = new Size(
                Math.Max(desired.Width, child.DesiredSize.Width),
                Math.Max(desired.Height, child.DesiredSize.Height));
        }

        return desired;
    }

    private void ArrangeOverlay(Size finalSize)
    {
        var rect = new Rect(finalSize);
        foreach (var child in Children)
        {
            child.Arrange(rect);
        }
    }

    private static Size GetRemainingSize(Size availableSize, Size pinnedSize, Alignment alignment)
    {
        return alignment switch
        {
            Alignment.Left => new Size(Math.Max(0, availableSize.Width - pinnedSize.Width), availableSize.Height),
            Alignment.Right => new Size(Math.Max(0, availableSize.Width - pinnedSize.Width), availableSize.Height),
            Alignment.Top => new Size(availableSize.Width, Math.Max(0, availableSize.Height - pinnedSize.Height)),
            Alignment.Bottom => new Size(availableSize.Width, Math.Max(0, availableSize.Height - pinnedSize.Height)),
            _ => availableSize
        };
    }

    private static Size GetDesiredSize(Size mainSize, Size pinnedSize, Alignment alignment)
    {
        return alignment switch
        {
            Alignment.Left => new Size(mainSize.Width + pinnedSize.Width, Math.Max(mainSize.Height, pinnedSize.Height)),
            Alignment.Right => new Size(mainSize.Width + pinnedSize.Width, Math.Max(mainSize.Height, pinnedSize.Height)),
            Alignment.Top => new Size(Math.Max(mainSize.Width, pinnedSize.Width), mainSize.Height + pinnedSize.Height),
            Alignment.Bottom => new Size(Math.Max(mainSize.Width, pinnedSize.Width), mainSize.Height + pinnedSize.Height),
            _ => new Size(Math.Max(mainSize.Width, pinnedSize.Width), Math.Max(mainSize.Height, pinnedSize.Height))
        };
    }

    private static Size GetPinnedSize(Control pinned)
    {
        return pinned.IsVisible ? pinned.DesiredSize : new Size();
    }
}
