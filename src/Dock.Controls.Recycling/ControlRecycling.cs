// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling.Model;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Recycling;

/// <summary>
/// 
/// </summary>
public class ControlRecycling : AvaloniaObject, IControlRecycling
{
    private readonly Dictionary<object, object> _cache = new();
    private bool _tryToUseIdAsKey;

    /// <summary>
    /// 
    /// </summary>
    public static readonly DirectProperty<ControlRecycling, bool> TryToUseIdAsKeyProperty =
        AvaloniaProperty.RegisterDirect<ControlRecycling, bool>(nameof(TryToUseIdAsKey), o => o.TryToUseIdAsKey, (o, v) => o.TryToUseIdAsKey = v);

    /// <summary>
    /// 
    /// </summary>
    public bool TryToUseIdAsKey
    {
        get => _tryToUseIdAsKey;
        set => SetAndRaise(TryToUseIdAsKeyProperty, ref _tryToUseIdAsKey, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    /// <returns></returns>
    public bool TryGetValue(object? data, out object? control)
    {
        if (data is null)
        {
            control = null;
            return false;
        }

        return _cache.TryGetValue(data, out control);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    public void Add(object data, object control)
    {
        _cache[data] = control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="existing"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public object? Build(object? data, object? existing, object? parent)
    {
        var key = data;
        if (key is null)
        {
            return null;
        }

        if (TryToUseIdAsKey && key is IControlRecyclingIdProvider idProvider)
        {
            if (!string.IsNullOrWhiteSpace(idProvider.GetControlRecyclingId()))
            {
                key = idProvider.GetControlRecyclingId();
            }
        }

        if (TryGetValue(key, out var control))
        {
            // If the cached control is currently in the visual tree, remove it from its parent
            if (control is Visual visual && !ReferenceEquals(existing, control))
            {
                RemoveFromVisualParent(visual);
            }

            return control;
        }

        var dataTemplate = (parent as Control)?.FindDataTemplate(data);

        control = dataTemplate?.Build(data);
        if (control is null)
        {
            return null;
        }

        Add(key!, control);

        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Removes a visual control from its current parent in the visual tree.
    /// </summary>
    /// <param name="visual">The visual to remove from its parent.</param>
    private static void RemoveFromVisualParent(Visual visual)
    {
        var parent = visual.GetVisualParent();
        
        switch (parent)
        {
            case Panel panel when visual is Control control:
                panel.Children.Remove(control);
                break;
            case ContentPresenter contentPresenter:
                contentPresenter.Content = null;
                break;
            case ContentControl contentControl:
                contentControl.Content = null;
                break;
            case Decorator decorator:
                decorator.Child = null;
                break;
        }
    }
}
