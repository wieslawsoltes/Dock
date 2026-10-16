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

    internal bool IsOpen => _menu?.IsOpen == true;

    internal void Show(ContextMenu menu, object? data, PointerEventArgs? pointer = null)
    {
        if (ReferenceEquals(_menu, menu) && menu.IsOpen)
        {
            Configure(menu, data, pointer);
            menu.UpdateLayout();
            _presentation.Capture(menu);
            return;
        }
        Close();
        if (TopLevel.GetTopLevel(target) is null) return;
        menu.Close();
        if (menu.IsOpen) return;
        _menu = menu;
        menu.Opening += Opening;
        menu.Closing += Closing;
        menu.Closed += Closed;
        menu.AddHandler(MenuItem.SubmenuOpenedEvent, SubmenuOpened);
        target.ResourcesChanged += ResourcesChanged;
        Configure(menu, data, pointer);
        _focus.Capture(target);
        try
        {
            Open(menu, pointer);
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

    private void Configure(ContextMenu menu, object? data, PointerEventArgs? pointer)
    {
        menu.SetCurrentValue(StyledElement.DataContextProperty, data);
        menu.SetCurrentValue(ContextMenu.PlacementProperty, pointer is null ? PlacementMode.Bottom : PlacementMode.Pointer);
        menu.SetCurrentValue(ContextMenu.PlacementTargetProperty, target);
        menu.SetCurrentValue(ContextMenu.HorizontalOffsetProperty, 0);
        menu.SetCurrentValue(ContextMenu.VerticalOffsetProperty, 0);
    }

    private void Open(ContextMenu menu, PointerEventArgs? pointer)
    {
        ContextMenu? previous = target.ContextMenu;
        bool restored = false;
        void Restore()
        {
            if (restored) return;
            restored = true;
            if (!ReferenceEquals(target.ContextMenu, menu)) return;
            if (BindingOperations.GetBindingExpressionBase(target, Control.ContextMenuProperty) is { } binding) binding.UpdateTarget();
            else target.SetCurrentValue(Control.ContextMenuProperty, previous);
        }
        void BeforeOpen(object? sender, CancelEventArgs e) => Restore();

        try
        {
            // Use the cancelable Avalonia opening route, then restore the
            // application's ordinary menu before the popup acquires its parent.
            // Restoring after opening would detach and close that popup.
            menu.Opening += BeforeOpen;
            target.SetCurrentValue(Control.ContextMenuProperty, menu);
            target.RaiseEvent(new LayoutContextMenuRequest(pointer is null ? null : new ContextRequestedEventArgs(pointer)));
        }
        finally
        {
            menu.Opening -= BeforeOpen;
            Restore();
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
        target.ResourcesChanged -= ResourcesChanged;
        menu.Close();
        menu.SetCurrentValue(StyledElement.DataContextProperty, null);
        _presentation.Release(menu);
        menu.SetCurrentValue(ContextMenu.PlacementTargetProperty, null);
        if (menu.Parent is Popup popup) popup.PlacementTarget = null;
        _focus.Restore();
        closed?.Invoke();
    }

    private void Opening(object? sender, CancelEventArgs e)
    {
        if (!ReferenceEquals(sender, _menu) || TopLevel.GetTopLevel(target) is null) e.Cancel = true;
    }
    private void Closing(object? sender, CancelEventArgs e)
    {
        if (ReferenceEquals(sender, _menu)) _presentation.Capture(_menu!);
    }
    private void Closed(object? sender, RoutedEventArgs e)
    {
        // A preceding Closed handler can reopen this same menu. The old
        // routed event must not close the replacement invocation.
        if (ReferenceEquals(sender, _menu) && !_menu!.IsOpen) Close();
    }
    private void SubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (_menu is not { } menu) return;
        menu.UpdateLayout();
        _presentation.Capture(menu);
    }

    private void ResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        if (_menu is IResourceHost menu) menu.NotifyHostedResourcesChanged(e);
    }
}
