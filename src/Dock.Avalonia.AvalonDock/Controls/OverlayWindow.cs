using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvalonDock.Infrastructure;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;

namespace AvalonDock.Controls;

/// <summary>Hosts Dock's native floating selectors and indicators for an AvalonDock layout.</summary>
/// <remarks>Created by Dock when floating docking adorners are enabled. Dock continues to own
/// hit testing, capabilities, drag capture, drop validation, and layout mutations.</remarks>
public class OverlayWindow : DockAdornerWindow
{
    private LayoutOverlayProjection? _projection;

    internal OverlayWindow(DockingManager manager, Control surface, Control adorner)
    {
        _projection = new LayoutOverlayProjection(surface, adorner);
        Styles.Add(new AvalonDockTheme());
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
        OnApplyTemplate();
    }

    /// <summary>Applies the hosted native target's template so its selectors participate in Dock's hit testing.</summary>
    public virtual void OnApplyTemplate()
    {
        if (Content is TemplatedControl target) target.ApplyTemplate();
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        _projection = null;
        Content = null;
        base.OnClosed(e);
    }
}
