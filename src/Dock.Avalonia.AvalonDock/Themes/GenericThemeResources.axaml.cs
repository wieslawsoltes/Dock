using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvalonDock.Themes;

internal sealed partial class GenericThemeResources : ResourceDictionary
{
    public GenericThemeResources() => AvaloniaXamlLoader.Load(this);
}
