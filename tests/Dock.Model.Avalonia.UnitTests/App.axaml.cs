using Avalonia;
using Avalonia.Markup.Xaml;

namespace Dock.Model.Avalonia.UnitTests;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
