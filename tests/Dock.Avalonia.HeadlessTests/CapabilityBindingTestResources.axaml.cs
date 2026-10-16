using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.HeadlessTests;

public partial class CapabilityBindingTestResources : ResourceDictionary
{
    public CapabilityBindingTestResources() => AvaloniaXamlLoader.Load(this);
}
