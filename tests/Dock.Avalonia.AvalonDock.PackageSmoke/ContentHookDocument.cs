using System;
using System.ComponentModel;
using AvalonDock.Layout;

namespace PackageConsumer;

internal sealed class ContentHookDocument : LayoutDocument
{
    internal int SelectionChanges { get; private set; }
    internal int Activations { get; private set; }
    internal int ClosingRequests { get; private set; }
    internal int CompletedCloses { get; private set; }
    internal bool CancelClose { get; set; }

    protected override void OnIsSelectedChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue || IsSelected != newValue)
            throw new Exception("The external selection hook did not receive the completed transition.");
        SelectionChanges++;
        base.OnIsSelectedChanged(oldValue, newValue);
    }

    protected override void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (!IsSelected || !ReferenceEquals(Root?.ActiveContent, this))
                throw new Exception("The external activation hook ran before selection and root focus completed.");
            Activations++;
        }
        base.OnIsActiveChanged(oldValue, newValue);
    }

    protected override void OnClosing(CancelEventArgs args)
    {
        ClosingRequests++;
        args.Cancel |= CancelClose;
        base.OnClosing(args);
    }

    protected override void OnClosed()
    {
        if (Parent is not null || Root is not null)
            throw new Exception("The external closed hook ran before detaching its content.");
        CompletedCloses++;
        base.OnClosed();
    }
}
