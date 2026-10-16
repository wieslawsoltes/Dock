using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Whole-pane/group moves have no per-content move event. Retire empty return
// panes only after the complete outer operation, before reporting completion.
internal sealed class LayoutOuterSplitScope(LayoutRoot root, IDockOperationScope? inner) : IDockOperationScope
{
    public void Complete()
    {
        root.CollectGarbage();
        inner?.Complete();
    }

    public void Dispose() => inner?.Dispose();
}
