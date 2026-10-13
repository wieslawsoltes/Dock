using System;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Resolve against the incoming content during Build, so changing content types
// cannot briefly apply the previous compiled template to the new ViewModel.
internal sealed class LayoutItemContentTemplate(LayoutContent model, DockingManager manager, ContentPresenter presenter,
    LayoutTemplateSettings settings)
    : IRecyclingDataTemplate
{
    private readonly WeakReference<DockingManager> _manager = new(manager);
    private readonly ConditionalWeakTable<Control, IDataTemplate> _builtTemplates = new();

    internal LayoutTemplateSettings Settings { get; } = settings;

    public bool Match(object? data) => true;
    public Control? Build(object? data) => Build(data, null);

    public Control? Build(object? data, Control? existing)
    {
        _manager.TryGetTarget(out DockingManager? owner);
        IDataTemplate? template = Settings.Select(data, presenter);
        if (template is null)
        {
            // Delegate explicit controls and native template content to Dock's
            // existing ownership, deferred-content, and recycling machinery.
            if (data is null or Control or ITemplate or IDataTemplate or Func<IServiceProvider, object>)
                return data is null ? null : ((IRecyclingDataTemplate)model.DockModel).Build(model.DockModel, existing ?? presenter.Child);
            template = owner?.FindDataTemplate(data) ?? FuncDataTemplate.Default;
        }

        if (existing is not null && (!_builtTemplates.TryGetValue(existing, out IDataTemplate? previous) ||
            !ReferenceEquals(previous, template))) existing = null;
        Control? result = template is IRecyclingDataTemplate recycling ? recycling.Build(data, existing) : template.Build(data);
        if (result is not null) _builtTemplates.AddOrUpdate(result, template);
        return result;
    }
}
