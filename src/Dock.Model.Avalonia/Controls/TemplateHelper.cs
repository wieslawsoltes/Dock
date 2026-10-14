// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling;
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
        return controlRecycling is not null
            ? controlRecycling.Build(parent, existing, null) as Control
            : BuildFromTemplateContent(content);
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
