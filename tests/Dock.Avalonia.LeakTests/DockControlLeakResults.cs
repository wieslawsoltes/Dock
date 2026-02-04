using System;

namespace Dock.Avalonia.LeakTests;

internal sealed record SelectorLeakResult(WeakReference ControlRef, WeakReference LayoutRef);

internal sealed record DragLeakResult(WeakReference ControlRef, WeakReference LayoutRef, bool DragStarted);

internal sealed record FloatLeakResult(
    WeakReference ControlRef,
    WeakReference LayoutRef,
    WeakReference? DockWindowRef,
    WeakReference? HostRef,
    WeakReference? HostWindowRef);

internal sealed record ManagedFloatLeakResult(
    WeakReference ControlRef,
    WeakReference LayoutRef,
    WeakReference? DockWindowRef,
    WeakReference? HostRef,
    WeakReference? ManagedDocumentRef);

internal sealed record PinnedWindowLeakResult(
    WeakReference ControlRef,
    WeakReference LayoutRef,
    WeakReference? PinnedWindowRef,
    bool OverlayActive);
