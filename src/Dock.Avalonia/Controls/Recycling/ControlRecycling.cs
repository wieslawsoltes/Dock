using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls.Recycling;

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
        if (data is null)
        {
            return null;
        }

        var key = data;

        if (TryToUseIdAsKey && data is IDockable dockable)
        {
#if DEBUG
            Console.WriteLine($"Build: {data}, Id='{dockable.Id}'");
#endif
            if (!string.IsNullOrWhiteSpace(dockable.Id))
            {
                key = dockable.Id;
            }
        }

        if (TryGetValue(key, out var control))
        {
#if DEBUG
            Console.WriteLine($"[Cached] {key}, {control}");
#endif
            return control;
        }

        var dataTemplate = (parent as Control)?.FindDataTemplate(data);

        control = dataTemplate?.Build(data);
        if (control is null)
        {
            return null;
        }

        Add(key, control);
#if DEBUG
        Console.WriteLine($"[Added] {key}, {control}");
#endif
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
