using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Model;
using Dock.Model.Core;
using Dock.Avalonia.Internal;
using Avalonia.Layout;
using System.Linq;
using System;
using Avalonia.Controls.Metadata;

namespace Dock.Avalonia.Controls;

public class PopupHostWindow : Popup, IHostWindow
{
    private readonly DockManager _dockManager;
    private readonly PopupHostWindowState _state;

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(PopupHostWindow);

    public PopupHostWindow()
    {
        _dockManager = new DockManager();
        _state = new PopupHostWindowState(_dockManager, this);

        // Default popup placement near pointer. Offsets are used to control
        // the final position when the popup is shown.
        PlacementMode = PlacementMode.Pointer;

        Opened += (_, _) =>
        {
            if (Window is { })
            {
                Window.Factory?.HostWindows.Add(this);
            }
        };

        Closed += (_, _) =>
        {
            if (Window is { })
            {
                Window.Factory?.HostWindows.Remove(this);
            }
        };
    }

    public IDockManager DockManager => _dockManager;

    public IHostWindowState HostWindowState => _state;

    public bool IsTracked { get; set; }

    public IDockWindow? Window { get; set; }


    public void Present(bool isDialog)
    {
        if (!IsOpen)
        {
            if (Window is { })
            {
                Window.Factory?.OnWindowOpened(Window);
            }

            if (PlacementTarget is null && Window?.Layout?.Factory?.DockControls.FirstOrDefault() is Control control)
            {
                PlacementTarget = control;
            }

            IsOpen = true;
        }
    }

    public void Exit()
    {
        if (IsOpen)
        {
            if (Window is { })
            {
                if (!Window.OnClose())
                {
                    return;
                }
            }

            IsOpen = false;
            Window?.Factory?.OnWindowClosed(Window);
        }
    }

    public void SetPosition(double x, double y)
    {
        HorizontalOffset = x;
        VerticalOffset = y;

        if (IsTracked && Window is { })
        {
            Window.Save();
        }
    }

    public void GetPosition(out double x, out double y)
    {
        x = HorizontalOffset;
        y = VerticalOffset;
    }

    public void SetSize(double width, double height)
    {
        if (Child is Layoutable layout)
        {
            if (!double.IsNaN(width)) layout.Width = width;
            if (!double.IsNaN(height)) layout.Height = height;

            if (IsTracked && Window is { })
            {
                Window.Save();
            }
        }
    }

    public void GetSize(out double width, out double height)
    {
        if (Child is Layoutable layout)
        {
            width = layout.Bounds.Width;
            height = layout.Bounds.Height;
        }
        else
        {
            width = 0;
            height = 0;
        }
    }

    public void SetTitle(string title)
    {
        // Popup does not show a title
    }

    public void SetLayout(IDock layout)
    {
        DataContext = layout;
        Child = new DockControl { Layout = layout };
    }
}
