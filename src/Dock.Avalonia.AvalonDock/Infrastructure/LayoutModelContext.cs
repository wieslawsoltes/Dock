using System;
using System.Runtime.CompilerServices;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

// A weak identity map shared by nodes when trees join. Removed models are not
// retained by the layout, and a Dock move preserves the public wrapper identity.
internal sealed class LayoutModelContext
{
    private readonly ConditionalWeakTable<IDockable, LayoutElement> _elements = new();
    private LayoutModelContext? _parent;
    private LayoutModelContext Current => _parent is null ? this : _parent = _parent.Current;

    internal void Register(LayoutElement element) => Current._elements.AddOrUpdate(element.DockModel, element);

    internal void Adopt(LayoutModelContext other)
    {
        LayoutModelContext target = Current;
        LayoutModelContext source = other.Current;
        if (ReferenceEquals(target, source)) return;
        foreach (var pair in source._elements) target._elements.AddOrUpdate(pair.Key, pair.Value);
        source._elements.Clear();
        source._parent = target;
    }

    internal LayoutElement Get(IDockable model)
    {
        LayoutModelContext current = Current;
        if (current._elements.TryGetValue(model, out LayoutElement? element)) return element;
        return model switch
        {
            Document document => new LayoutDocument(document, current),
            Tool tool => new LayoutAnchorable(tool, current),
            DocumentDock documents => new LayoutDocumentPane(documents, current),
            ToolDock tools => new LayoutAnchorablePane(tools, current),
            ProportionalDock panel => new LayoutPanel(panel, current),
            RootDock root => new LayoutRoot(root, current),
            _ => throw new NotSupportedException($"The Dock node '{model.Id}' has no AvalonDock adapter.")
        };
    }
}
