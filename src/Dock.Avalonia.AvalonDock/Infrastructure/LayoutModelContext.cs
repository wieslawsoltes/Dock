using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// A weak identity map shared by nodes when trees join. Removed models are not
// retained by the layout, and a Dock move preserves the public wrapper identity.
internal sealed class LayoutModelContext
{
    private static readonly AttachedProperty<WeakReference<LayoutElement>?> ElementProperty =
        AvaloniaProperty.RegisterAttached<LayoutModelContext, DockableBase, WeakReference<LayoutElement>?>("Element");
    private readonly ConditionalWeakTable<IDockable, LayoutElement> _elements = new();
    private LayoutModelContext? _parent;
    private LayoutModelContext Current => _parent is null ? this : _parent = _parent.Current;

    internal void Register(LayoutElement element)
    {
        Current._elements.AddOrUpdate(element.DockModel, element);
        if (element.DockModel.GetValue(ElementProperty) is { } reference) reference.SetTarget(element);
        else element.DockModel.SetValue(ElementProperty, new WeakReference<LayoutElement>(element));
    }

    private LayoutElement? FindExisting(IDockable model)
    {
        LayoutModelContext current = Current;
        if (current._elements.TryGetValue(model, out LayoutElement? element)) return element;
        // A native owner assignment can join previously unrelated public trees.
        // The model's weak identity preserves an existing wrapper before joining contexts.
        if (model is DockableBase native && native.GetValue(ElementProperty) is { } reference && reference.TryGetTarget(out element))
        {
            current.Adopt(element.Context);
            return element;
        }
        return null;
    }

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
        if (FindExisting(model) is { } element) return element;
        return model switch
        {
            Document document => new LayoutDocument(document, current),
            Tool tool => new LayoutAnchorable(tool, current),
            DocumentDock documents => new LayoutDocumentPane(documents, current),
            ToolDock tools => new LayoutAnchorablePane(tools, current),
            ProportionalDock panel => new LayoutPanel(panel, current),
            RootDock { Window: { } window } => GetWindow(window),
            RootDock root => new LayoutRoot(root, current),
            _ => throw new NotSupportedException($"The Dock node '{model.Id}' has no AvalonDock adapter.")
        };
    }

    internal LayoutAnchorablePaneGroup GetAnchorableGroup(ProportionalDock model)
    {
        LayoutModelContext current = Current;
        if (FindExisting(model) is LayoutAnchorablePaneGroup group) return group;
        if (model.VisibleDockables is { } children)
            foreach (IDockable child in children) if (child is ProportionalDock nested) GetAnchorableGroup(nested);
        return new LayoutAnchorablePaneGroup(model, current);
    }

    internal LayoutFloatingWindow GetWindow(IDockWindow window)
    {
        LayoutModelContext current = Current;
        if (window is not DockWindow native || window.Layout is not RootDock root)
            throw new NotSupportedException("Floating layouts require a Dock Avalonia window and root.");
        if (FindExisting(root) is LayoutFloatingWindow floating) return floating;
        if (root.VisibleDockables is { Count: > 0 } children && children[0] is DocumentDock)
            return new LayoutDocumentFloatingWindow(native, current);
        return new LayoutAnchorableFloatingWindow(native, current);
    }
}
