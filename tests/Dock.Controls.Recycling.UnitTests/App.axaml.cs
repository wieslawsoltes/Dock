using Avalonia;
using Avalonia.Markup.Xaml;

namespace Dock.Controls.Recycling.UnitTests;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
