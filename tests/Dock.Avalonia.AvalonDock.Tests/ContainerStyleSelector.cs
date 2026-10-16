using Avalonia;
using Avalonia.Styling;
using AvalonDock.Controls;

namespace Dock.Avalonia.AvalonDock.Tests;

public sealed class ContainerStyleSelector : StyleSelector
{
    public ControlTheme? DocumentStyle { get; set; }
    public ControlTheme? ToolStyle { get; set; }
    public ControlTheme? AlternateStyle { get; set; }
    public object? LastItem { get; private set; }
    public AvaloniaObject? LastContainer { get; private set; }
    public int Calls { get; private set; }

    public override ControlTheme? SelectStyle(object? item, AvaloniaObject container)
    {
        LastItem = item;
        LastContainer = container;
        Calls++;
        if (item is AlternateContent) return AlternateStyle;
        return container is LayoutAnchorableItem ? ToolStyle : DocumentStyle;
    }
}
