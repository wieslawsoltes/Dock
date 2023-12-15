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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls.Recycling;

/// <summary>
/// 
/// </summary>
public class ControlRecyclingDataTemplate : AvaloniaObject, IRecyclingDataTemplate
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly StyledProperty<Control?> ParentProperty =
        AvaloniaProperty.Register<ControlRecyclingDataTemplate, Control?>(nameof(Parent));

    /// <summary>
    /// 
    /// </summary>
    public Control? Parent
    {
        get => GetValue(ParentProperty);
        set => SetValue(ParentProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public Control? Build(object? param)
    {
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Match(object? data)
    {
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="existing"></param>
    /// <returns></returns>
    public Control? Build(object? data, Control? existing)
    {
        if (Parent is not { } parent)
        {
            return null;
        }

        var controlRecycling = DockProperties.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            return controlRecycling.Build(data, existing, parent) as Control;
        }

        return parent.FindDataTemplate(data)?.Build(data);
    }
}
