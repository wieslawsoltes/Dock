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

        _topLevel.AddHandler(Visual.VisualTreeAttachmentEvent, OnAttached,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _topLevel.AddHandler(Visual.VisualTreeDetachmentEvent, OnDetached,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    private void AttachExisting(Visual root)
    {
        foreach (var dock in root.GetVisualDescendants().OfType<DockControl>())
        {
            AttachOverlay(dock);
        }
    }

    private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (e.Source is DockControl dock)
        {
            AttachOverlay(dock);
        }
    }

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (e.Source is DockControl dock)
        {
            RemoveOverlay(dock);
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
        _topLevel.RemoveHandler(Visual.VisualTreeAttachmentEvent, OnAttached);
        _topLevel.RemoveHandler(Visual.VisualTreeDetachmentEvent, OnDetached);

        foreach (var (dock, helper) in _helpers.ToList())
        {
            dock.LayoutUpdated -= OnDockLayoutUpdated;
            helper.RemoveOverlay(dock);
        }

        _helpers.Clear();
    }
}
