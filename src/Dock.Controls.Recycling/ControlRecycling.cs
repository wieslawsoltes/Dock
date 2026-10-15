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
    private readonly Dictionary<object, CacheEntry> _cache = new();
    private bool _tryToUseIdAsKey;

    private readonly record struct CacheEntry(object Control, ControlRecyclingVersion? Version);

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
    /// Looks up a control using the data or its enabled recycling ID. A content
    /// revision mismatch from the same source is a cache miss.
    /// </summary>
    /// <param name="data">The data, template owner, or explicit cache key.</param>
    /// <param name="control">The cached control when the entry is current.</param>
    /// <returns>Whether a current entry exists.</returns>
    public bool TryGetValue(object? data, out object? control)
    {
        if (data is null)
        {
            control = null;
            return false;
        }

        var version = (data as IControlRecyclingVersionProvider)?.GetControlRecyclingVersion();
        if (_cache.TryGetValue(GetCacheKey(data), out var entry)
            && !(entry.Version is { } previous && version is { } current
                 && ReferenceEquals(previous.Source, current.Source)
                 && previous.Revision != current.Revision))
        {
            control = entry.Control;
            return true;
        }

        control = null;
        return false;
    }

    /// <summary>
    /// Stores a control under the data or its enabled recycling ID, recording
    /// the current content revision when the data supplies one.
    /// </summary>
    /// <param name="data">The data, template owner, or explicit cache key.</param>
    /// <param name="control">The successfully created or reused control.</param>
    public void Add(object data, object control)
    {
        // A successful build or reuse commits the current source and revision.
        // In particular, an explicit-ID handoff adopts the replacement source.
        _cache[GetCacheKey(data)] = new CacheEntry(control,
            (data as IControlRecyclingVersionProvider)?.GetControlRecyclingVersion());
    }

    /// <summary>
    /// Reuses or creates content from a supplied control template, or from the
    /// data template found on the parent. Only successful results enter the cache.
    /// </summary>
    /// <param name="data">The data or an <see cref="ITemplate{T}"/> producing a control.</param>
    /// <param name="existing">The control already presented by the caller.</param>
    /// <param name="parent">The control used to locate a data template when needed.</param>
    /// <returns>The realized content, or the existing control if template content cannot be reparented.</returns>
    public object? Build(object? data, object? existing, object? parent)
    {
        if (data is null)
        {
            return null;
        }

        // Capture before running template code: it may change its own content or ID.
        var key = GetCacheKey(data);
        var version = (data as IControlRecyclingVersionProvider)?.GetControlRecyclingVersion();
        var contentTemplate = data as ITemplate<Control?>;
        var parentControl = parent as Control;

        if (TryGetValue(data, out var control))
        {
            if (control is Control cachedControl)
            {
                var updatedControl = cachedControl;

                if (contentTemplate is null && parentControl is not null)
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

                if (!ReferenceEquals(existing, updatedControl))
                {
                    if (!TryDetachFromParent(updatedControl))
                    {
                        var fallback = BuildFallback(parentControl, data, existing);
                        if (fallback is not null)
                        {
                            _cache[key] = new CacheEntry(fallback, version);
                        }

                        return fallback ?? (contentTemplate is not null ? existing : null);
                    }
                }

                _cache[key] = new CacheEntry(updatedControl, version);
                return updatedControl;
            }

            _cache[key] = new CacheEntry(control!, version);
            return control;
        }

        var dataTemplate = contentTemplate is null ? parentControl?.FindDataTemplate(data) : null;
        if (contentTemplate is not null)
        {
            control = contentTemplate.Build();
        }
        else if (dataTemplate is IRecyclingDataTemplate recyclingDataTemplate)
        {
            control = recyclingDataTemplate.Build(data, null);
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
                    _cache[key] = new CacheEntry(fallback, version);
                }

                return fallback ?? (contentTemplate is not null ? existing : null);
            }
        }

        _cache[key] = new CacheEntry(control, version);

        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public bool Remove(object data)
    {
        if (data is null)
        {
            return false;
        }

        var key = GetCacheKey(data);
        return _cache.Remove(key);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    private object GetCacheKey(object data)
    {
        var key = data;
        if (TryToUseIdAsKey && data is IControlRecyclingIdProvider idProvider)
        {
            var id = idProvider.GetControlRecyclingId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                key = id;
            }
        }

        return key;
    }

    /// <summary>
    /// Removes a visual control from its current parent in the visual tree.
    /// </summary>
    /// <param name="visual">The visual to remove from its parent.</param>
    private static bool TryDetachFromParent(Visual visual)
    {
        if (visual.GetVisualParent() is ContentPresenter visualContentPresenter)
        {
            return TryDetachFromContentPresenter(visualContentPresenter, visual);
        }

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
        if (data is ITemplate<Control?> contentTemplate)
        {
            var contentBuilt = contentTemplate.Build();
            if (contentBuilt is null || ReferenceEquals(contentBuilt, existing) || TryDetachFromParent(contentBuilt))
            {
                return contentBuilt;
            }

            var rebuiltContent = contentTemplate.Build();
            return rebuiltContent is not null && (ReferenceEquals(rebuiltContent, existing) || TryDetachFromParent(rebuiltContent))
                ? rebuiltContent
                : null;
        }

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
