using System;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

internal class DockLayoutBuilder
{
    private readonly Factory _factory = new();

    public IRootDock Build(LayoutConfig config)
    {
        var documents = config.Documents
            .Select(d => new Document { Id = d.Id, Title = d.Title })
            .Cast<IDockable>()
            .ToArray();

        var documentDock = _factory.CreateDocumentDock();
        documentDock.Id = "Documents";
        documentDock.VisibleDockables = _factory.CreateList<IDockable>(documents);
        documentDock.ActiveDockable = documents.FirstOrDefault();

        var layout = _factory.CreateProportionalDock();
        layout.Orientation = Enum.TryParse(config.Orientation, true, out Orientation o) ? o : Orientation.Horizontal;
        layout.VisibleDockables = _factory.CreateList<IDockable>();

        foreach (var tool in config.Tools)
        {
            var toolDockable = new Tool { Id = tool.Id, Title = tool.Title };
            var toolDock = _factory.CreateToolDock();
            toolDock.Alignment = Enum.TryParse(tool.Alignment, true, out Alignment a) ? a : Alignment.Left;
            toolDock.Proportion = tool.Proportion;
            toolDock.VisibleDockables = _factory.CreateList<IDockable>(toolDockable);
            toolDock.ActiveDockable = toolDockable;
            layout.VisibleDockables.Add(toolDock);
            layout.VisibleDockables.Add(_factory.CreateProportionalDockSplitter());
        }

        layout.VisibleDockables.Add(documentDock);
        layout.ActiveDockable = documentDock;

        var root = _factory.CreateRootDock();
        root.VisibleDockables = _factory.CreateList<IDockable>(layout);
        root.DefaultDockable = layout;
        root.ActiveDockable = layout;

        _factory.InitLayout(root);
        return root;
    }
}
