using System;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutNavigatorPresenter
{
    private NavigatorWindow? _window;
    private bool _creating;

    internal bool IsActive => _creating || _window is not null;

    internal void Show(DockingManager manager)
    {
        if (IsActive) return;
        _creating = true;
        NavigatorWindow? window = null;
        try
        {
            window = manager.CreateNavigatorWindow();
            _window = window;
            window.PropertyChanged += WindowPropertyChanged;
            window.Closed += WindowClosed;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (TopLevel.GetTopLevel(manager) is Window { IsVisible: true } owner)
                _ = window.ShowDialog(owner);
            else window.Show();
        }
        catch
        {
            if (window is not null)
            {
                Detach(window);
                window.Close();
            }
            throw;
        }
        finally { _creating = false; }
    }

    internal void Close() => _window?.Close();

    private void WindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == Visual.IsVisibleProperty && change.NewValue is false && sender is NavigatorWindow window)
            Detach(window);
    }

    private void WindowClosed(object? sender, EventArgs args)
    {
        if (sender is NavigatorWindow window) Detach(window);
    }

    private void Detach(NavigatorWindow window)
    {
        window.PropertyChanged -= WindowPropertyChanged;
        window.Closed -= WindowClosed;
        if (ReferenceEquals(_window, window)) _window = null;
    }
}
