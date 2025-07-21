// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;

namespace Dock.Avalonia.Diagnostics;

internal sealed class DockDebugOverlayManager : IDisposable
{
    private readonly TopLevel _topLevel;
    private readonly Dictionary<DockControl, DebugOverlayHelper> _helpers = new();

    public DockDebugOverlayManager(TopLevel topLevel)
    {
        _topLevel = topLevel;
        AttachExisting(topLevel);
        _topLevel.LayoutUpdated += OnRootLayoutUpdated;
    }

    private void AttachExisting(Visual root)
    {
        foreach (var dock in root.GetVisualDescendants().OfType<DockControl>().ToList())
        {
            AttachOverlay(dock);
        }
    }

    private void OnRootLayoutUpdated(object? sender, EventArgs e)
    {
        var current = _topLevel.GetVisualDescendants().OfType<DockControl>().ToList();

        foreach (var dock in current)
        {
            if (!_helpers.ContainsKey(dock))
            {
                AttachOverlay(dock);
            }
        }

        foreach (var dock in _helpers.Keys.ToList())
        {
            if (!current.Contains(dock))
            {
                RemoveOverlay(dock);
            }
        }
    }


    private void AttachOverlay(DockControl dock)
    {
        if (_helpers.ContainsKey(dock))
        {
            return;
        }

        var helper = new DebugOverlayHelper();
        _helpers[dock] = helper;
        helper.AddOverlay(dock);
        dock.LayoutUpdated += OnDockLayoutUpdated;
    }

    private void RemoveOverlay(DockControl dock)
    {
        if (_helpers.TryGetValue(dock, out var helper))
        {
            dock.LayoutUpdated -= OnDockLayoutUpdated;
            helper.RemoveOverlay(dock);
            _helpers.Remove(dock);
        }
    }

    private void OnDockLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is DockControl dock && _helpers.TryGetValue(dock, out var helper))
        {
            helper.Invalidate();
        }
    }

    public void Dispose()
    {
        _topLevel.LayoutUpdated -= OnRootLayoutUpdated;

        foreach (var pair in _helpers.ToList())
        {
            var dock = pair.Key;
            var helper = pair.Value;
            dock.LayoutUpdated -= OnDockLayoutUpdated;
            helper.RemoveOverlay(dock);
        }

        _helpers.Clear();
    }
}
