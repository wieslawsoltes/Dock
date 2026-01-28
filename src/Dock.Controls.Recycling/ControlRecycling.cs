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

        var parentControl = parent as Control;

        if (TryGetValue(key, out var control))
        {
            if (control is Control cachedControl)
            {
                var updatedControl = cachedControl;

                if (parentControl is not null)
                {
                    var template = parentControl.FindDataTemplate(data);
                    if (template is IRecyclingDataTemplate recyclingTemplate)
                    {
                        var recycled = recyclingTemplate.Build(data, cachedControl);
                        if (recycled is not null)
                        {
                            updatedControl = recycled;
                        }
                    }
                }

                if (!ReferenceEquals(updatedControl, cachedControl))
                {
                    Add(key!, updatedControl);
                }

                if (!ReferenceEquals(existing, updatedControl))
                {
                    RemoveFromVisualParent(updatedControl);
                }

                return updatedControl;
            }

            return control;
        }

        var dataTemplate = parentControl?.FindDataTemplate(data);
        if (dataTemplate is IRecyclingDataTemplate recyclingDataTemplate)
        {
            control = recyclingDataTemplate.Build(data, existing as Control);
        }
        else
        {
            control = dataTemplate?.Build(data);
        }
        if (control is null)
        {
            return null;
        }

        if (control is Control createdControl && !ReferenceEquals(existing, createdControl))
        {
            RemoveFromVisualParent(createdControl);
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
        var parent = (visual as Control)?.Parent ?? visual.GetVisualParent();
        
        switch (parent)
        {
            case Panel panel when visual is Control child:
                panel.Children.Remove(child);
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
