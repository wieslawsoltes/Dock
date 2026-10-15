using System;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class GroupBaseDerivationProbe
{
    internal static void Verify()
    {
        LayoutRoot root = new();
        ApplicationGroup group = new() { Parent = root.RootPanel };
        if (!ReferenceEquals(group.Root, root) || !ReferenceEquals(group.DockModel.Owner, root.RootPanel.DockModel))
            throw new Exception("An external base group lost native parent identity.");
        int collectionCalls = 0;
        int childCalls = 0;
        int parentCalls = 0;
        group.ChildrenCollectionChanged += (_, _) => collectionCalls++;
        group.ChildrenTreeChanged += (_, args) =>
        {
            if (args.Change != ChildrenTreeChange.DirectChildrenChanged) throw new Exception("Wrong child change.");
            childCalls++;
        };
        root.RootPanel.ChildrenTreeChanged += (_, args) =>
        {
            if (args.Change != ChildrenTreeChange.TreeChanged) throw new Exception("Wrong parent change.");
            parentCalls++;
        };
        group.Notify();
        if (collectionCalls != 1 || childCalls != 1 || parentCalls != 1 || group.TreeHooks != 1)
            throw new Exception("An external group did not dispatch its protected notification hooks.");
        group.Parent = null;
        if (group.Root is not null || group.DockModel.Owner is not null)
            throw new Exception("An external group retained its detached native parent.");
    }

    private sealed class ApplicationGroup : LayoutGroupBase
    {
        public int TreeHooks { get; private set; }
        public void Notify()
        {
            OnChildrenCollectionChanged();
            NotifyChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
        }
        protected override void OnChildrenTreeChanged(ChildrenTreeChange change)
        {
            TreeHooks++;
            base.OnChildrenTreeChanged(change);
        }
    }
}
