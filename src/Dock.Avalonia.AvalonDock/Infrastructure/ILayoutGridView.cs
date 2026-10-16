using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal interface ILayoutGridView
{
    void ReleaseChildren();
    void SetDragSourcePreview(ILayoutPanelElement? pane);
}
