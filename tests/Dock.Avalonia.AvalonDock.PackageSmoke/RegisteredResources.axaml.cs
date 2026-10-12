using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PackageConsumer;

public partial class RegisteredResources : ResourceDictionary
{
    public RegisteredResources() => AvaloniaXamlLoader.Load(this);
}
