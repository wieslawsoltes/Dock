using AvalonDock.Layout;

namespace PackageConsumer;

internal sealed class VisibilityPane : LayoutAnchorablePane
{
    internal int VisibilityChanges { get; private set; }

    internal void SetVisibility(bool visible) => IsVisible = visible;

    protected override void OnIsVisibleChanged()
    {
        VisibilityChanges++;
        base.OnIsVisibleChanged();
    }
}
