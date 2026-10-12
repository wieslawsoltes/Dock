using Avalonia;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;
using Dock.Settings;

namespace AvalonDock.Infrastructure;

// Native tool chrome already owns its grip and window drag. Custom title hosts
// use the same factory movement and native window drag entry point.
internal sealed class LayoutAnchorableTitleInput(AnchorablePaneTitle title)
{
    private PointerPressedEventArgs? _press;

    internal void Cancel() => _press = null;

    internal void Pressed(PointerPressedEventArgs args)
    {
        Cancel();
        if (args.Handled || title.Model is not { } model || HasNativeGrip()) return;
        if (!CanDrag(model)) return;
        if (model.FindParent<LayoutAnchorableFloatingWindow>() is { IsSinglePane: true } floating &&
            floating.DockWindow.Host is HostWindow host)
        {
            args.Handled = host.TryBeginMoveDrag(args, WindowDragDockScope.FullWindow);
            return;
        }
        _press = args;
    }

    internal void Released(PointerReleasedEventArgs args)
    {
        Cancel();
        if (title.Model is { } model) model.IsActive = true;
    }

    internal void Moved(PointerEventArgs args)
    {
        if (!args.GetCurrentPoint(title).Properties.IsLeftButtonPressed) Cancel();
    }

    internal void Exited(PointerEventArgs args)
    {
        PointerPressedEventArgs? press = _press;
        Cancel();
        if (press is null || !title.IsEffectivelyEnabled || !args.GetCurrentPoint(title).Properties.IsLeftButtonPressed) return;
        LayoutAnchorablePane? pane = FindVisualPane();
        LayoutAnchorable? model = pane is null ? title.Model :
            pane.SelectedContent as LayoutAnchorable ?? (pane.Children.Count > 0 ? pane.Children[0] : null);
        if (model is not { Root.Manager: { } manager } || !CanDrag(model)) return;
        LayoutAnchorableFloatingWindow? previous = model.FindParent<LayoutAnchorableFloatingWindow>();
        if (pane is not null) manager.DockFactory.FloatAllDockables(model.DockModel);
        else model.Float();
        if (model.FindParent<LayoutAnchorableFloatingWindow>() is { } floating && !ReferenceEquals(floating, previous) &&
            floating.DockWindow.Host is HostWindow host)
            host.TryBeginMoveDrag(press, WindowDragDockScope.FullWindow);
    }

    private static bool CanDrag(LayoutAnchorable model) => model.Root?.Manager is not null &&
        DockCapabilityResolver.IsEnabled(model.DockModel, DockCapability.Drag, DockCapabilityResolver.ResolveOperationDock(model.DockModel));

    private bool HasNativeGrip()
    {
        for (Visual? current = title.GetVisualParent(); current is not null; current = current.GetVisualParent())
        {
            if (current is ToolChromeControl) return true;
            if (current is DockingManager) break;
        }
        return false;
    }

    private LayoutAnchorablePane? FindVisualPane()
    {
        for (Visual? parent = title.GetVisualParent(); parent is not null; parent = parent.GetVisualParent())
            if (parent is LayoutAnchorablePaneControl { Model: LayoutAnchorablePane pane }) return pane;
        return null;
    }
}
