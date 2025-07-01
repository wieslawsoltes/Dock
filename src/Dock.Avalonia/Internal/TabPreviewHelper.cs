using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class TabPreviewHelper
{
    public TabPreviewControl? Preview { get; private set; }
    private Visual? _host;

    public void AddAdorner(Visual visual)
    {
        if (_host == visual)
            return;

        RemoveAdorner();

        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is null)
            return;

        Preview = new TabPreviewControl
        {
            [AdornerLayer.AdornedElementProperty] = visual,
            IsHitTestVisible = false
        };
        Panel.SetZIndex(Preview, int.MaxValue);

        ((ISetLogicalParent)Preview).SetParent(visual as ILogical);
        layer.Children.Add(Preview);
        _host = visual;
    }

    public void RemoveAdorner()
    {
        if (_host is { } host)
        {
            var layer = AdornerLayer.GetAdornerLayer(host);
            if (layer is { } && Preview is { })
            {
                layer.Children.Remove(Preview);
                ((ISetLogicalParent)Preview).SetParent(null);
            }
        }

        Preview = null;
        _host = null;
    }

    public void Move(Rect rect, string title)
    {
        if (Preview is null)
            return;

        Preview.Title = title;
        Preview.Move(rect);
    }
}
