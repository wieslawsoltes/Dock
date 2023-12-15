/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
