// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Mdi;

/// <summary>
/// Provides the default classic MDI layout behavior.
/// </summary>
public sealed class ClassicMdiLayoutManager : IMdiLayoutManager
{
    /// <summary>
    /// Gets the shared classic layout manager instance.
    /// </summary>
    public static readonly ClassicMdiLayoutManager Instance = new();

    /// <inheritdoc/>
    public void Arrange(IReadOnlyList<MdiLayoutEntry> entries, Size finalSize)
    {
        if (entries.Count == 0)
        {
            return;
        }

        var minimizedDocuments = entries
            .Where(entry => entry.Document.MdiState == MdiWindowState.Minimized)
            .OrderBy(entry => entry.Document.MdiZIndex)
            .ToList();
        var normalDocuments = entries
            .Where(entry => entry.Document.MdiState != MdiWindowState.Minimized)
            .ToList();

        var reservedHeight = minimizedDocuments.Count > 0
            ? MdiLayoutDefaults.MinimizedHeight + MdiLayoutDefaults.MinimizedSpacing
            : 0;
        var availableHeight = Math.Max(0, finalSize.Height - reservedHeight);
        var availableSize = new Size(finalSize.Width, availableHeight);

        ArrangeMinimizedWindows(minimizedDocuments, finalSize, availableHeight);

        var index = 0;
        foreach (var entry in normalDocuments)
        {
            var bounds = ToAvaloniaRect(entry.Document.MdiBounds);
            if (!IsValidBounds(bounds))
            {
                bounds = CreateDefaultBounds(availableSize, index);
                entry.Document.MdiBounds = ToDockRect(bounds);
            }

            bounds = ClampBounds(bounds, availableSize);

            if (entry.Document.MdiState == MdiWindowState.Maximized)
            {
                bounds = new Rect(0, 0, availableSize.Width, availableSize.Height);
            }

            entry.Control.Arrange(bounds);
            index++;
        }
    }

    /// <inheritdoc/>
    public Rect GetDragBounds(IMdiDocument document, Rect startBounds, Vector delta, Size finalSize, IReadOnlyList<MdiLayoutEntry> entries)
    {
        var bounds = new Rect(
            startBounds.X + delta.X,
            startBounds.Y + delta.Y,
            startBounds.Width,
            startBounds.Height);

        bounds = ClampBounds(bounds, GetAvailableSize(entries, finalSize));
        return bounds;
    }

    /// <inheritdoc/>
    public Rect GetResizeBounds(IMdiDocument document, Rect startBounds, Vector delta, MdiResizeDirection direction, Size finalSize, IReadOnlyList<MdiLayoutEntry> entries)
    {
        var x = startBounds.X;
        var y = startBounds.Y;
        var width = startBounds.Width;
        var height = startBounds.Height;

        if (direction.HasFlag(MdiResizeDirection.Left))
        {
            x += delta.X;
            width -= delta.X;
        }

        if (direction.HasFlag(MdiResizeDirection.Right))
        {
            width += delta.X;
        }

        if (direction.HasFlag(MdiResizeDirection.Top))
        {
            y += delta.Y;
            height -= delta.Y;
        }

        if (direction.HasFlag(MdiResizeDirection.Bottom))
        {
            height += delta.Y;
        }

        var bounds = new Rect(x, y, width, height);
        bounds = ApplySizeConstraints(bounds, document, direction);
        bounds = ClampBounds(bounds, GetAvailableSize(entries, finalSize));
        return bounds;
    }

    /// <inheritdoc/>
    public void UpdateZOrder(IReadOnlyList<IMdiDocument> documents, IMdiDocument? activeDocument)
    {
        if (documents.Count == 0)
        {
            return;
        }

        var ordered = documents.ToList();
        if (activeDocument is not null && ordered.Remove(activeDocument))
        {
            ordered.Add(activeDocument);
        }

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].MdiZIndex = i;
        }
    }

    private static Rect ToAvaloniaRect(DockRect bounds)
    {
        return new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private static DockRect ToDockRect(Rect bounds)
    {
        return new DockRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private static void ArrangeMinimizedWindows(IReadOnlyList<MdiLayoutEntry> minimizedDocuments, Size finalSize, double availableHeight)
    {
        if (minimizedDocuments.Count == 0)
        {
            return;
        }

        var spacing = MdiLayoutDefaults.MinimizedSpacing;
        var totalSpacing = spacing * Math.Max(0, minimizedDocuments.Count - 1);
        var width = minimizedDocuments.Count > 0
            ? Math.Max(0, (finalSize.Width - totalSpacing) / minimizedDocuments.Count)
            : 0;
        var targetWidth = Math.Min(width, MdiLayoutDefaults.MinimizedWidth);
        if (double.IsNaN(targetWidth) || targetWidth <= 0)
        {
            return;
        }

        var x = 0.0;
        var y = Math.Max(0, availableHeight);
        foreach (var entry in minimizedDocuments)
        {
            entry.Control.Arrange(new Rect(x, y, targetWidth, MdiLayoutDefaults.MinimizedHeight));
            x += targetWidth + spacing;
        }
    }

    private static Size GetAvailableSize(IReadOnlyList<MdiLayoutEntry> entries, Size finalSize)
    {
        if (entries.Count == 0)
        {
            return finalSize;
        }

        var minimizedCount = 0;
        foreach (var entry in entries)
        {
            if (entry.Document.MdiState == MdiWindowState.Minimized)
            {
                minimizedCount++;
            }
        }

        var reservedHeight = minimizedCount > 0
            ? MdiLayoutDefaults.MinimizedHeight + MdiLayoutDefaults.MinimizedSpacing
            : 0;

        return new Size(finalSize.Width, Math.Max(0, finalSize.Height - reservedHeight));
    }

    private static Rect CreateDefaultBounds(Size availableSize, int index)
    {
        var width = Math.Max(MdiLayoutDefaults.MinimumWidth, availableSize.Width * MdiLayoutDefaults.DefaultWidthRatio);
        var height = Math.Max(MdiLayoutDefaults.MinimumHeight, availableSize.Height * MdiLayoutDefaults.DefaultHeightRatio);
        width = Math.Min(width, availableSize.Width);
        height = Math.Min(height, availableSize.Height);

        var offset = MdiLayoutDefaults.CascadeOffset * index;
        var x = Math.Min(offset, Math.Max(0, availableSize.Width - width));
        var y = Math.Min(offset, Math.Max(0, availableSize.Height - height));
        return new Rect(x, y, width, height);
    }

    private static Rect ClampBounds(Rect bounds, Size availableSize)
    {
        var width = Math.Min(bounds.Width, availableSize.Width);
        var height = Math.Min(bounds.Height, availableSize.Height);
        if (width < 0 || height < 0)
        {
            return new Rect(0, 0, 0, 0);
        }

        var x = bounds.X;
        var y = bounds.Y;
        if (x < 0)
        {
            x = 0;
        }

        if (y < 0)
        {
            y = 0;
        }

        if (x + width > availableSize.Width)
        {
            x = Math.Max(0, availableSize.Width - width);
        }

        if (y + height > availableSize.Height)
        {
            y = Math.Max(0, availableSize.Height - height);
        }

        return new Rect(x, y, width, height);
    }

    private static Rect ApplySizeConstraints(Rect bounds, IDockable dockable, MdiResizeDirection direction)
    {
        var minWidth = double.IsNaN(dockable.MinWidth) ? MdiLayoutDefaults.MinimumWidth : dockable.MinWidth;
        var minHeight = double.IsNaN(dockable.MinHeight) ? MdiLayoutDefaults.MinimumHeight : dockable.MinHeight;
        var maxWidth = double.IsNaN(dockable.MaxWidth) ? double.PositiveInfinity : dockable.MaxWidth;
        var maxHeight = double.IsNaN(dockable.MaxHeight) ? double.PositiveInfinity : dockable.MaxHeight;

        var width = bounds.Width;
        var height = bounds.Height;
        var x = bounds.X;
        var y = bounds.Y;

        if (width < minWidth)
        {
            if (direction.HasFlag(MdiResizeDirection.Left))
            {
                x = bounds.Right - minWidth;
            }

            width = minWidth;
        }
        else if (width > maxWidth)
        {
            if (direction.HasFlag(MdiResizeDirection.Left))
            {
                x = bounds.Right - maxWidth;
            }

            width = maxWidth;
        }

        if (height < minHeight)
        {
            if (direction.HasFlag(MdiResizeDirection.Top))
            {
                y = bounds.Bottom - minHeight;
            }

            height = minHeight;
        }
        else if (height > maxHeight)
        {
            if (direction.HasFlag(MdiResizeDirection.Top))
            {
                y = bounds.Bottom - maxHeight;
            }

            height = maxHeight;
        }

        return new Rect(x, y, width, height);
    }

    private static bool IsValidBounds(Rect bounds)
    {
        if (double.IsNaN(bounds.Width) || double.IsNaN(bounds.Height))
        {
            return false;
        }

        return bounds.Width > 0 && bounds.Height > 0;
    }
}
