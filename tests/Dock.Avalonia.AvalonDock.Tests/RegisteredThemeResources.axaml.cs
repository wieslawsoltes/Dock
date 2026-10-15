using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class RegisteredThemeResources : ResourceDictionary
{
    public RegisteredThemeResources() => AvaloniaXamlLoader.Load(this);
}
