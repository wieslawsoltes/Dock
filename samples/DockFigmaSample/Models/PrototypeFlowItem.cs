namespace DockFigmaSample.Models;

public class PrototypeFlowItem
{
    public PrototypeFlowItem(string trigger, string action, string destination)
    {
        Trigger = trigger;
        Action = action;
        Destination = destination;
    }

    public string Trigger { get; }
    public string Action { get; }
    public string Destination { get; }
}
