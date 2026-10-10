using Avalonia;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

internal sealed partial class LayoutHeaderPresenter
{
    public static readonly DirectProperty<LayoutHeaderPresenter, LayoutItem?> ItemProperty =
        AvaloniaProperty.RegisterDirect<LayoutHeaderPresenter, LayoutItem?>(nameof(Item), view => view.Item, (view, value) => view.Item = value);

    private LayoutItem? _item;
    public LayoutItem? Item { get => _item; set => SetAndRaise(ItemProperty, ref _item, value); }
    internal LayoutHeaderKind Kind { get; set; }
    internal IDataTemplate? DefaultTemplate { get; set; }
}
