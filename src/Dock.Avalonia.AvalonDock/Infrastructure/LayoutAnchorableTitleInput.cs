using Avalonia;
using Avalonia.Controls;
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
        if (args.Handled || !CanUseSource(args.Source) || title.Model is not { } model || HasNativeGrip()) return;
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
        if (!args.Handled && CanUseSource(args.Source) && title.Model is { } model) model.IsActive = true;
    }

    internal void Moved(PointerEventArgs args)
    {
        if (!args.GetCurrentPoint(title).Properties.IsLeftButtonPressed) Cancel();
    }

    internal void Exited(PointerEventArgs args)
    {
        PointerPressedEventArgs? press = _press;
        Cancel();
        if (press is null || !title.IsEffectivelyEnabled || !args.GetCurrentPoint(title).Properties.IsLeftButtonPressed ||
            title.Model is not { Root.Manager: { } manager } model || !CanDrag(model)) return;
        LayoutAnchorableFloatingWindow? previous = model.FindParent<LayoutAnchorableFloatingWindow>();
        if (model.Parent is LayoutAnchorablePane) manager.DockFactory.FloatAllDockables(model.DockModel);
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

    private bool CanUseSource(object? source)
    {
        if (!title.IsEffectivelyEnabled || title.Model is not { } model) return false;
        bool insideTitle = true;
        for (Visual? current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (ReferenceEquals(current, title)) insideTitle = false;
            else if (insideTitle && current is Control { Focusable: true }) return false;
            if (current is DockingManager manager) return ReferenceEquals(manager, model.Root?.Manager);
            if (current is LayoutFloatingWindowControl host) return ReferenceEquals(host.Model.Root?.Manager, model.Root?.Manager);
        }
        return true;
    }
}
