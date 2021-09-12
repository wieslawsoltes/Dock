using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvaloniaDemo
{
    public class DemoFactory : Factory
    {
        public override IRootDock CreateLayout()
        {
            return new RootDock();
        }

        public override void InitLayout(IDockable layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => layout,
                [nameof(IProportionalDock)] = () => layout,
                [nameof(IDocumentDock)] = () => layout,
                [nameof(IToolDock)] = () => layout,
                [nameof(ISplitterDockable)] = () => layout,
                [nameof(IDockWindow)] = () => layout,
                [nameof(IDocument)] = () => layout,
                [nameof(ITool)] = () => layout,
            };

            this.HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            this.DockableLocator = new Dictionary<string, Func<IDockable?>>();

            base.InitLayout(layout);
        }
    }
}
