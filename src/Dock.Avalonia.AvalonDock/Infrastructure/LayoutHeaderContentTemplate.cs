using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutHeaderContentTemplate(LayoutTemplateSettings settings,
    IDataTemplate? fallback, ContentPresenter presenter) : IRecyclingDataTemplate
{
    private readonly ConditionalWeakTable<Control, IDataTemplate> _builtTemplates = new();

    internal LayoutTemplateSettings Settings { get; } = settings;
    internal IDataTemplate? Fallback { get; } = fallback;

    public bool Match(object? data) => data is LayoutContent;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        if (data is not LayoutContent) return null;
        IDataTemplate? template = Settings.Select(data, presenter) ?? Fallback;
        if (template is null) return null;
        if (existing is not null && (!_builtTemplates.TryGetValue(existing, out IDataTemplate? previous) ||
            !ReferenceEquals(previous, template))) existing = null;
        Control? result = template is IRecyclingDataTemplate recycling ? recycling.Build(data, existing) : template.Build(data);
        if (result is not null) _builtTemplates.AddOrUpdate(result, template);
        return result;
    }
}
