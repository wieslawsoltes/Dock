using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace DockSimplifiedSample;

/// <summary>
/// Creates the observable docking layout used by the simplified sample.
/// </summary>
public sealed class DockFactory : Factory
{
    /// <inheritdoc />
    public override IRootDock CreateLayout()
    {
        var document = new Document { Id = "Document1", Title = "Document 1" };
        var tool = new Tool { Id = "Tool1", Title = "Tool 1" };

        var documentDock = new DocumentDock
        {
            Id = "Documents",
            Title = "Documents",
            VisibleDockables = CreateList<IDockable>(document),
            ActiveDockable = document,
            Proportion = 0.75
        };

        var toolDock = new ToolDock
        {
            Id = "Tools",
            Title = "Tools",
            VisibleDockables = CreateList<IDockable>(tool),
            ActiveDockable = tool,
            Alignment = Alignment.Left,
            Proportion = 0.25
        };

        var mainLayout = new ProportionalDock
        {
            Id = "MainLayout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                toolDock,
                new ProportionalDockSplitter { CanResize = true },
                documentDock)
        };

        return new RootDock
        {
            Id = "Root",
            Title = "Root",
            VisibleDockables = CreateList<IDockable>(mainLayout),
            ActiveDockable = mainLayout
        };
    }
}
