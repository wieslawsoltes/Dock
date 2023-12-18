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
