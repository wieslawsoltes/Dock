using Prism.Commands;

namespace DockPrismSample.ViewModels;

public static class DockCommands
{
    public static CompositeCommand SaveAll { get; } = new CompositeCommand(true);
}
