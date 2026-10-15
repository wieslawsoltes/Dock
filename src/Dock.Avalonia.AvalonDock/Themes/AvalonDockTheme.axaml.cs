using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace AvalonDock.Themes;

/// <summary>Provides the manager template and Dock's Fluent docking theme.</summary>
public class AvalonDockTheme : Styles
{
    /// <summary>Loads the AvalonDock adapter theme.</summary>
    public AvalonDockTheme() => AvaloniaXamlLoader.Load(this);
}
