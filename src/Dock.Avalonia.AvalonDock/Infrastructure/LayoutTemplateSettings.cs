using Avalonia;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

// Each presenter observes the two provider properties separately. Keeping the
// last delivered values prevents a failed or still-running callback from
// leaking another property's current value into an existing presentation.
internal readonly record struct LayoutTemplateSettings(IDataTemplate? Template, DataTemplateSelector? Selector)
{
    internal IDataTemplate? Select(object? data, AvaloniaObject container)
        => Template ?? Selector?.SelectTemplate(data, container);

    internal LayoutTemplateSettings Update(LayoutTemplateSettings current, bool? selectorChanged)
        => selectorChanged switch
        {
            true => new(Template, current.Selector),
            false => new(current.Template, Selector),
            null => current
        };

    internal bool Matches(LayoutTemplateSettings other)
        => ReferenceEquals(Template, other.Template) && ReferenceEquals(Selector, other.Selector);

    internal static LayoutTemplateSettings Content(DockingManager manager)
        => new(manager.LayoutItemTemplate, manager.LayoutItemTemplateSelector);

    internal static LayoutTemplateSettings Header(DockingManager manager, LayoutHeaderKind kind)
        => kind switch
        {
            LayoutHeaderKind.Document => new(manager.DocumentHeaderTemplate, manager.DocumentHeaderTemplateSelector),
            LayoutHeaderKind.Anchorable => new(manager.AnchorableHeaderTemplate, manager.AnchorableHeaderTemplateSelector),
            LayoutHeaderKind.AnchorableTitle => new(manager.AnchorableTitleTemplate, manager.AnchorableTitleTemplateSelector),
            LayoutHeaderKind.DocumentTitle => new(manager.DocumentTitleTemplate, manager.DocumentTitleTemplateSelector),
            LayoutHeaderKind.DocumentMenu => new(manager.DocumentPaneMenuItemHeaderTemplate, manager.DocumentPaneMenuItemHeaderTemplateSelector),
            _ => default
        };
}
