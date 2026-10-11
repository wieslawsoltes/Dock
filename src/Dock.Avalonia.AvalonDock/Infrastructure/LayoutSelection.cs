using System;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutSelection
{
    // Before a root supplies its factory, pane selection must still update the
    // native content flags. Once initialized, Dock owns this synchronization.
    internal static void SynchronizeWithoutFactory<T>(LayoutGroup<T> pane, IDockable? selected)
        where T : LayoutContent
    {
        if (pane.DockModel.Factory is not null) return;
        for (int index = 0; index < pane.Children.Count; index++)
        {
            T content = pane.Children[index];
            if (!ReferenceEquals(content.DockModel.Owner, pane.DockModel)) continue;
            content.SetSelectedFromPane(ReferenceEquals(content.DockModel, selected));
        }
    }
}
