using Avalonia;
using Avalonia.Controls.Templates;

namespace AvalonDock.Controls;

/// <summary>Selects an Avalonia data template for AvalonDock application content.</summary>
/// <remarks>Provides the AvalonDock counterpart of WPF's data-template selector contract.</remarks>
public class DataTemplateSelector
{
    /// <summary>Selects a template, or returns null to use normal content-template lookup.</summary>
    /// <param name="item">The application content to display.</param>
    /// <param name="container">The presenter displaying the content.</param>
    /// <returns>The selected Avalonia template, or null for the default presentation.</returns>
    public virtual IDataTemplate? SelectTemplate(object? item, AvaloniaObject container) => null;
}
