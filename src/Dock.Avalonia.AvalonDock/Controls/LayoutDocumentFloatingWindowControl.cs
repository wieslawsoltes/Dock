using System;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>A native Dock host presenting an AvalonDock floating document.</summary>
public class LayoutDocumentFloatingWindowControl : LayoutFloatingWindowControl
{
    private readonly LayoutDocumentFloatingWindow _model;

    /// <summary>Creates a document host, including a host subclass supplied through the native window locator.</summary>
    /// <param name="model">The floating document model.</param>
    /// <param name="isContentImmutable">Whether loaded content is protected from replacement.</param>
    protected internal LayoutDocumentFloatingWindowControl(LayoutDocumentFloatingWindow model, bool isContentImmutable = false)
        : base(model, isContentImmutable) => _model = model;

    /// <inheritdoc/>
    public override ILayoutElement Model => _model;
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutDocumentFloatingWindowControl);
    /// <summary>Gets the existing item container for the currently hosted root document.</summary>
    public LayoutItem? RootDocumentLayoutItem => _model.RootDocument is { } document
        ? _model.Root?.Manager?.GetLayoutItemFromModel(document) : null;

    /// <summary>Starts observing root-document changes after native initialization.</summary>
    /// <remarks>Call the base hook to retain automatic closure when the root document is removed.</remarks>
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ObserveLayoutModel(closeEmptyDocument: true);
    }

    /// <summary>Checks document descendants of the current Model, including a model supplied by an override.</summary>
    protected override bool CanClose(object? parameter = null) =>
        LayoutFloatingWindowOperations.CanExecute<LayoutDocument>(Model, hide: false, parameter);

    /// <summary>Activates a clicked native caption and routes its context menu before native fallback.</summary>
    protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        LayoutFloatingCaptionMessages.Process(this, _model, msg, wParam, lParam, ref handled);
        return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
    }
}
