using System;
using Avalonia.Controls;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

// Keep Avalonia's menu input and flyout lifetime while using the public
// AvalonDock icon-template contract for generated document entries.
internal sealed class LayoutDocumentMenuPresenter : MenuFlyoutPresenter
{
    private readonly LayoutMenuPresentation _presentation = new();

    protected override Type StyleKeyOverride => typeof(MenuFlyoutPresenter);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new MenuItemEx();

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is MenuItemEx item)
        {
            _presentation.Capture(item);
            _presentation.Release(item);
            item.SetCurrentValue(MenuItem.IconProperty, null);
        }
        base.ClearContainerForItemOverride(container);
    }
}
