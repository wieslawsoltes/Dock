using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;

namespace AvalonDock.Infrastructure;

/// <summary>A themed resize hit area that delegates dragging to the native window.</summary>
public sealed class LayoutWindowResizeGrip : Border
{
    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ||
            TopLevel.GetTopLevel(this) is not Window { CanResize: true, WindowState: WindowState.Normal } window) return;
        WindowEdge? edge = WindowDecorationProperties.GetElementRole(this) switch
        {
            WindowDecorationsElementRole.ResizeW => WindowEdge.West,
            WindowDecorationsElementRole.ResizeE => WindowEdge.East,
            WindowDecorationsElementRole.ResizeN => WindowEdge.North,
            WindowDecorationsElementRole.ResizeS => WindowEdge.South,
            WindowDecorationsElementRole.ResizeNW => WindowEdge.NorthWest,
            WindowDecorationsElementRole.ResizeNE => WindowEdge.NorthEast,
            WindowDecorationsElementRole.ResizeSW => WindowEdge.SouthWest,
            WindowDecorationsElementRole.ResizeSE => WindowEdge.SouthEast,
            _ => null
        };
        if (edge is not { } direction) return;
        window.BeginResizeDrag(direction, e);
        e.Handled = true;
    }
}
