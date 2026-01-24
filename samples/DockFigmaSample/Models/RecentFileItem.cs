using Avalonia.Media;

namespace DockFigmaSample.Models;

public class RecentFileItem
{
    public RecentFileItem(string name, string team, string updated, Color accent)
    {
        Name = name;
        Team = team;
        Updated = updated;
        AccentBrush = new SolidColorBrush(accent);
    }

    public string Name { get; }
    public string Team { get; }
    public string Updated { get; }
    public IBrush AccentBrush { get; }
}
