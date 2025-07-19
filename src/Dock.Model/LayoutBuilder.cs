using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Fluent builder for creating dock layouts.
/// </summary>
public class LayoutBuilder
{
    private readonly FactoryBase _factory;
    private readonly Stack<IDock> _stack = new();
    private IRootDock? _root;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutBuilder"/> class.
    /// </summary>
    /// <param name="factory">The factory to use when creating dockables.</param>
    public LayoutBuilder(FactoryBase factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Starts a new root dock and pushes it onto the stack.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public LayoutBuilder AddRootDock()
    {
        _root = _factory.CreateRootDock();
        _stack.Push(_root);
        return this;
    }

    /// <summary>
    /// Begins a new proportional dock.
    /// </summary>
    /// <param name="orientation">The dock orientation.</param>
    /// <returns>The current builder instance.</returns>
    public LayoutBuilder BeginProportionalDock(Orientation orientation)
    {
        var dock = _factory.CreateProportionalDock();
        dock.Orientation = orientation;
        AddDockable(dock);
        _stack.Push(dock);
        return this;
    }

    /// <summary>
    /// Ends the current dock and returns to the parent.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public LayoutBuilder EndDock()
    {
        if (_stack.Count > 1)
        {
            _stack.Pop();
        }
        return this;
    }

    /// <summary>
    /// Adds a new tool dock to the current parent dock.
    /// </summary>
    /// <param name="id">The dock id.</param>
    /// <param name="alignment">The tool alignment.</param>
    /// <param name="tools">The tool dockables.</param>
    /// <returns>The current builder instance.</returns>
    public LayoutBuilder AddToolDock(string id, Alignment alignment, params IDockable[] tools)
    {
        var dock = _factory.CreateToolDock();
        dock.Id = id;
        dock.Alignment = alignment;
        dock.VisibleDockables = _factory.CreateList(tools);
        if (tools.Length > 0)
        {
            dock.ActiveDockable = tools[0];
        }
        AddDockable(dock);
        return this;
    }

    /// <summary>
    /// Adds a new document dock to the current parent dock.
    /// </summary>
    /// <param name="id">The dock id.</param>
    /// <param name="documents">The document dockables.</param>
    /// <returns>The current builder instance.</returns>
    public LayoutBuilder AddDocumentDock(string id, params IDockable[] documents)
    {
        var dock = _factory.CreateDocumentDock();
        dock.Id = id;
        dock.VisibleDockables = _factory.CreateList(documents);
        if (documents.Length > 0)
        {
            dock.ActiveDockable = documents[0];
        }
        AddDockable(dock);
        return this;
    }

    /// <summary>
    /// Adds a proportional dock splitter to the current parent dock.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public LayoutBuilder AddSplitter()
    {
        var splitter = _factory.CreateProportionalDockSplitter();
        AddDockable(splitter);
        return this;
    }

    private void AddDockable(IDockable dockable)
    {
        if (_stack.Count == 0)
        {
            throw new InvalidOperationException("No parent dock on the stack.");
        }

        var parent = _stack.Peek();
        _factory.AddDockable(parent, dockable);
    }

    /// <summary>
    /// Finishes building and initializes the resulting layout.
    /// </summary>
    /// <returns>The root dock instance.</returns>
    public IRootDock Build()
    {
        if (_root is null)
        {
            throw new InvalidOperationException("Root dock was not created.");
        }

        _factory.InitLayout(_root);
        return _root;
    }
}
