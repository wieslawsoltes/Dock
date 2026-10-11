using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvalonDock.Themes;

/// <summary>Contains the compiled default manager, content, floating-window, and navigator resources.</summary>
internal sealed partial class GenericThemeResources : ResourceDictionary
{
    /// <summary>Loads the compiled resources referenced by <see cref="GenericTheme.GetResourceUri"/>.</summary>
    public GenericThemeResources() => AvaloniaXamlLoader.Load(this);
}
