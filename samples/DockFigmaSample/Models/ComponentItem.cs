namespace DockFigmaSample.Models;

public class ComponentItem
{
    public ComponentItem(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }
    public string Description { get; }
}
