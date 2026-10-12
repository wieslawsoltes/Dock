using Avalonia;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed partial class LayoutHeaderPresenter
{
    public static readonly DirectProperty<LayoutHeaderPresenter, LayoutItem?> ItemProperty =
        AvaloniaProperty.RegisterDirect<LayoutHeaderPresenter, LayoutItem?>(nameof(Item), view => view.Item, (view, value) => view.Item = value);

    private LayoutItem? _item;
    private LayoutControlModelBinding? _binding;
    public LayoutItem? Item { get => _item; set => SetAndRaise(ItemProperty, ref _item, value); }
    internal LayoutHeaderKind Kind { get; set; }
    internal IDataTemplate? DefaultTemplate { get; set; }
    internal AvaloniaObject? SelectionContainer { get; set; }

    internal void WaitForItem(LayoutContent model)
    {
        _binding = new(this, item =>
        {
            LayoutItem? previous = Item;
            Item = item;
            _binding?.WatchItem(item);
            if (item is not null) item.AttachHeader(this);
            else
            {
                ContentTemplate = null;
                if (previous is not null)
                {
                    DefaultTemplate = null;
                    SelectionContainer = null;
                    DataTemplates.Clear();
                    UpdateChild();
                    LayoutItemViewTransfer.Prepare(this, null);
                }
            }
        }, () => _binding?.SetModel(null)) { IsNativePresentation = true };
        _binding.SetModel(model);
    }
}
