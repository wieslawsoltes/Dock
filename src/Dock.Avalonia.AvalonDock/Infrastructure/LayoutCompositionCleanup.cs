using Avalonia;
using Avalonia.Rendering.Composition;

namespace AvalonDock.Infrastructure;

// Application editors and cached native targets can outlive their presentation.
// A closed window cannot render another frame to reconcile removed children.
internal static class LayoutCompositionCleanup
{
    internal static void ReleaseChildren(Visual presenter)
    {
        if (ElementComposition.GetElementVisual(presenter) is CompositionContainerVisual visual)
            visual.Children.Clear();
    }
}
