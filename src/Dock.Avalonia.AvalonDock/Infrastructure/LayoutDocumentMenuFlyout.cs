using Avalonia;
using Avalonia.Controls;

namespace AvalonDock.Infrastructure;

// ChildrenSorted returns a new snapshot after collection changes. Bind that
// source directly so an open native presenter follows snapshot replacement.
internal sealed class LayoutDocumentMenuFlyout : MenuFlyout
{
    protected override Control CreatePresenter()
    {
        Control presenter = base.CreatePresenter();
        presenter.Bind(ItemsControl.ItemsSourceProperty, this.GetObservable(ItemsSourceProperty));
        return presenter;
    }
}
