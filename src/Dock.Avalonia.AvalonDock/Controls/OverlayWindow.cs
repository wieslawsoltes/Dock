using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using AvalonDock.Infrastructure;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;

namespace AvalonDock.Controls;

/// <summary>Hosts Dock's native floating selectors and indicators for an AvalonDock layout.</summary>
/// <remarks>Created by Dock when floating docking adorners are enabled. Dock continues to own
/// hit testing, capabilities, drag capture, drop validation, and layout mutations.</remarks>
[TemplatePart("PART_DropTargetsContainer", typeof(Canvas))]
[TemplatePart("PART_DockingManagerDropTargets", typeof(Grid))]
[TemplatePart("PART_AnchorablePaneDropTargets", typeof(Grid))]
[TemplatePart("PART_DocumentPaneDropTargets", typeof(Grid))]
[TemplatePart("PART_DocumentPaneFullDropTargets", typeof(Grid))]
[TemplatePart("PART_PreviewBox", typeof(Path))]
public class OverlayWindow : DockAdornerWindow
{
    private LayoutOverlayProjection? _projection;
    private LayoutOverlayPresentation? _presentation;
    private INameScope? _templateScope;
    private ContentPresenter? _nativePresenter;
    private readonly AvalonDockTheme _defaultTheme = new();

    /// <summary>Creates a native overlay for a manager, docking surface, and adorner.</summary>
    /// <remarks>This Avalonia constructor also supports subclasses supplied by a native adorner-window factory.</remarks>
    protected internal OverlayWindow(DockingManager manager, Control surface, Control adorner)
    {
        _projection = new LayoutOverlayProjection(surface, adorner);
        Styles.Add(_defaultTheme);
        manager.AttachThemeResources(this);
    }

    /// <summary>Gets a new screen-coordinate snapshot of the native docking surface, or null after closing.</summary>
    /// <remarks>This Avalonia projection is additional to the upstream API. Coordinates are physical screen pixels.</remarks>
    public IDropArea? DropArea { get { VerifyAccess(); return _projection?.CaptureArea(); } }

    /// <summary>Gets a new typed overlay-area snapshot, or null after closing.</summary>
    public OverlayArea? Area { get { VerifyAccess(); return _projection?.CaptureOverlayArea(); } }

    /// <summary>Gets snapshots of the native selectors that are currently visible and arranged.</summary>
    /// <remarks>Read again after native target validation or layout changes. This does not change native docking policy.</remarks>
    public IReadOnlyList<OverlayWindowDropTarget> DropTargets
    { get { VerifyAccess(); return _projection?.CaptureTargets() ?? Array.Empty<OverlayWindowDropTarget>(); } }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(OverlayWindow);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _templateScope = e.NameScope;
        _nativePresenter = e.NameScope.Find<ContentPresenter>("PART_NativeTargetPresenter")
            ?? e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        OnApplyTemplate();
    }

    /// <summary>Connects named overlay parts, or the hosted native target template, to Dock's hit testing.</summary>
    public virtual void OnApplyTemplate()
    {
        _presentation?.Dispose();
        _presentation = null;
        if (Content is TemplatedControl target) target.ApplyTemplate();
        if (_projection is not null && _templateScope is not null)
            _presentation = LayoutOverlayPresentation.Create(this, _projection, _templateScope);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TemplateProperty || change.Property == ContentProperty)
        {
            DetachNativeContent();
            _presentation?.Dispose();
            _presentation = null;
            if (change.Property == TemplateProperty) { _templateScope = null; _nativePresenter = null; }
        }
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty && _templateScope is not null && _projection is not null &&
            ReferenceEquals(Content, _projection.Adorner)) OnApplyTemplate();
    }

    /// <summary>Dispatches a native close request to the AvalonDock cancellation hook.</summary>
    protected override void OnClosing(WindowClosingEventArgs e) => OnClosing((CancelEventArgs)e);

    /// <summary>Preserves overload dispatch from derived Avalonia closing overrides.</summary>
    /// <typeparam name="TEventArgs">The native window-closing argument type.</typeparam>
    /// <param name="e">The arguments supplied by Avalonia.</param>
    protected void OnClosing<TEventArgs>(TEventArgs e) where TEventArgs : WindowClosingEventArgs =>
        OnClosing((CancelEventArgs)e);

    /// <summary>Processes an overlay close request using AvalonDock's cancellation signature.</summary>
    /// <remarks>Calling base raises Avalonia's Closing event with the same native arguments.
    /// A canceled close preserves the content, native selectors, and current projections.</remarks>
    protected virtual void OnClosing(CancelEventArgs e)
    {
        if (e is WindowClosingEventArgs nativeArgs) base.OnClosing(nativeArgs);
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        DetachNativeContent();
        _presentation?.Dispose();
        _presentation = null;
        _templateScope = null;
        _nativePresenter = null;
        _projection = null;
        Content = null;
        // Dock caches its target after this window closes. Its implicit theme
        // can outlive attachment, so release the theme's owner link as well.
        Styles.Remove(_defaultTheme);
        base.OnClosed(e);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_nativePresenter is not null) LayoutCompositionCleanup.ReleaseChildren(_nativePresenter);
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachNativeContent()
    {
        // A retired template may assign local Content instead of a template
        // binding. Remove that visual parent before Dock reuses its target.
        if (_nativePresenter is { } presenter && _projection is { } projection &&
            ReferenceEquals(presenter.Content, projection.Adorner))
        {
            LayoutCompositionCleanup.ReleaseChildren(presenter);
            presenter.SetCurrentValue(ContentPresenter.ContentProperty, null);
            presenter.UpdateChild();
        }
    }
}
