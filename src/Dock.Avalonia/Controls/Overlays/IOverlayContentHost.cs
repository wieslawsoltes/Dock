using Avalonia.Controls.Templates;

namespace Dock.Avalonia.Controls.Overlays;

internal interface IOverlayContentHost
{
    object? Content { get; set; }

    IDataTemplate? ContentTemplate { get; set; }
}
