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

            control = BuildFromTemplateContent(content);
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

        return BuildFromTemplateContent(content);
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

    private static Control? BuildFallback(object? content, Control? existing)
    {
        var built = BuildFromTemplateContent(content);
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

        var rebuilt = BuildFromTemplateContent(content);
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

    private static Control? BuildFromTemplateContent(object? content)
    {
        return Load(content)?.Result;
    }

    internal static TemplateResult<Control>? Load(object? templateContent)
    {
        if (templateContent is null)
        {
            return null;
        }

        if (templateContent is TemplateResult<Control> templateResult)
        {
            return templateResult;
        }

        if (templateContent is Control control)
        {
            return new TemplateResult<Control>(control, null!);
        }

        if (templateContent is Func<IServiceProvider, object> direct)
        {
            var evaluated = direct(null!);
            if (ReferenceEquals(evaluated, templateContent))
            {
                return null;
            }

            return Load(evaluated);
        }

        return TemplateContent.Load(templateContent);
    }
}
