using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Registers the public content presentation with Dock's existing drag preview
// lookup. External standalone content controls must not replace a live editor.
internal sealed class LayoutContentPreviewRegistration
{
    private readonly Control _control;
    private LayoutContent? _model;
    private bool _native;
    private IDictionary<IDockable, object>? _registry;
    private IDockable? _registered;

    internal LayoutContentPreviewRegistration(Control control)
    {
        _control = control;
        control.AttachedToVisualTree += OnAttached;
        control.DetachedFromVisualTree += OnDetached;
    }

    internal void Refresh(LayoutContent? model, bool native)
    {
        Unregister();
        _model = model;
        _native = native;
        if (native && model?.DockModel is { Factory: { } factory } dockable &&
            TopLevel.GetTopLevel(_control) is not null)
        {
            _registry = dockable is ITool ? factory.ToolControls : factory.DocumentControls;
            _registered = dockable;
            _registry[dockable] = _control;
        }
    }

    private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e) => Refresh(_model, _native);
    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) => Unregister();

    private void Unregister()
    {
        if (_registry is { } registry && _registered is { } model &&
            registry.TryGetValue(model, out object? current) && ReferenceEquals(current, _control))
            registry.Remove(model);
        _registry = null;
        _registered = null;
    }
}
