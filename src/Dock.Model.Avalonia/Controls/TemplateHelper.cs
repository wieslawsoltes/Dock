// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;

namespace Dock.Model.Avalonia.Controls;

internal static class TemplateHelper
{
    internal static Control? Build(object? content, AvaloniaObject parent)
    {
        if (content is null)
        {
            return null;
        }

        if (content is Control directControl)
        {
            return directControl;
        }

        var controlRecycling = ControlRecyclingDataTemplate.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            if (controlRecycling.TryGetValue(content, out var control))
            {
                return control as Control;
            }

            control = TemplateContent.Load(content)?.Result;
            if (control is not null)
            {
                controlRecycling.Add(content, control);
            }

            return control as Control;
        }

        return TemplateContent.Load(content)?.Result;
    }

    internal static TemplateResult<Control>? Load(object? templateContent)
    {
        if (templateContent is null)
        {
            return null;
        }

        if (templateContent is Control control)
        {
            return new TemplateResult<Control>(control, null!);
        }

        if (templateContent is Func<IServiceProvider, object> direct)
        {
            var result = direct(null!);
            
            // If the function returns a Control directly, wrap it in a TemplateResult
            if (result is Control resultControl)
            {
                return new TemplateResult<Control>(resultControl, null!);
            }
            
            // If the function returns a TemplateResult, cast and return it
            if (result is TemplateResult<Control> templateResult)
            {
                return templateResult;
            }
            
            // Otherwise try to cast (might throw if not compatible)
            return (TemplateResult<Control>)result;
        }

        return TemplateContent.Load(templateContent);
    }
}
