using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace DockPerspectivesSample;

public class DockFactory : Factory
{
    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            [nameof(IRootDock)] = () => layout,
            [nameof(IProportionalDock)] = () => layout,
            [nameof(IProportionalDockSplitter)] = () => layout,
            [nameof(IDocumentDock)] = () => layout,
            [nameof(IToolDock)] = () => layout,
            [nameof(IDockWindow)] = () => layout,
            [nameof(IDocument)] = () => layout,
            [nameof(ITool)] = () => layout,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>();

        base.InitLayout(layout);
    }
}
