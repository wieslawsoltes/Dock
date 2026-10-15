using System;
using Avalonia.Controls;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

// Avalonia removes Window.Owner on Hide. Keep the owner's closing lifetime
// independently so document selection can hide a navigator until its owner closes.
internal sealed class LayoutNavigatorOwner(NavigatorWindow window) : IDisposable
{
    private readonly WeakReference<NavigatorWindow> _window = new(window);
    private Window? _owner;

    internal void Attach(Window? owner)
    {
        if (ReferenceEquals(_owner, owner)) return;
        Dispose();
        _owner = owner;
        if (_owner is not null) _owner.Closed += OwnerClosed;
    }

    private void OwnerClosed(object? sender, EventArgs args)
    {
        Dispose();
        if (_window.TryGetTarget(out NavigatorWindow? window)) window.CloseWithOwner();
    }

    public void Dispose()
    {
        if (_owner is not null) _owner.Closed -= OwnerClosed;
        _owner = null;
    }
}
