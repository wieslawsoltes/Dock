using Avalonia;
using Avalonia.Markup.Xaml;

namespace Dock.Model.Avalonia.UnitTests;

public partial class App : Application
{
    public override void Initialize()
    {
#if DOCK_USE_GENERATED_APP_INITIALIZE_COMPONENT
        InitializeComponent();
#else
        AvaloniaXamlLoader.Load(this);
#endif
    }
}
