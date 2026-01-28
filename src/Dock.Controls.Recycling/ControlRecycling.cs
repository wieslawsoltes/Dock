// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private readonly Dictionary<object, object> _instanceCache = new(ReferenceEqualityComparer.Instance);
    private bool _tryToUseIdAsKey;

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);

        int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private static readonly AttachedProperty<object?> RecyclingDataProperty =
        AvaloniaProperty.RegisterAttached<ControlRecycling, Control, object?>("RecyclingData");

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

        if (_instanceCache.TryGetValue(data, out control))
        {
            return true;
        }

        if (TryToUseIdAsKey && data is IControlRecyclingIdProvider idProvider)
        {
            var id = idProvider.GetControlRecyclingId();
            if (!string.IsNullOrWhiteSpace(id) && _cache.TryGetValue(id, out control))
            {
                return true;
            }
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

        var idKey = TryToUseIdAsKey && key is IControlRecyclingIdProvider idProvider
            ? idProvider.GetControlRecyclingId()
            : null;
        if (!string.IsNullOrWhiteSpace(idKey))
        {
            if (_instanceCache.TryGetValue(key, out var instanceControl))
            {
                return UseCachedControl(key, idKey!, data, instanceControl, existing, parent as Control);
            }

            key = idKey!;
        }

        var parentControl = parent as Control;

        if (TryGetValue(key, out var control))
        {
            return UseCachedControl(key!, key!, data, control, existing, parentControl);
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
            if (!TryDetachFromParent(createdControl))
            {
                var fallback = BuildFallback(parentControl, data, existing);
                if (fallback is not null)
                {
                    Add(key!, fallback);
                }

                TrackInstanceControl(data, fallback);
                return fallback;
            }
        }

        Add(key!, control);
        TrackInstanceControl(data, control);

        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _instanceCache.Clear();
    }

    private static object? GetRecyclingData(Control control)
    {
        return control.GetValue(RecyclingDataProperty);
    }

    private void TrackInstanceControl(object? data, object? control)
    {
        if (data is null || control is not Control controlValue)
        {
            return;
        }

        SetCurrentInstance(controlValue, data);
    }

    private void RemoveInstanceControl(Control control)
    {
        var existing = GetRecyclingData(control);
        if (existing is not null)
        {
            _instanceCache.Remove(existing);
        }
    }

    private void SetCurrentInstance(Control control, object data)
    {
        var existing = GetRecyclingData(control);
        if (existing is not null && !ReferenceEquals(existing, data))
        {
            _instanceCache.Remove(existing);
        }

        control.SetValue(RecyclingDataProperty, data);
        _instanceCache[data] = control;
    }

    private object? UseCachedControl(object originalKey, object cacheKey, object? data, object? cached, object? existing, Control? parentControl)
    {
        if (cached is not Control cachedControl)
        {
            return cached;
        }

        if (data is not null && !ReferenceEquals(originalKey, data))
        {
            if (!ReferenceEquals(GetRecyclingData(cachedControl), data))
            {
                if (cachedControl.GetVisualParent() is not null)
                {
                    var fallbackControl = BuildFallback(parentControl, data, existing);
                    if (fallbackControl is not null)
                    {
                        Add(cacheKey, fallbackControl);
                    }

                    TrackInstanceControl(data, fallbackControl);
                    return fallbackControl;
                }

                RemoveInstanceControl(cachedControl);
            }
        }

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
            Add(cacheKey, updatedControl);
        }

        if (!ReferenceEquals(existing, updatedControl))
        {
            if (!TryDetachFromParent(updatedControl))
            {
                var fallback = BuildFallback(parentControl, data, existing);
                if (fallback is not null)
                {
                    Add(cacheKey, fallback);
                }

                TrackInstanceControl(data, fallback);
                return fallback;
            }
        }

        TrackInstanceControl(data, updatedControl);
        return updatedControl;
    }

    /// <summary>
    /// Removes a visual control from its current parent in the visual tree.
    /// </summary>
    /// <param name="visual">The visual to remove from its parent.</param>
    private static bool TryDetachFromParent(Visual visual)
    {
        var parent = (visual as Control)?.Parent ?? visual.GetVisualParent();

        if (parent is null)
        {
            return true;
        }
        
        switch (parent)
        {
            case Panel panel when visual is Control child:
                return panel.Children.Remove(child);
            case ContentPresenter contentPresenter:
                return TryDetachFromContentPresenter(contentPresenter, visual);
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, visual):
                contentControl.SetCurrentValue(ContentControl.ContentProperty, null);
                return true;
            case Decorator decorator when ReferenceEquals(decorator.Child, visual):
                decorator.Child = null;
                return true;
            default:
                return false;
        }
    }

    private static bool TryDetachFromContentPresenter(ContentPresenter presenter, Visual visual)
    {
        if (!ReferenceEquals(presenter.Child, visual))
        {
            return false;
        }

        presenter.SetCurrentValue(ContentPresenter.ContentProperty, null);
        presenter.UpdateChild();

        return visual.GetVisualParent() is null;
    }

    private static object? BuildFallback(Control? parentControl, object? data, object? existing)
    {
        if (parentControl is null)
        {
            return null;
        }

        var dataTemplate = parentControl.FindDataTemplate(data);
        if (dataTemplate is IRecyclingDataTemplate recyclingDataTemplate)
        {
            var existingControl = existing as Control;
            var control = recyclingDataTemplate.Build(data, existingControl);
            if (control is null)
            {
                control = recyclingDataTemplate.Build(data, null);
            }

            if (control is Control fallbackControl)
            {
                if (ReferenceEquals(fallbackControl, existingControl))
                {
                    return fallbackControl;
                }

                if (TryDetachFromParent(fallbackControl))
                {
                    return fallbackControl;
                }

                var rebuilt = recyclingDataTemplate.Build(data, null);
                if (rebuilt is Control rebuiltControl && TryDetachFromParent(rebuiltControl))
                {
                    return rebuiltControl;
                }

                return null;
            }

            return control;
        }

        var built = dataTemplate?.Build(data);
        if (built is Control builtControl)
        {
            if (TryDetachFromParent(builtControl))
            {
                return builtControl;
            }

            var rebuilt = dataTemplate?.Build(data);
            if (rebuilt is Control rebuiltControl && TryDetachFromParent(rebuiltControl))
            {
                return rebuiltControl;
            }

            return null;
        }

        return built;
    }
}
