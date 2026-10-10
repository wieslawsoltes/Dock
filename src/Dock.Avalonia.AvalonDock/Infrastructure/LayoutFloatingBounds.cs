using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Floating geometry is exposed on AvalonDock's contents and groups while Dock
// stores it on the window. Keep those views synchronized without moving a host
// in response to the host's own Save operation.
internal sealed class LayoutFloatingBounds
{
    private readonly LayoutFloatingWindow _owner;
    private readonly DockWindow _window;
    private readonly List<LayoutElement> _elements = new();
    private bool _changing;

    internal LayoutFloatingBounds(LayoutFloatingWindow owner)
    {
        _owner = owner;
        _window = owner.DockWindow;
        _window.PropertyChanged += WindowChanged;
    }

    internal void Refresh()
    {
        foreach (LayoutElement element in _elements) element.PropertyChanged -= ElementChanged;
        _elements.Clear();
        foreach (ILayoutElement element in _owner.Descendents())
            if (element is LayoutElement node and ILayoutElementForFloatingWindow)
            {
                _elements.Add(node);
                node.PropertyChanged += ElementChanged;
            }
        if (_elements.Count == 0) return;
        _changing = true;
        try
        {
            ILayoutElementForFloatingWindow primary = (ILayoutElementForFloatingWindow)_elements[0];
            if (!HasSize(_window.Width) && !HasSize(_window.Height))
            {
                bool maximized = false;
                foreach (LayoutElement node in _elements)
                {
                    ILayoutElementForFloatingWindow candidate = (ILayoutElementForFloatingWindow)node;
                    if (!HasSize(primary.FloatingWidth) && !HasSize(primary.FloatingHeight) &&
                        HasSize(candidate.FloatingWidth) && HasSize(candidate.FloatingHeight)) primary = candidate;
                    maximized |= candidate.IsMaximized;
                }
                if (HasSize(primary.FloatingWidth)) _window.Width = primary.FloatingWidth;
                if (HasSize(primary.FloatingHeight)) _window.Height = primary.FloatingHeight;
                if (double.IsFinite(primary.FloatingLeft)) _window.X = primary.FloatingLeft;
                if (double.IsFinite(primary.FloatingTop)) _window.Y = primary.FloatingTop;
                if (maximized) _window.WindowState = DockWindowState.Maximized;
            }
            CopyFromWindow();
        }
        finally { _changing = false; }
    }

    private void ElementChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (_changing || sender is not ILayoutElementForFloatingWindow element) return;
        _changing = true;
        try
        {
            switch (args.PropertyName)
            {
                case nameof(element.FloatingWidth) when HasSize(element.FloatingWidth):
                    _window.Width = element.FloatingWidth;
                    _window.Host?.SetSize(_window.Width, _window.Height);
                    break;
                case nameof(element.FloatingHeight) when HasSize(element.FloatingHeight):
                    _window.Height = element.FloatingHeight;
                    _window.Host?.SetSize(_window.Width, _window.Height);
                    break;
                case nameof(element.FloatingLeft) when double.IsFinite(element.FloatingLeft):
                    _window.X = element.FloatingLeft;
                    _window.Host?.SetPosition(_window.X, _window.Y);
                    break;
                case nameof(element.FloatingTop) when double.IsFinite(element.FloatingTop):
                    _window.Y = element.FloatingTop;
                    _window.Host?.SetPosition(_window.X, _window.Y);
                    break;
                case nameof(element.IsMaximized):
                    _window.WindowState = element.IsMaximized ? DockWindowState.Maximized : DockWindowState.Normal;
                    _window.Host?.SetWindowState(_window.WindowState);
                    break;
                default: return;
            }
            CopyFromWindow();
        }
        finally { _changing = false; }
    }

    private void WindowChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (_changing || (args.Property != DockWindow.XProperty && args.Property != DockWindow.YProperty &&
            args.Property != DockWindow.WidthProperty && args.Property != DockWindow.HeightProperty &&
            args.Property != DockWindow.WindowStateProperty)) return;
        _changing = true;
        try { CopyFromWindow(); }
        finally { _changing = false; }
    }

    private void CopyFromWindow()
    {
        foreach (LayoutElement node in _elements)
        {
            ILayoutElementForFloatingWindow element = (ILayoutElementForFloatingWindow)node;
            if (HasSize(_window.Width)) element.FloatingWidth = _window.Width;
            if (HasSize(_window.Height)) element.FloatingHeight = _window.Height;
            if (double.IsFinite(_window.X)) element.FloatingLeft = _window.X;
            if (double.IsFinite(_window.Y)) element.FloatingTop = _window.Y;
            element.IsMaximized = _window.WindowState == DockWindowState.Maximized;
        }
    }

    private static bool HasSize(double value) => double.IsFinite(value) && value > 0;
}
