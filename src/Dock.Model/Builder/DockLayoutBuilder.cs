using System.Linq;
using Dock.Model.Core;
using Dock.Model.Controls;

namespace Dock.Model.Builder;

/// <summary>
/// Provides a fluent API to build simple Dock layouts.
/// </summary>
public class DockLayoutBuilder
{
    private readonly IFactory _factory;
    private readonly IRootDock _root;
    private readonly IProportionalDock _layout;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockLayoutBuilder"/> class.
    /// </summary>
    /// <param name="factory">Factory used to create dockables.</param>
    public DockLayoutBuilder(IFactory factory)
    {
        _factory = factory;
        _root = _factory.CreateRootDock();
        _layout = _factory.CreateProportionalDock();
        _layout.VisibleDockables = _factory.CreateList<IDockable>();
        _root.VisibleDockables = _factory.CreateList<IDockable>(_layout);
        _root.DefaultDockable = _layout;
    }

    private void AddDockable(IDockable dockable)
    {
        if (_layout.VisibleDockables is null)
        {
            _layout.VisibleDockables = _factory.CreateList<IDockable>();
        }

        if (_layout.VisibleDockables.Count > 0)
        {
            _layout.VisibleDockables.Add(_factory.CreateProportionalDockSplitter());
        }

        _layout.VisibleDockables.Add(dockable);
    }

    /// <summary>
    /// Sets horizontal split orientation.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public DockLayoutBuilder SplitHorizontally()
    {
        _layout.Orientation = Orientation.Horizontal;
        return this;
    }

    /// <summary>
    /// Sets vertical split orientation.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public DockLayoutBuilder SplitVertically()
    {
        _layout.Orientation = Orientation.Vertical;
        return this;
    }

    /// <summary>
    /// Adds a document dock containing the specified document.
    /// </summary>
    /// <param name="document">The document to add.</param>
    /// <returns>The builder instance.</returns>
    public DockLayoutBuilder WithDocument(IDockable document)
    {
        var docDock = _factory.CreateDocumentDock();
        docDock.VisibleDockables = _factory.CreateList<IDockable>(document);
        docDock.ActiveDockable = document;
        AddDockable(docDock);
        return this;
    }

    /// <summary>
    /// Adds a tool dock containing the specified tool.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    /// <param name="alignment">The tool dock alignment.</param>
    /// <returns>The builder instance.</returns>
    public DockLayoutBuilder WithTool(IDockable tool, Alignment alignment)
    {
        var toolDock = _factory.CreateToolDock();
        toolDock.Alignment = alignment;
        toolDock.VisibleDockables = _factory.CreateList<IDockable>(tool);
        toolDock.ActiveDockable = tool;
        AddDockable(toolDock);
        return this;
    }

    /// <summary>
    /// Finalizes the layout.
    /// </summary>
    /// <returns>The root dock.</returns>
    public IRootDock Build()
    {
        _factory.InitLayout(_root);
        return _root;
    }
}

