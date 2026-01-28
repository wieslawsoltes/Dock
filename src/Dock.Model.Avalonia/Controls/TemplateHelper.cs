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
                if (!TryDetachFromParent(directControl))
                {
                    return existing;
                }
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
                        if (!TryDetachFromParent(cachedControl))
                        {
                            var fallback = BuildFallback(content, existing);
                            if (fallback is not null)
                            {
                                controlRecycling.Add(key, fallback);
                            }

                            return fallback;
                        }
                    }

                    return cachedControl;
                }

                return control as Control;
            }

            control = TemplateContent.Load(content)?.Result;
            if (control is not null)
            {
                if (control is Control builtControl && !ReferenceEquals(existing, builtControl))
                {
                    if (!TryDetachFromParent(builtControl))
                    {
                        var fallback = BuildFallback(content, existing);
                        if (fallback is not null)
                        {
                            controlRecycling.Add(key, fallback);
                        }

                        return fallback;
                    }
                }

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
            case ContentPresenter contentPresenter when ReferenceEquals(contentPresenter.Content, visual):
                contentPresenter.Content = null;
                return true;
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, visual):
                contentControl.Content = null;
                return true;
            case Decorator decorator when ReferenceEquals(decorator.Child, visual):
                decorator.Child = null;
                return true;
            default:
                return false;
        }
    }

    private static Control? BuildFallback(object? content, Control? existing)
    {
        var built = TemplateContent.Load(content)?.Result;
        if (built is null)
        {
            return existing;
        }

        if (ReferenceEquals(built, existing))
        {
            return built;
        }

        if (TryDetachFromParent(built))
        {
            return built;
        }

        var rebuilt = TemplateContent.Load(content)?.Result;
        if (rebuilt is null)
        {
            return existing;
        }

        if (ReferenceEquals(rebuilt, existing))
        {
            return rebuilt;
        }

        return TryDetachFromParent(rebuilt) ? rebuilt : existing;
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
