namespace DockFigmaSample.Models;

public class InspectSpecItem
{
    public InspectSpecItem(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }
    public string Value { get; }
}
