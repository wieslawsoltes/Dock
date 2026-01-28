// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Recycling.Model;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;

namespace Dock.Model.Avalonia.Controls;

internal static class TemplateHelper
{
    internal static Control? Build(object? content, AvaloniaObject parent, Control? existing)
    {
        if (content is null)
        {
            return null;
        }

        if (content is Control directControl)
        {
            if (!ReferenceEquals(existing, directControl))
            {
                RemoveFromVisualParent(directControl);
            }
            return directControl;
        }

        var controlRecycling = ControlRecyclingDataTemplate.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            var key = GetCacheKey(controlRecycling, parent, content);
            if (controlRecycling.TryGetValue(key, out var control))
            {
                if (control is Control cachedControl)
                {
                    if (!ReferenceEquals(existing, cachedControl))
                    {
                        RemoveFromVisualParent(cachedControl);
                    }

                    return cachedControl;
                }

                return control as Control;
            }

            control = TemplateContent.Load(content)?.Result;
            if (control is not null)
            {
                controlRecycling.Add(key, control);
            }

            return control as Control;
        }

        return TemplateContent.Load(content)?.Result;
    }

    private static object GetCacheKey(IControlRecycling controlRecycling, AvaloniaObject parent, object content)
    {
        if (controlRecycling.TryToUseIdAsKey && parent is IControlRecyclingIdProvider idProvider)
        {
            var id = idProvider.GetControlRecyclingId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }

        return content;
    }

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
            return (TemplateResult<Control>)direct(null!);
        }

        return TemplateContent.Load(templateContent);
    }
}
