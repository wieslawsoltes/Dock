using Avalonia.Media;

namespace DockFigmaSample.Models;

public class AssetSwatch
{
    public AssetSwatch(string name, Color color)
    {
        Name = name;
        Brush = new SolidColorBrush(color);
        Hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public string Name { get; }
    public IBrush Brush { get; }
    public string Hex { get; }
}
