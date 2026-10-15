using System;
using AvalonDock.Layout;

namespace PackageConsumer;

internal sealed class LifecycleDocument : LayoutDocument
{
    internal int BeforeChanges { get; private set; }
    internal int AfterChanges { get; private set; }
    internal int RootChanges { get; private set; }

    internal void ResetNotifications() => BeforeChanges = AfterChanges = RootChanges = 0;

    protected override void OnParentChanging(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        if (!ReferenceEquals(Parent, oldValue)) throw new Exception("The changing hook did not observe the previous parent.");
        BeforeChanges++;
        base.OnParentChanging(oldValue, newValue);
    }

    protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        if (!ReferenceEquals(Parent, newValue)) throw new Exception("The changed hook did not observe the new parent.");
        AfterChanges++;
        base.OnParentChanged(oldValue, newValue);
    }

    protected override void OnRootChanged(ILayoutRoot? oldRoot, ILayoutRoot? newRoot)
    {
        if (!ReferenceEquals(Root, newRoot)) throw new Exception("The root hook did not observe the new layout.");
        RootChanges++;
        base.OnRootChanged(oldRoot, newRoot);
    }
}
