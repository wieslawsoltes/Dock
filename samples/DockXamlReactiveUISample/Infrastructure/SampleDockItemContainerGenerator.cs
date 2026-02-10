using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace DockXamlReactiveUISample.Infrastructure;

public sealed class SampleDockItemContainerGenerator : DockItemContainerGenerator
{
    public override IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index)
    {
        return new SampleGeneratedDocument
        {
            Id = $"SampleDocument-{index}"
        };
    }

    public override void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index)
    {
        base.PrepareDocumentContainer(dock, container, item, index);
        container.Title = $"Doc {container.Title}";

        if (container is SampleGeneratedDocument generatedDocument)
        {
            generatedDocument.SourceIndex = index;
        }
    }

    public override IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index)
    {
        return new SampleGeneratedTool
        {
            Id = $"SampleTool-{index}"
        };
    }

    public override void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index)
    {
        base.PrepareToolContainer(dock, container, item, index);
        container.Title = $"Tool {container.Title}";

        if (container is SampleGeneratedTool generatedTool)
        {
            generatedTool.SourceIndex = index;
        }
    }
}

public sealed class SampleGeneratedDocument : Document
{
    public int SourceIndex { get; set; } = -1;
}

public sealed class SampleGeneratedTool : Tool
{
    public int SourceIndex { get; set; } = -1;
}
