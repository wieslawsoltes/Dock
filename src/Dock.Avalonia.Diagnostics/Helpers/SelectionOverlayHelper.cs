// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Diagnostics.Helpers;

internal class SelectionOverlayHelper
{
    private SelectionOverlayAdorner? _adorner;
    private Control? _root;
    private Control? _target;

    public void AttachOverlay(Control root)
    {
        if (_root == root)
        {
            return;
        }

        RemoveOverlay();

        var layer = AdornerLayer.GetAdornerLayer(root);
        if (layer is null)
        {
            return;
        }

        _root = root;
        _adorner = new SelectionOverlayAdorner
        {
            [AdornerLayer.AdornedElementProperty] = root
        };

        ((ISetLogicalParent)_adorner).SetParent(root);
        layer.Children.Add(_adorner);

        root.LayoutUpdated += OnLayoutUpdated;
    }

    public void RemoveOverlay()
    {
        if (_root is null || _adorner is null)
        {
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(_root);
        if (layer is not null)
        {
            layer.Children.Remove(_adorner);
        }

        ((ISetLogicalParent)_adorner).SetParent(null);
        _root.LayoutUpdated -= OnLayoutUpdated;
        _root = null;
        _adorner = null;
        _target = null;
    }

    public void Highlight(Control? target)
    {
        _target = target;
        Update();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        Update();
    }

    private void Update()
    {
        if (_root is null || _adorner is null)
        {
            return;
        }

        if (_target is { })
        {
            var pos = _target.TranslatePoint(new Point(), _root);
            if (pos.HasValue)
            {
                _adorner.HighlightRect = new Rect(pos.Value, _target.Bounds.Size);
            }
            else
            {
                _adorner.HighlightRect = null;
            }
        }
        else
        {
            _adorner.HighlightRect = null;
        }

        _adorner.InvalidateVisual();
    }

    public static bool TryGetControl(
        IFactory factory,
        IDockable dockable,
        out Control? root,
        out Control? control)
    {
        if (factory.VisibleDockableControls.TryGetValue(dockable, out var c) &&
            factory.VisibleRootControls.TryGetValue(dockable, out var r) && r is Control rc)
        {
            control = c as Control;
            root = rc;
            return true;
        }

        if (factory.PinnedDockableControls.TryGetValue(dockable, out c) &&
            factory.PinnedRootControls.TryGetValue(dockable, out var pr) && pr is Control prc)
        {
            control = c as Control;
            root = prc;
            return true;
        }

        if (factory.TabDockableControls.TryGetValue(dockable, out c) &&
            factory.TabRootControls.TryGetValue(dockable, out var tr) && tr is Control trc)
        {
            control = c as Control;
            root = trc;
            return true;
        }

        root = null;
        control = null;
        return false;
    }
}

