using System;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Hosts a native floating group that contains multiple documents or mixed document and tool panes.</summary>
public class LayoutGroupFloatingWindowControl : LayoutFloatingWindowControl
{
    private readonly LayoutGroupFloatingWindow _model;

    /// <summary>Creates a native host for a grouped floating layout.</summary>
    protected internal LayoutGroupFloatingWindowControl(LayoutGroupFloatingWindow model, bool isContentImmutable = false)
        : base(model, isContentImmutable) => _model = model;

    /// <inheritdoc/>
    public override ILayoutElement Model => _model;
    /// <inheritdoc/>
    protected override bool CanClose(object? parameter = null) => LayoutFloatingWindowOperations.CanExecute(_model, false, parameter);
    /// <inheritdoc/>
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ObserveLayoutModel();
    }

    /// <summary>Routes native caption activation and system-menu requests through the owning manager.</summary>
    protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        LayoutFloatingCaptionMessages.Process(this, msg, wParam, lParam, ref handled);
        return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
    }
}
