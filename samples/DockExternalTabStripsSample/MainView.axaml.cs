using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace DockExternalTabStripsSample;

public partial class MainView : UserControl
{
    private readonly DockControl _leftDockControl;
    private readonly DockControl _rightDockControl;
    private readonly DocumentTabStrip _leftTabStrip;
    private readonly DocumentTabStrip _rightTabStrip;
    private bool _surfacesRegistered;

    public MainView()
    {
        InitializeComponent();

        _leftDockControl = this.FindControl<DockControl>("LeftDockControl")
            ?? throw new InvalidOperationException("LeftDockControl was not found.");
        _rightDockControl = this.FindControl<DockControl>("RightDockControl")
            ?? throw new InvalidOperationException("RightDockControl was not found.");

        _leftTabStrip = this.FindControl<DocumentTabStrip>("LeftTabStrip")
            ?? throw new InvalidOperationException("LeftTabStrip was not found.");
        _rightTabStrip = this.FindControl<DocumentTabStrip>("RightTabStrip")
            ?? throw new InvalidOperationException("RightTabStrip was not found.");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RegisterExternalSurfaces();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnregisterExternalSurfaces();
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void RegisterExternalSurfaces()
    {
        if (_surfacesRegistered)
        {
            return;
        }

        _leftDockControl.RegisterExternalDockSurface(_leftTabStrip);
        _rightDockControl.RegisterExternalDockSurface(_rightTabStrip);

        _surfacesRegistered = true;
    }

    private void UnregisterExternalSurfaces()
    {
        if (!_surfacesRegistered)
        {
            return;
        }

        _leftDockControl.UnregisterExternalDockSurface(_leftTabStrip);
        _rightDockControl.UnregisterExternalDockSurface(_rightTabStrip);

        _surfacesRegistered = false;
    }
}
