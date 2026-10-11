using System;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>A native Dock host presenting an AvalonDock floating document.</summary>
public class LayoutDocumentFloatingWindowControl : LayoutFloatingWindowControl
{
    private readonly LayoutDocumentFloatingWindow _model;

    internal LayoutDocumentFloatingWindowControl(LayoutDocumentFloatingWindow model, bool isContentImmutable = false)
        : base(model, isContentImmutable) => _model = model;

    /// <inheritdoc/>
    public override ILayoutElement Model => _model;
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutDocumentFloatingWindowControl);
    /// <summary>Gets the existing item container for the currently hosted root document.</summary>
    public LayoutItem? RootDocumentLayoutItem => _model.RootDocument is { } document
        ? _model.Root?.Manager?.GetLayoutItemFromModel(document) : null;

    /// <inheritdoc/>
    protected override bool CanClose(object? parameter = null) =>
        LayoutFloatingWindowOperations.CanExecute(_model, hide: false, parameter);
}
