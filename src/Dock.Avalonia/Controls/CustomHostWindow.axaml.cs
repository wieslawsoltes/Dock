using Avalonia.Controls.Metadata;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Host window with a custom Avalonia title bar.
/// </summary>
public class CustomHostWindow : HostWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomHostWindow"/> class.
    /// </summary>
    public CustomHostWindow()
    {
        ToolChromeControlsWholeWindow = true;
        DocumentChromeControlsWholeWindow = true;
    }
}
