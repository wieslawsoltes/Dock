using Avalonia;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.INPC
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
