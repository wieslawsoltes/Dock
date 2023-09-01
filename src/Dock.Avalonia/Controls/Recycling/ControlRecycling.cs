using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls.Recycling;

/// <summary>
/// 
/// </summary>
public class ControlRecycling : IControlRecycling
{
    private readonly Dictionary<object, object> _cache = new();

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

        if (TryGetValue(data, out var control))
        {
#if DEBUG
            Console.WriteLine($"[Cached] {data}, {control}");
#endif
            return control;
        }

        var dataTemplate = (parent as Control)?.FindDataTemplate(data);

        control = dataTemplate?.Build(data);
        if (control is null)
        {
            return null;
        }

        Add(data, control);
#if DEBUG
        Console.WriteLine($"[Added] {data}, {control}");
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
