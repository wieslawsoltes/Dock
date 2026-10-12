using System;
using System.Collections.Generic;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// One public layout spans the primary native root and every floating root.
// Keep its activation history without extending the lifetime of removed content.
internal sealed class LayoutActiveContent
{
    private readonly LayoutRoot _root;
    private readonly Action<string> _changing;
    private readonly Action<string> _changed;
    private WeakReference<LayoutContent>? _active;
    private WeakReference<LayoutContent>? _lastDocument;
    private WeakReference<IRootDock>? _nativeRoot;
    private int _revision;
    private int _focusDepth;
    private int _projectionDepth;
    private List<(LayoutContent Content, bool Previous)>? _pendingActivation;

    internal LayoutActiveContent(LayoutRoot root, Action<string> changing, Action<string> changed)
    {
        _root = root;
        _changing = changing;
        _changed = changed;
        if (root.DockFactory is LayoutFactory factory)
        {
            factory.SettingFocus += BeginFocus;
            factory.FocusSet += EndFocus;
        }
        root.DockFactory.FocusedDockableChanged += FocusedDockableChanged;
        NativeFocusChanged((RootDock)root.DockModel);
    }

    internal LayoutContent? Active => Target(_active);
    internal LayoutContent? LastDocument => Target(_lastDocument);

    internal void Activate(LayoutContent? content)
    {
        if (ReferenceEquals(Active, content)) return;
        LayoutContent? previous = Active;
        IRootDock? previousRoot = Target(_nativeRoot);
        IRootDock nativeRoot = content is not null
            ? _root.DockFactory.FindRoot(content.DockModel, static root => root.IsFocusableRoot) ?? (RootDock)_root.DockModel
            : previousRoot ?? (RootDock)_root.DockModel;
        _revision++;
        _changing(nameof(LayoutRoot.ActiveContent));
        if (previous is not null) previous.IsActive = false;
        if (content is not null && !ReferenceEquals(previousRoot, nativeRoot) && previous?.DockModel.Owner is IDock { IsActive: true } owner)
        {
            owner.IsActive = false;
            _root.DockFactory.OnDockableDeactivated(previous.DockModel);
        }
        SetTarget(ref _active, content);
        SetTarget(ref _nativeRoot, nativeRoot);
        if (content is { IsActive: false }) content.IsActive = true;
        else ProjectNativeFocus(content);
        _changed(nameof(LayoutRoot.ActiveContent));
        if (content is null or LayoutDocument || content.Parent is LayoutDocumentPane) SetLastDocument(content);
    }

    internal void ProjectNativeFocus(LayoutContent? content)
    {
        IRootDock nativeRoot = content is not null
            ? _root.DockFactory.FindRoot(content.DockModel, static root => root.IsFocusableRoot) ?? (RootDock)_root.DockModel
            : Target(_nativeRoot) ?? (RootDock)_root.DockModel;
        if (ReferenceEquals(nativeRoot.FocusedDockable, content?.DockModel) &&
            (content?.DockModel.Owner is not IDock owner || owner.IsActive)) return;
        _projectionDepth++;
        try { _root.DockFactory.SetFocusedDockable(nativeRoot, content?.DockModel); }
        finally { _projectionDepth--; }
    }

    internal void NativeFocusChanged(IRootDock nativeRoot)
    {
        if (_focusDepth != 0 || _projectionDepth != 0 || (!ReferenceEquals(nativeRoot, _root.DockModel) &&
            !ReferenceEquals(nativeRoot.Window?.Owner, _root.DockModel))) return;
        if (nativeRoot.FocusedDockable is Document or Tool)
            ContentActivated((LayoutContent)_root.Context.Get(nativeRoot.FocusedDockable));
        else if (ReferenceEquals(Target(_nativeRoot), nativeRoot))
            Update(null, nativeRoot);
    }

    private void ContentActivated(LayoutContent content)
    {
        if (_focusDepth == 0 && ReferenceEquals(content.Root, _root))
            Update(content, _root.DockFactory.FindRoot(content.DockModel, static root => root.IsFocusableRoot));
    }

    internal void Validate()
    {
        if (Active is { } active && !ReferenceEquals(active.Root, _root)) Activate(null);
        if (LastDocument is { } document && !ReferenceEquals(document.Root, _root)) SetLastDocument(null);
    }

    private void FocusedDockableChanged(object? sender, FocusedDockableChangedEventArgs args)
    {
        // Null factory notifications have no source root; the native model's
        // property notification handles clearing without disturbing other roots.
        if (args.Dockable is Document or Tool && args.RootDock is { } nativeRoot)
            NativeFocusChanged(nativeRoot);
    }

    private void BeginFocus() => _focusDepth++;

    internal bool DeferActivationNotification(LayoutContent content, bool previous)
    {
        if (_focusDepth == 0) return false;
        _pendingActivation ??= new();
        foreach ((LayoutContent pending, _) in _pendingActivation)
            if (ReferenceEquals(pending, content)) return true;
        _pendingActivation.Add((content, previous));
        return true;
    }

    private void EndFocus(IDock dock)
    {
        if (--_focusDepth != 0) return;
        try
        {
            if (_root.DockFactory.FindRoot(dock, static root => root.IsFocusableRoot) is { } nativeRoot)
                NativeFocusChanged(nativeRoot);
        }
        finally
        {
            // Clear ownership before callbacks, including callbacks that start
            // another focus operation or throw. No pending flag remains on content.
            List<(LayoutContent Content, bool Previous)>? pending = _pendingActivation;
            _pendingActivation = null;
            if (pending is not null)
                foreach ((LayoutContent content, bool previous) in pending)
                    content.CompleteActivationNotification(previous);
        }
    }

    private void Update(LayoutContent? content, IRootDock? nativeRoot)
    {
        if (!ReferenceEquals(Active, content))
        {
            int revision = ++_revision;
            _changing(nameof(LayoutRoot.ActiveContent));
            if (revision != _revision) return;
            if (content is not null && !ReferenceEquals(Target(_nativeRoot), nativeRoot) &&
                Active is { DockModel.Owner: IDock owner } previous && (owner.IsActive || previous.IsActive))
            {
                // Native cross-root deactivation discovers roots through loaded
                // DockControls. The model-only API must also deactivate a pane
                // whose view has not been realized yet, retaining its focus history.
                owner.IsActive = false;
                _root.DockFactory.OnDockableDeactivated(previous.DockModel);
                if (revision != _revision) return;
            }
            SetTarget(ref _active, content);
            SetTarget(ref _nativeRoot, nativeRoot);
            _changed(nameof(LayoutRoot.ActiveContent));
            // The root records the content from this transition after observers
            // run, even when a manager callback has redirected active content.
        }
        else SetTarget(ref _nativeRoot, nativeRoot);
        if (content is null or LayoutDocument || content.Parent is LayoutDocumentPane)
            SetLastDocument(content);
    }

    private void SetLastDocument(LayoutContent? content)
    {
        LayoutContent? previous = LastDocument;
        if (ReferenceEquals(previous, content)) return;
        _changing(nameof(LayoutRoot.LastFocusedDocument));
        previous?.SetLastFocusedDocument(false);
        SetTarget(ref _lastDocument, content);
        // Content notifications surround each flag independently. Reentrant
        // activation may change the root history during the new flag's callback.
        content?.SetLastFocusedDocument(true);
        _changed(nameof(LayoutRoot.LastFocusedDocument));
    }

    private static T? Target<T>(WeakReference<T>? reference) where T : class =>
        reference is not null && reference.TryGetTarget(out T? value) ? value : null;

    private static void SetTarget<T>(ref WeakReference<T>? reference, T? value) where T : class
    {
        if (reference is not null) reference.SetTarget(value!);
        else if (value is not null) reference = new(value);
    }
}
