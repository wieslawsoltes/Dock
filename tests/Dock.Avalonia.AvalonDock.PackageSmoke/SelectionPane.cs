using AvalonDock.Layout;

namespace PackageConsumer;

internal sealed class SelectionPane : LayoutDocumentPane
{
    internal int MoveCount { get; private set; }
    internal (int OldIndex, int NewIndex, int SelectedIndex) LastMove { get; private set; }

    protected override void ChildMoved(int oldIndex, int newIndex)
    {
        MoveCount++;
        LastMove = (oldIndex, newIndex, SelectedContentIndex);
        base.ChildMoved(oldIndex, newIndex);
    }
}
