using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Floating children retain their public parent after replacement. Only remove
// actual members when preparing a native move: floating RemoveChild operates on
// its single slot even when the caller passes a different or former child.
internal static class LayoutParenting
{
    internal static void RemoveFromParent(ILayoutElement element)
    {
        if (element.Parent is not { } parent) return;
        foreach (ILayoutElement child in parent.Children)
            if (ReferenceEquals(child, element))
            {
                parent.RemoveChild(element);
                return;
            }
    }
}
