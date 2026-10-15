using System;
using System.Collections.Generic;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutNavigatorItems
{
    internal static (LayoutDocumentItem[] Documents, LayoutAnchorableItem[] Anchorables) Capture(DockingManager manager)
    {
        List<(LayoutDocumentItem Item, DateTime Activated, int Order)> documents = new();
        List<LayoutAnchorableItem> tools = new();
        foreach (ILayoutElement element in manager.Layout.Descendents())
        {
            if (element is LayoutDocument { IsEnabled: true } document &&
                manager.GetLayoutItemFromModel(document) is LayoutDocumentItem item)
                documents.Add((item, document.LastActivationTimeStamp.GetValueOrDefault(), documents.Count));
            else if (element is LayoutAnchorable { IsVisible: true, IsEnabled: true } tool &&
                     manager.GetLayoutItemFromModel(tool) is LayoutAnchorableItem container)
                tools.Add(container);
        }
        documents.Sort(static (left, right) =>
        {
            int byTime = right.Activated.CompareTo(left.Activated);
            return byTime != 0 ? byTime : left.Order.CompareTo(right.Order);
        });
        LayoutDocumentItem[] ordered = new LayoutDocumentItem[documents.Count];
        for (int index = 0; index < ordered.Length; index++) ordered[index] = documents[index].Item;
        return (ordered, tools.ToArray());
    }
}
