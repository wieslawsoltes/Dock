using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIManagedSample.Views;

[RequiresUnreferencedCode("Requires unreferenced code for ThemeManager.")]
[RequiresDynamicCode("Requires unreferenced code for ThemeManager.")]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        InitializeThemes();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeThemes()
    {
        var dark = false;
        var theme = this.Find<Button>("ThemeButton");
        if (theme is { })
        {
            theme.Click += (_, _) =>
            {
                dark = !dark;
                App.ThemeManager?.Switch(dark ? 1 : 0);
            };
        }
    }
}
