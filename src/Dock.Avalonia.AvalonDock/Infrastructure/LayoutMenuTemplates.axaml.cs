using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutMenuTemplates : ResourceDictionary
{
    public LayoutMenuTemplates() => AvaloniaXamlLoader.Load(this);
}
