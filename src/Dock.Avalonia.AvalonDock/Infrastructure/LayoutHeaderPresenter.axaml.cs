using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;

namespace AvalonDock.Infrastructure;

internal sealed partial class LayoutHeaderPresenter : ContentPresenter
{
    public LayoutHeaderPresenter() => AvaloniaXamlLoader.Load(this);
}
