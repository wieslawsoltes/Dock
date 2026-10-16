using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutNavigatorFocus : IDisposable
{
    private static readonly AttachedProperty<LayoutNavigatorFocus?> PendingProperty =
        AvaloniaProperty.RegisterAttached<LayoutNavigatorFocus, DockingManager, LayoutNavigatorFocus?>("PendingNavigatorFocus");

    private readonly WeakReference<DockingManager> _owner;
    private readonly WeakReference<LayoutItem> _item;
    private readonly WeakReference<Control> _content;
    private readonly bool _requestedActive;
    private WeakReference<TopLevel>? _focusScope;
    private bool _awaitingActivation = true;
    private bool _queued;
    private bool _disposed;

    private LayoutNavigatorFocus(DockingManager owner, LayoutItem item, Control content)
    {
        _owner = new(owner);
        _item = new(item);
        _content = new(content);
        _requestedActive = item.IsActive;
        owner.DetachedFromVisualTree += Detached;
        item.PropertyChanged += ItemChanged;
        content.AttachedToVisualTree += Attached;
        content.DetachedFromVisualTree += Detached;
    }

    internal static void Restore(LayoutItem item)
    {
        if (item.Manager is not { } owner) return;
        owner.GetValue(PendingProperty)?.Dispose();
        if (item.LayoutElement?.Content is not Control content) return;
        LayoutNavigatorFocus request = new(owner, item, content);
        owner.SetValue(PendingProperty, request);
        // Closing a modal navigator queues activation of its owner. Reassert the
        // selected floating host through Dock, after that activation request.
        if (item.LayoutElement?.FindParent<LayoutFloatingWindow>()?.DockWindow.Host is { } host &&
            host is Window { IsVisible: true }) host.SetActive();
        request.Queue();
    }

    private void Attached(object? sender, VisualTreeAttachmentEventArgs args) => Queue();
    private void Detached(object? sender, VisualTreeAttachmentEventArgs args) => Dispose();
    private void FocusChanged(object? sender, FocusChangedEventArgs args) => Dispose();

    private void ItemChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == LayoutItem.LayoutElementProperty || change.Property == LayoutItem.ModelProperty ||
            (!_awaitingActivation && (change.Property == LayoutItem.IsActiveProperty || change.Property == LayoutItem.IsSelectedProperty) && change.NewValue is false))
            Dispose();
    }

    private void Queue()
    {
        if (_disposed || _queued) return;
        _queued = true;
        // Attachment can happen after several delayed or budgeted Dock passes.
        // Run after the content's descendants have joined the visual tree too.
        Dispatcher.UIThread.Post(TryFocus, DispatcherPriority.ContextIdle);
    }

    private void TryFocus()
    {
        _queued = false;
        if (_disposed) return;
        if (!_item.TryGetTarget(out LayoutItem? item) || !item.IsAttached ||
            !_content.TryGetTarget(out Control? target) || !ReferenceEquals(item.LayoutElement?.Content, target))
        {
            Dispose();
            return;
        }
        if (_awaitingActivation)
        {
            _awaitingActivation = false;
            if (_requestedActive && !item.IsActive)
            {
                Dispose();
                return;
            }
        }
        if (TopLevel.GetTopLevel(target)?.FocusManager is not { } manager)
        {
            if (!item.IsActive) Dispose();
            else if (_focusScope is null && _owner.TryGetTarget(out DockingManager? owner))
            {
                TopLevel? scope = item.LayoutElement?.FindParent<LayoutFloatingWindow>()?.DockWindow.Host as TopLevel ?? TopLevel.GetTopLevel(owner);
                if (scope is not null)
                {
                    _focusScope = new(scope);
                    scope.AddHandler(InputElement.GotFocusEvent, FocusChanged, RoutingStrategies.Bubble, handledEventsToo: true);
                }
            }
            return;
        }
        Dispose();
        if (!target.IsEffectivelyVisible) return;
        IInputElement? focus = target.Focusable ? target : manager.FindNextElement(NavigationDirection.Next,
            new FindNextElementOptions { FocusedElement = target });
        focus?.Focus();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_owner.TryGetTarget(out DockingManager? owner))
        {
            owner.DetachedFromVisualTree -= Detached;
            if (ReferenceEquals(owner.GetValue(PendingProperty), this)) owner.ClearValue(PendingProperty);
        }
        if (_item.TryGetTarget(out LayoutItem? item)) item.PropertyChanged -= ItemChanged;
        if (_content.TryGetTarget(out Control? content))
        {
            content.AttachedToVisualTree -= Attached;
            content.DetachedFromVisualTree -= Detached;
        }
        if (_focusScope?.TryGetTarget(out TopLevel? scope) == true) scope.RemoveHandler(InputElement.GotFocusEvent, FocusChanged);
    }
}
