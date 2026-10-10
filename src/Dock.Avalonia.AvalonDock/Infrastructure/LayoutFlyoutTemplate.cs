using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace AvalonDock.Infrastructure;

// Each chrome owns its flyout. A theme resource must not retain opened menus
// and their input/focus state after the hosting manager has been closed.
internal sealed class LayoutFlyoutTemplate : ITemplate
{
    [Content]
    [TemplateContent(TemplateResultType = typeof(MenuFlyout))]
    public object? Content { get; set; }

    public object? Build() => TemplateContent.Load<MenuFlyout>(Content)?.Result;
}
