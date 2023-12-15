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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Dock.Settings;

namespace Dock.Model.Avalonia.Controls;

internal static class TemplateHelper
{
    internal static Control? Build(object? content, AvaloniaObject parent)
    {
        if (content is null)
        {
            return null;
        }

        var controlRecycling = DockProperties.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            if (controlRecycling.TryGetValue(content, out var control))
            {
#if DEBUG
                Console.WriteLine($"[Cached] {content}, {control}");
#endif
                return control as Control;
            }

            control = TemplateContent.Load(content)?.Result;
            if (control is not null)
            {
                controlRecycling.Add(content, control);
#if DEBUG
                Console.WriteLine($"[Added] {content}, {control}");
#endif
            }

            return control as Control;
        }

        return TemplateContent.Load(content)?.Result;
    }

    internal static TemplateResult<Control>? Load(object? templateContent)
    {
        if (templateContent is Func<IServiceProvider, object> direct)
        {
            return (TemplateResult<Control>)direct(null!);
        }
        throw new ArgumentException(nameof(templateContent));
    }
}
