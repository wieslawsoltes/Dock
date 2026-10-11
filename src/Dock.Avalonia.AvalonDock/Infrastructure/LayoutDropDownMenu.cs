using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvalonDock.Infrastructure;

// The public dropdown controls own an invocation, while the application owns
// the menu, its items, bindings, and commands. Reuse the native menu cleanup.
internal sealed class LayoutDropDownMenu(Control target, Action? closed = null)
{
    private readonly LayoutMenuPresentation _presentation = new();
    private readonly LayoutContextMenuFocus _focus = new();
    private ContextMenu? _menu;
    private ContextMenu? _previous;

    internal bool IsOpen => _menu?.IsOpen == true;

    internal void Show(ContextMenu menu, object? data, PointerEventArgs? pointer = null)
    {
        Close();
        if (TopLevel.GetTopLevel(target) is null || !target.IsEffectivelyEnabled) return;
        menu.Close();
        if (menu.IsOpen) return;
        _menu = menu;
        _previous = target.ContextMenu;
        menu.Opening += Opening;
        menu.Closing += Closing;
        menu.Closed += Closed;
        menu.AddHandler(MenuItem.SubmenuOpenedEvent, SubmenuOpened);
        menu.SetCurrentValue(StyledElement.DataContextProperty, data);
        menu.SetCurrentValue(ContextMenu.PlacementProperty, pointer is null ? PlacementMode.Bottom : PlacementMode.Pointer);
        menu.SetCurrentValue(ContextMenu.PlacementTargetProperty, target);
        menu.SetCurrentValue(ContextMenu.HorizontalOffsetProperty, 0);
        menu.SetCurrentValue(ContextMenu.VerticalOffsetProperty, 0);
        target.SetCurrentValue(Control.ContextMenuProperty, menu);
        _focus.Capture(target);
        try
        {
            target.RaiseEvent(new LayoutContextMenuRequest(pointer is null ? null : new ContextRequestedEventArgs(pointer)));
            if (!ReferenceEquals(_menu, menu)) { menu.Close(); return; }
            if (!menu.IsOpen) Close();
            else
            {
                menu.UpdateLayout();
                _presentation.Capture(menu);
            }
        }
        catch
        {
            Close();
            throw;
        }
    }

    internal void Close()
    {
        if (_menu is not { } menu) return;
        _menu = null;
        _presentation.Capture(menu);
        menu.Opening -= Opening;
        menu.Closing -= Closing;
        menu.Closed -= Closed;
        menu.RemoveHandler(MenuItem.SubmenuOpenedEvent, SubmenuOpened);
        menu.Close();
        menu.SetCurrentValue(StyledElement.DataContextProperty, null);
        _presentation.Release(menu);
        if (ReferenceEquals(target.ContextMenu, menu))
        {
            if (BindingOperations.GetBindingExpressionBase(target, Control.ContextMenuProperty) is { } binding) binding.UpdateTarget();
            else target.SetCurrentValue(Control.ContextMenuProperty, _previous);
        }
        _previous = null;
        menu.SetCurrentValue(ContextMenu.PlacementTargetProperty, null);
        if (menu.Parent is Popup popup) popup.PlacementTarget = null;
        _focus.Restore();
        closed?.Invoke();
    }

    private void Opening(object? sender, CancelEventArgs e)
    {
        if (!ReferenceEquals(sender, _menu) || TopLevel.GetTopLevel(target) is null || !target.IsEffectivelyEnabled) e.Cancel = true;
    }
    private void Closing(object? sender, CancelEventArgs e)
    {
        if (ReferenceEquals(sender, _menu)) _presentation.Capture(_menu!);
    }
    private void Closed(object? sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, _menu)) Close();
    }
    private void SubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (_menu is not { } menu) return;
        menu.UpdateLayout();
        _presentation.Capture(menu);
    }
}
