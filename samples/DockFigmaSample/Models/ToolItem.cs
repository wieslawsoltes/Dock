namespace DockFigmaSample.Models;

public class ToolItem
{
    public ToolItem(string name, string glyph, bool isActive = false)
    {
        Name = name;
        Glyph = glyph;
        IsActive = isActive;
    }

    public string Name { get; }
    public string Glyph { get; }
    public bool IsActive { get; }
}
