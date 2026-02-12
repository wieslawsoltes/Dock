using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace BrowserTabTheme.Themes;

public partial class BrowserTabTheme : Styles
{
    public BrowserTabTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
