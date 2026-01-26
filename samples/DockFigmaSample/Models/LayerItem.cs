using Avalonia;

namespace DockFigmaSample.Models;

public class LayerItem
{
    public LayerItem(string name, string type, int depth, bool isVisible = true, bool isLocked = false)
    {
        Name = name;
        Type = type;
        IsVisible = isVisible;
        IsLocked = isLocked;
        Indent = new Thickness(8 + depth * 16, 2, 8, 2);
    }

    public string Name { get; }
    public string Type { get; }
    public bool IsVisible { get; }
    public bool IsLocked { get; }
    public Thickness Indent { get; }
}
