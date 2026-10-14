using Avalonia;
using Avalonia.Markup.Xaml;

namespace DockSystemTextJsonSourceGenAotSample;

internal sealed partial class BindingSmokeApp : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
