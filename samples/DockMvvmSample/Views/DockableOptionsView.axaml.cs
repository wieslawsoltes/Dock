using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Model.Core;

namespace DockMvvmSample.Views;

public partial class DockableOptionsView : UserControl
{
    public static object?[] PinnedDockDisplayModeOptions { get; } =
    {
        null,
        PinnedDockDisplayMode.Overlay,
        PinnedDockDisplayMode.Inline
    };

    public DockableOptionsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
