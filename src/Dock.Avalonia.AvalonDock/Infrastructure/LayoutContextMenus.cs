using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives.PopupPositioning;
using AvalonDock.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Owns an invocation, not the application's menu or commands. A manager's two
// menus can be reused by different native tabs and floating hosts.
internal sealed class LayoutContextMenus
{
    private readonly LayoutMenuPresentation _presentation = new();
    private readonly LayoutContextMenuFocus _focus = new();
    private ContextMenu? _menu;
    private Control? _target;
    private Control? _owner;
    private LayoutItem? _item;
    private IDock? _pane;
    private ContextMenu? _previousTargetMenu;
    private bool _suppressOpeningKeyRelease;

    public void Show(ContextMenu? menu, Control target, LayoutItem? item, ContextRequestedEventArgs? request, IDock? pane,
        PixelPoint? screenPosition = null, bool placeAtOrigin = false)
    {
        Close();
        if (menu is null || item is { IsAttached: false }) return;
        menu.Close();
        _menu = menu;
        _target = target;
        Control owner = item ?? (Control)target;
        _owner = owner;
        _item = item;
        _pane = pane;
        _previousTargetMenu = owner.ContextMenu;
        _suppressOpeningKeyRelease = placeAtOrigin;
        menu.Opening += Opening;
        menu.Closing += Closing;
        menu.Closed += Closed;
        menu.AddHandler(MenuItem.SubmenuOpenedEvent, SubmenuOpened);
        if (placeAtOrigin) menu.AddHandler(InputElement.KeyUpEvent, OpeningKeyReleased, RoutingStrategies.Tunnel);
        target.DetachedFromVisualTree += TargetDetached;
        if (item is not null) item.PropertyChanged += ItemChanged;
        owner.ResourcesChanged += OwnerResourcesChanged;
        if (pane is AvaloniaObject observable) observable.PropertyChanged += PaneChanged;
        menu.SetCurrentValue(StyledElement.DataContextProperty, item);
        menu.SetCurrentValue(ContextMenu.PlacementProperty, request?.TryGetPosition(null, out _) == true ? PlacementMode.Pointer : PlacementMode.Bottom);
        menu.SetCurrentValue(ContextMenu.PlacementRectProperty, null);
        if (screenPosition.HasValue || placeAtOrigin)
        {
            menu.SetCurrentValue(ContextMenu.PlacementProperty, PlacementMode.AnchorAndGravity);
            menu.SetCurrentValue(ContextMenu.PlacementAnchorProperty, PopupAnchor.TopLeft);
            menu.SetCurrentValue(ContextMenu.PlacementGravityProperty, PopupGravity.BottomRight);
            Point position = screenPosition is { } point ? target.PointToClient(point) : default;
            menu.SetCurrentValue(ContextMenu.PlacementRectProperty, new Rect(position, default(Size)));
        }
        menu.SetCurrentValue(ContextMenu.PlacementTargetProperty, target);
        menu.SetCurrentValue(ContextMenu.HorizontalOffsetProperty, 0);
        menu.SetCurrentValue(ContextMenu.VerticalOffsetProperty, 0);
        owner.SetCurrentValue(Control.ContextMenuProperty, menu);
        _focus.Capture(target);
        // Use Avalonia's ordinary context-request path, including its cancelable
        // Opening event. The marker prevents fallback recursion if canceled.
        if (item is not null) item.RequestContextMenu(target, new LayoutContextMenuRequest(request));
        else target.RaiseEvent(new LayoutContextMenuRequest(request));
        if (!ReferenceEquals(_menu, menu)) menu.Close();
        else if (!menu.IsOpen) Close();
        else
        {
            // Avalonia's keyboard context request selects Bottom placement.
            // The manager hook instead anchors the popup at the content origin.
            if (placeAtOrigin && menu.Parent is Popup popup) popup.Placement = PlacementMode.AnchorAndGravity;
            menu.UpdateLayout();
            _presentation.Capture(menu);
        }
    }

    public void Release(ContextMenu? menu)
    {
        if (menu is not null && ReferenceEquals(menu, _menu)) Close();
    }

    public void Close()
    {
        if (_menu is not { } menu) return;
        _presentation.Capture(menu);
        _menu = null;
        menu.Opening -= Opening;
        menu.Closing -= Closing;
        menu.Closed -= Closed;
        menu.RemoveHandler(MenuItem.SubmenuOpenedEvent, SubmenuOpened);
        menu.RemoveHandler(InputElement.KeyUpEvent, OpeningKeyReleased);
        _suppressOpeningKeyRelease = false;
        // Close the popup before removing its logical owner so Avalonia can
        // detach its visuals while their theme and template are still active.
        menu.Close();
        if (menu.IsOpen && menu.Parent is Popup openPopup) openPopup.IsOpen = false;
        menu.SetCurrentValue(StyledElement.DataContextProperty, null);
        _presentation.Release(menu);
        if (_target is { } target)
        {
            target.DetachedFromVisualTree -= TargetDetached;
        }
        if (_owner is { } owner)
        {
            owner.ResourcesChanged -= OwnerResourcesChanged;
            if (ReferenceEquals(owner.ContextMenu, menu))
            {
                if (BindingOperations.GetBindingExpressionBase(owner, Control.ContextMenuProperty) is { } binding) binding.UpdateTarget();
                else owner.SetCurrentValue(Control.ContextMenuProperty, _previousTargetMenu);
            }
        }
        if (_item is { } item) item.PropertyChanged -= ItemChanged;
        if (_pane is AvaloniaObject pane) pane.PropertyChanged -= PaneChanged;
        _target = null;
        _owner = null;
        _item = null;
        _pane = null;
        _previousTargetMenu = null;
        menu.SetCurrentValue(ContextMenu.PlacementTargetProperty, null);
        if (menu.Parent is Popup popup) popup.PlacementTarget = null;
        _focus.Restore();
    }

    private void Opening(object? sender, CancelEventArgs e)
    {
        if (!ReferenceEquals(sender, _menu) || _item is { IsAttached: false }) e.Cancel = true;
    }

    private void OpeningKeyReleased(object? sender, KeyEventArgs e)
    {
        if (!_suppressOpeningKeyRelease || e.Key != Key.F10) return;
        _suppressOpeningKeyRelease = false;
        // Focus moved into the popup on key press. Do not let its ordinary
        // keyboard-toggle handler close it on that same shortcut's release.
        e.Handled = true;
    }

    private void Closing(object? sender, CancelEventArgs e)
    {
        if (ReferenceEquals(sender, _menu) && sender is ContextMenu menu) _presentation.Capture(menu);
    }

    private void SubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (_menu is not { } menu) return;
        menu.UpdateLayout();
        _presentation.Capture(menu);
    }

    private void Closed(object? sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, _menu)) Close();
    }
    private void TargetDetached(object? sender, VisualTreeAttachmentEventArgs e) => Close();
    private void OwnerResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        // ContextMenu's popup inherits from its owner without joining the owner's
        // logical children, so forward resource changes for the active invocation.
        if (_menu is IResourceHost menu) menu.NotifyHostedResourcesChanged(e);
    }

    private void ItemChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_item?.IsAttached != true) Close();
    }

    private void PaneChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(IDock.ActiveDockable) || _menu is null) return;
        if (_pane?.ActiveDockable is not { Factory: LayoutFactory factory } model || factory.GetLayoutItem(model) is not { } item)
        {
            Close();
            return;
        }
        if (_item is { } oldItem) oldItem.PropertyChanged -= ItemChanged;
        _item = item;
        item.PropertyChanged += ItemChanged;
        _menu.SetCurrentValue(StyledElement.DataContextProperty, item);
    }
}

internal sealed class LayoutContextMenuRequest(ContextRequestedEventArgs? source)
    : ContextRequestedEventArgs(source ?? new ContextRequestedEventArgs());
