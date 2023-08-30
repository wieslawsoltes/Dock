using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Dock.Avalonia.Controls.Recycling;

/// <summary>
/// 
/// </summary>
public class ControlRecycling
{
    private readonly Dictionary<object?, Control> _cache = new();

    private bool TryGetValue(object? data, out Control? control)
    {
        if (data is null)
        {
            control = null;
            return false;
        }

        return _cache.TryGetValue(data, out control);
    }

    private void Add(object data, Control control)
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
    public Control? Build(object? data, Control? existing, Control? parent)
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

        var dataTemplate = parent?.FindDataTemplate(data, null);

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
}
