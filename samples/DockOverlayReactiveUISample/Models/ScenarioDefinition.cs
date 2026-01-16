namespace DockOverlayReactiveUISample.Models;

public sealed class ScenarioDefinition
{
    public ScenarioDefinition(Scenario scenario, string title, string description)
    {
        Scenario = scenario;
        Title = title;
        Description = description;
    }

    public Scenario Scenario { get; }

    public string Title { get; }

    public string Description { get; }

    public override string ToString() => Title;
}
