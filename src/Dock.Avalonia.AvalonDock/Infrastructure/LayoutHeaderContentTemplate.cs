using System;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutHeaderContentTemplate(DockingManager manager, LayoutHeaderKind kind,
    IDataTemplate? fallback, ContentPresenter presenter) : IRecyclingDataTemplate
{
    private readonly WeakReference<DockingManager> _manager = new(manager);
    private readonly ConditionalWeakTable<Control, IDataTemplate> _builtTemplates = new();

    public bool Match(object? data) => data is LayoutContent;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        if (data is not LayoutContent) return null;
        _manager.TryGetTarget(out DockingManager? owner);
        IDataTemplate? template = kind switch
        {
            LayoutHeaderKind.Document => owner?.DocumentHeaderTemplateSelector?.SelectTemplate(data, presenter) ?? owner?.DocumentHeaderTemplate,
            LayoutHeaderKind.Anchorable => owner?.AnchorableHeaderTemplateSelector?.SelectTemplate(data, presenter) ?? owner?.AnchorableHeaderTemplate,
            LayoutHeaderKind.AnchorableTitle => owner?.AnchorableTitleTemplateSelector?.SelectTemplate(data, presenter) ?? owner?.AnchorableTitleTemplate,
            _ => null
        };
        template ??= fallback;
        if (template is null) return null;
        if (existing is not null && (!_builtTemplates.TryGetValue(existing, out IDataTemplate? previous) ||
            !ReferenceEquals(previous, template))) existing = null;
        Control? result = template is IRecyclingDataTemplate recycling ? recycling.Build(data, existing) : template.Build(data);
        if (result is not null) _builtTemplates.AddOrUpdate(result, template);
        return result;
    }
}
