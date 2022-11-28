using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvaloniaDemo;

public class DockFactory : Factory
{
    private readonly object _context;

    public DockFactory(object context)
    {
        _context = context;
    }

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            [nameof(IRootDock)] = () => _context,
            [nameof(IProportionalDock)] = () => _context,
            [nameof(IProportionalDockSplitter)] = () => _context,
            [nameof(IDocumentDock)] = () => _context,
            [nameof(IToolDock)] = () => _context,
            [nameof(IDockWindow)] = () => _context,
            [nameof(IDocument)] = () => _context,
            [nameof(ITool)] = () => _context,
            ["LeftPane"] = () => _context,
            ["LeftPaneTop"] = () => _context,
            ["LeftPaneTopSplitter"] = () => _context,
            ["LeftPaneBottom"] = () => _context,
            ["RightPane"] = () => _context,
            ["RightPaneTop"] = () => _context,
            ["RightPaneTopSplitter"] = () => _context,
            ["RightPaneBottom"] = () => _context,
            ["DocumentsPane"] = () => _context,
            ["MainLayout"] = () => _context,
            ["LeftSplitter"] = () => _context,
            ["RightSplitter"] = () => _context,
            ["MainLayout"] = () => _context,
            ["Dashboard"] = () => layout,
            ["Home"] = () => _context
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>();

        base.InitLayout(layout);
    }
}
