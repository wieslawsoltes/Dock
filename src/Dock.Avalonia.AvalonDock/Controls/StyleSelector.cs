using Avalonia;
using Avalonia.Styling;

namespace AvalonDock.Controls;

/// <summary>Selects an Avalonia control theme for an AvalonDock item container.</summary>
/// <remarks>Provides the AvalonDock counterpart of WPF's style selector contract.</remarks>
public class StyleSelector
{
    /// <summary>Selects a container theme, or returns null for normal Avalonia styling.</summary>
    /// <param name="item">The application content represented by the container.</param>
    /// <param name="container">The document or anchorable item container.</param>
    /// <returns>The selected control theme, or null.</returns>
    public virtual ControlTheme? SelectStyle(object? item, AvaloniaObject container) => null;
}
