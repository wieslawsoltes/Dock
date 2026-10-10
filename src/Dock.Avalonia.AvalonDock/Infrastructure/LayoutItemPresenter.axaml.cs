using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;

namespace AvalonDock.Infrastructure;

internal sealed partial class LayoutItemPresenter : ContentPresenter
{
    public LayoutItemPresenter() => AvaloniaXamlLoader.Load(this);
}
