// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Helper methods for arranging MDI documents.
/// </summary>
public static class MdiLayoutHelper
{
    /// <summary>
    /// Cascades MDI documents within the document dock bounds.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    public static void CascadeDocuments(IDocumentDock dock)
    {
        if (!TryGetLayoutBounds(dock, out var bounds))
        {
            return;
        }

        var documents = GetOrderedDocuments(dock);
        var normalDocuments = documents.Where(d => d.MdiState != MdiWindowState.Minimized).ToList();
        if (normalDocuments.Count == 0)
        {
            return;
        }

        var offset = MdiLayoutDefaults.CascadeOffset;
        var maxOffsetX = GetMaxOffset(bounds.Width, normalDocuments.Count, MdiLayoutDefaults.MinimumWidth);
        var maxOffsetY = GetMaxOffset(bounds.Height, normalDocuments.Count, MdiLayoutDefaults.MinimumHeight);
        var maxOffset = Math.Max(0, Math.Min(maxOffsetX, maxOffsetY));
        if (maxOffset > 0 && offset > maxOffset)
        {
            offset = maxOffset;
        }

        var width = Math.Max(bounds.Width * MdiLayoutDefaults.DefaultWidthRatio, bounds.Width - offset * (normalDocuments.Count - 1));
        var height = Math.Max(bounds.Height * MdiLayoutDefaults.DefaultHeightRatio, bounds.Height - offset * (normalDocuments.Count - 1));
        width = Clamp(width, MdiLayoutDefaults.MinimumWidth, bounds.Width);
        height = Clamp(height, MdiLayoutDefaults.MinimumHeight, bounds.Height);

        for (var i = 0; i < normalDocuments.Count; i++)
        {
            var document = normalDocuments[i];
            document.MdiState = MdiWindowState.Normal;
            document.MdiBounds = new DockRect(bounds.X + offset * i, bounds.Y + offset * i, width, height);
        }

        UpdateZOrder(documents);
    }

    /// <summary>
    /// Tiles MDI documents horizontally.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    public static void TileDocumentsHorizontal(IDocumentDock dock)
    {
        if (!TryGetLayoutBounds(dock, out var bounds))
        {
            return;
        }

        var documents = GetOrderedDocuments(dock);
        var normalDocuments = documents.Where(d => d.MdiState != MdiWindowState.Minimized).ToList();
        if (normalDocuments.Count == 0)
        {
            return;
        }

        var height = bounds.Height / normalDocuments.Count;
        for (var i = 0; i < normalDocuments.Count; i++)
        {
            var document = normalDocuments[i];
            document.MdiState = MdiWindowState.Normal;
            document.MdiBounds = new DockRect(bounds.X, bounds.Y + height * i, bounds.Width, height);
        }

        UpdateZOrder(documents);
    }

    /// <summary>
    /// Tiles MDI documents vertically.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    public static void TileDocumentsVertical(IDocumentDock dock)
    {
        if (!TryGetLayoutBounds(dock, out var bounds))
        {
            return;
        }

        var documents = GetOrderedDocuments(dock);
        var normalDocuments = documents.Where(d => d.MdiState != MdiWindowState.Minimized).ToList();
        if (normalDocuments.Count == 0)
        {
            return;
        }

        var width = bounds.Width / normalDocuments.Count;
        for (var i = 0; i < normalDocuments.Count; i++)
        {
            var document = normalDocuments[i];
            document.MdiState = MdiWindowState.Normal;
            document.MdiBounds = new DockRect(bounds.X + width * i, bounds.Y, width, bounds.Height);
        }

        UpdateZOrder(documents);
    }

    /// <summary>
    /// Restores all MDI documents to the normal state.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    public static void RestoreDocuments(IDocumentDock dock)
    {
        var documents = GetOrderedDocuments(dock);
        foreach (var document in documents)
        {
            document.MdiState = MdiWindowState.Normal;
        }

        UpdateZOrder(documents);
    }

    private static bool TryGetLayoutBounds(IDocumentDock dock, out DockRect bounds)
    {
        dock.GetVisibleBounds(out _, out _, out var width, out var height);
        if (double.IsNaN(width) || double.IsNaN(height) || width <= 0 || height <= 0)
        {
            bounds = default;
            return false;
        }

        var minimizedCount = dock.VisibleDockables?.OfType<IMdiDocument>().Count(d => d.MdiState == MdiWindowState.Minimized) ?? 0;
        var reservedHeight = minimizedCount > 0 ? MdiLayoutDefaults.MinimizedHeight + MdiLayoutDefaults.MinimizedSpacing : 0;
        bounds = new DockRect(0, 0, width, Math.Max(0, height - reservedHeight));
        return true;
    }

    private static List<IMdiDocument> GetOrderedDocuments(IDocumentDock dock)
    {
        var documents = dock.VisibleDockables?.OfType<IMdiDocument>().ToList() ?? new List<IMdiDocument>();
        if (dock.ActiveDockable is IMdiDocument activeDocument)
        {
            documents.Remove(activeDocument);
            documents.Add(activeDocument);
        }

        return documents;
    }

    private static void UpdateZOrder(IReadOnlyList<IMdiDocument> documents)
    {
        for (var i = 0; i < documents.Count; i++)
        {
            documents[i].MdiZIndex = i;
        }
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private static double GetMaxOffset(double total, int count, double min)
    {
        if (count <= 1)
        {
            return 0;
        }

        var available = total - min;
        if (available <= 0)
        {
            return 0;
        }

        return available / (count - 1);
    }
}
