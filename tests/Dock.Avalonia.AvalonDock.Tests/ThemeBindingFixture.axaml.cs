using Avalonia;
using Avalonia.Controls;
using AvalonDock;
using AvalonDock.Themes;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class ThemeBindingFixture : Window
{
    public ThemeBindingFixture() => InitializeComponent();
}

public partial class ThemeBindingViewModel : ReactiveObject
{
    [Reactive] private Theme? _theme;
}

public class ThemeNotificationsManager : DockingManager
{
    public int Changes { get; private set; }
    protected override void OnThemeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Changes++;
        base.OnThemeChanged(e);
    }
}
