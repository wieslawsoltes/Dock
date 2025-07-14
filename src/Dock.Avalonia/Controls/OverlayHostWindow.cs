using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Model;
using Dock.Model.Core;
using Dock.Avalonia.Internal;

namespace Dock.Avalonia.Controls;

public class OverlayHostWindow : ContentControl, IHostWindow
{
    private readonly DockManager _dockManager;
    private readonly OverlayHostWindowState _state;
    private PixelPoint _position;

    protected override Type StyleKeyOverride => typeof(OverlayHostWindow);

    public OverlayHostWindow()
    {
        _dockManager = new DockManager();
        _state = new OverlayHostWindowState(_dockManager, this);
        IsVisible = false;
    }

    public IDockManager DockManager => _dockManager;

    public IHostWindowState HostWindowState => _state;

    public bool IsTracked { get; set; }

    public IDockWindow? Window { get; set; }

    public void Present(bool isDialog)
    {
        if (!IsVisible)
        {
            Window?.Factory?.HostWindows.Add(this);
            Window?.Factory?.OnWindowOpened(Window);
            IsVisible = true;
        }
    }

    public void Exit()
    {
        if (IsVisible)
        {
            if (Window is { } && !Window.OnClose())
            {
                return;
            }
            IsVisible = false;
            Window?.Factory?.HostWindows.Remove(this);
            Window?.Factory?.OnWindowClosed(Window);
        }
    }

    public void SetPosition(double x, double y)
    {
        _position = new PixelPoint((int)x, (int)y);
        Canvas.SetLeft(this, x);
        Canvas.SetTop(this, y);
        if (IsTracked && Window is { })
        {
            Window.Save();
        }
    }

    public void GetPosition(out double x, out double y)
    {
        x = _position.X;
        y = _position.Y;
    }

    public void SetSize(double width, double height)
    {
        if (!double.IsNaN(width)) Width = width;
        if (!double.IsNaN(height)) Height = height;
        if (IsTracked && Window is { })
        {
            Window.Save();
        }
    }

    public void GetSize(out double width, out double height)
    {
        width = Bounds.Width;
        height = Bounds.Height;
    }

    public void SetTitle(string title)
    {
        // Title not shown in overlay
    }

    public void SetLayout(IDock layout)
    {
        DataContext = layout;
        Content = new DockControl { Layout = layout };
    }
}
