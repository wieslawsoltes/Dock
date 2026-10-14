// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Recycling.Model;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;

namespace Dock.Model.Avalonia.Controls;

internal sealed class TemplateHelper
{
    private ConditionalWeakTable<Control, WeakReference<object>>? _cachedContents;

    internal Control? Build(object? content, AvaloniaObject parent, Control? existing)
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
            var key = GetCacheKey(controlRecycling, parent);
            if (controlRecycling.TryGetValue(key, out var cached) && CanReuse(cached, content))
            {
                if (cached is Control cachedControl)
                {
                    if (!ReferenceEquals(existing, cachedControl))
                    {
                        if (!TryDetachFromParent(cachedControl))
                        {
                            var fallback = BuildFallback(content, existing);
                            if (fallback is not null)
                            {
                                controlRecycling.Add(key, fallback);
                                RememberContent(fallback, content);
                            }

                            return fallback ?? existing;
                        }
                    }

                    RememberContent(cachedControl, content);
                    return cachedControl;
                }

                return cached as Control;
            }

            var control = BuildFromTemplateContent(content);
            if (control is not null)
            {
                if (!ReferenceEquals(existing, control))
                {
                    if (!TryDetachFromParent(control))
                    {
                        var fallback = BuildFallback(content, existing);
                        if (fallback is not null)
                        {
                            controlRecycling.Add(key, fallback);
                            RememberContent(fallback, content);
                        }

                        return fallback ?? existing;
                    }
                }

                controlRecycling.Add(key, control);
                RememberContent(control, content);
            }

            return control;
        }

        return BuildFromTemplateContent(content);
    }

    private bool CanReuse(object? control, object content)
    {
        // A dockable can revisit several recycling scopes or IDs. Validate the
        // actual cached view, rather than only the most recently built template.
        return control is not Control cachedControl
               || _cachedContents is null
               || !_cachedContents.TryGetValue(cachedControl, out var cachedContent)
               || (cachedContent.TryGetTarget(out var previous) && ReferenceEquals(previous, content));
    }

    private void RememberContent(Control control, object content)
    {
        // Neither replaced templates nor released views should be kept alive by
        // this bookkeeping. Unknown views still support explicit-ID recycling.
        var cachedContents = _cachedContents ??= new ConditionalWeakTable<Control, WeakReference<object>>();
        if (cachedContents.TryGetValue(control, out var cachedContent))
        {
            cachedContent.SetTarget(content);
        }
        else
        {
            cachedContents.Add(control, new WeakReference<object>(content));
        }
    }

    private static object GetCacheKey(IControlRecycling controlRecycling, AvaloniaObject parent)
    {
        if (controlRecycling.TryToUseIdAsKey && parent is IControlRecyclingIdProvider idProvider)
        {
            var id = idProvider.GetControlRecyclingId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }

        // Template content is shared by all containers generated from ItemsSource.
        // Cache its realized control per container so siblings never steal each other's views.
        return parent;
    }

    private static bool TryDetachFromParent(Visual visual)
    {
        if (visual.GetVisualParent() is ContentPresenter visualContentPresenter)
        {
            return TryDetachFromContentPresenter(visualContentPresenter, visual);
        }

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
            return null;
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
            return null;
        }

        if (ReferenceEquals(rebuilt, existing))
        {
            return rebuilt;
        }

        return TryDetachFromParent(rebuilt) ? rebuilt : null;
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
