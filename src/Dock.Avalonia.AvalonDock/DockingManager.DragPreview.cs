using Avalonia;

namespace AvalonDock;

public partial class DockingManager
{
    /// <summary>Identifies whether dragging shows the document or pane's full content.</summary>
    public static readonly StyledProperty<bool> ShowDockablePreviewOnDragProperty =
        AvaloniaProperty.Register<DockingManager, bool>(nameof(ShowDockablePreviewOnDrag), true);

    /// <summary>Gets or sets whether drag previews include the content being moved. The default is true.</summary>
    /// <remarks>This Avalonia extension applies to this manager's docked and floating layouts.
    /// It takes precedence over Dock's global preview setting and is read when each drag begins.</remarks>
    public bool ShowDockablePreviewOnDrag
    {
        get => GetValue(ShowDockablePreviewOnDragProperty);
        set => SetValue(ShowDockablePreviewOnDragProperty, value);
    }
}
