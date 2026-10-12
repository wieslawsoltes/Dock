using Avalonia;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

internal sealed partial class LayoutItemPresenter
{
    // ContentPresenter uses DataContext for the presented content. Keep its
    // compiled binding source separate so presenting a ViewModel cannot
    // replace the source container or create a Content/DataContext cycle.
    public static readonly DirectProperty<LayoutItemPresenter, LayoutItem?> ItemProperty =
        AvaloniaProperty.RegisterDirect<LayoutItemPresenter, LayoutItem?>(nameof(Item), view => view.Item, (view, value) => view.Item = value);

    private LayoutItem? _item;
    public LayoutItem? Item { get => _item; set => SetAndRaise(ItemProperty, ref _item, value); }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // The application can retain an editor after its layout is discarded.
        // Detach its composition subtree at our presentation boundary while it
        // is still available: a closing native window will not render another
        // frame to reconcile these children. Avalonia recreates it on reattach.
        LayoutCompositionCleanup.ReleaseChildren(this);
        base.OnDetachedFromVisualTree(e);
    }
}
