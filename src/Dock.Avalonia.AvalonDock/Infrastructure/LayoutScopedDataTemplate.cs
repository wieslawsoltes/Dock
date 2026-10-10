using System;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

// A separate floating root resolves application templates in the manager's
// scope. Resolve on each build so template and logical-parent changes remain
// visible, and do not retain a manager through a detached native window.
internal sealed class LayoutScopedDataTemplate(Control owner) : IRecyclingDataTemplate
{
    private readonly WeakReference<Control> _owner = new(owner);
    private readonly ConditionalWeakTable<Control, IDataTemplate> _builtTemplates = new();

    public bool Match(object? data) => Find(data) is not null;
    public Control? Build(object? data) => Build(data, null);
    public Control? Build(object? data, Control? existing)
    {
        IDataTemplate? template = Find(data);
        if (template is null) return null;
        if (existing is not null && (!_builtTemplates.TryGetValue(existing, out IDataTemplate? previous) ||
            !ReferenceEquals(previous, template))) existing = null;
        Control? result = template is IRecyclingDataTemplate recycling ? recycling.Build(data, existing) : template.Build(data);
        if (result is not null) _builtTemplates.AddOrUpdate(result, template);
        return result;
    }

    private IDataTemplate? Find(object? data) => _owner.TryGetTarget(out Control? owner) ? owner.FindDataTemplate(data) : null;
}
