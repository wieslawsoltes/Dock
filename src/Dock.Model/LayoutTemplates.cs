using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Provides helpers for creating common layouts.
/// </summary>
public static class LayoutTemplates
{
    /// <summary>
    /// Creates a simple layout with a single document dock.
    /// </summary>
    /// <param name="factory">The factory used to create dock elements.</param>
    /// <returns>The root dock.</returns>
    public static IRootDock CreateSingleDocumentLayout(IFactory factory)
    {
        var documents = factory.CreateDocumentDock();
        documents.Id = "Documents";
        documents.Title = "Documents";
        documents.VisibleDockables = factory.CreateList<IDockable>();
        documents.ActiveDockable = null;

        var root = factory.CreateRootDock();
        root.IsCollapsable = false;
        root.VisibleDockables = factory.CreateList<IDockable>(documents);
        root.ActiveDockable = documents;
        root.DefaultDockable = documents;
        return root;
    }

    /// <summary>
    /// Creates a single document layout with an output tool dock at the bottom.
    /// </summary>
    /// <param name="factory">The factory used to create dock elements.</param>
    /// <returns>The root dock.</returns>
    public static IRootDock CreateSingleDocumentWithOutputLayout(IFactory factory)
    {
        var documents = factory.CreateDocumentDock();
        documents.Id = "Documents";
        documents.Title = "Documents";
        documents.VisibleDockables = factory.CreateList<IDockable>();
        documents.ActiveDockable = null;

        var output = factory.CreateToolDock();
        output.Id = "Output";
        output.Title = "Output";
        output.Alignment = Alignment.Bottom;
        output.VisibleDockables = factory.CreateList<IDockable>();
        output.ActiveDockable = null;

        var main = factory.CreateProportionalDock();
        main.Orientation = Orientation.Vertical;
        main.VisibleDockables = factory.CreateList<IDockable>(
            documents,
            factory.CreateProportionalDockSplitter(),
            output);
        main.ActiveDockable = documents;

        var root = factory.CreateRootDock();
        root.IsCollapsable = false;
        root.VisibleDockables = factory.CreateList<IDockable>(main);
        root.ActiveDockable = main;
        root.DefaultDockable = main;
        return root;
    }

    /// <summary>
    /// Creates a layout with two document docks arranged horizontally.
    /// </summary>
    /// <param name="factory">The factory used to create dock elements.</param>
    /// <returns>The root dock.</returns>
    public static IRootDock CreateTwoPaneLayout(IFactory factory)
    {
        var left = factory.CreateDocumentDock();
        left.Id = "Left";
        left.Title = "Left";
        left.VisibleDockables = factory.CreateList<IDockable>();
        left.ActiveDockable = null;

        var right = factory.CreateDocumentDock();
        right.Id = "Right";
        right.Title = "Right";
        right.VisibleDockables = factory.CreateList<IDockable>();
        right.ActiveDockable = null;

        var main = factory.CreateProportionalDock();
        main.Orientation = Orientation.Horizontal;
        main.VisibleDockables = factory.CreateList<IDockable>(
            left,
            factory.CreateProportionalDockSplitter(),
            right);
        main.ActiveDockable = left;

        var root = factory.CreateRootDock();
        root.IsCollapsable = false;
        root.VisibleDockables = factory.CreateList<IDockable>(main);
        root.ActiveDockable = main;
        root.DefaultDockable = main;
        return root;
    }

    /// <summary>
    /// Creates a layout with two windows each containing a single document dock.
    /// </summary>
    /// <param name="factory">The factory used to create dock elements.</param>
    /// <returns>The root dock.</returns>
    public static IRootDock CreateMultiWindowLayout(IFactory factory)
    {
        var window1 = factory.CreateDockWindow();
        window1.Id = "Window1";
        window1.Title = "Window1";
        window1.Layout = CreateSingleDocumentLayout(factory);

        var window2 = factory.CreateDockWindow();
        window2.Id = "Window2";
        window2.Title = "Window2";
        window2.Layout = CreateSingleDocumentLayout(factory);

        var root = factory.CreateRootDock();
        root.IsCollapsable = false;
        root.Windows = factory.CreateList<IDockWindow>(window1, window2);
        return root;
    }
}

