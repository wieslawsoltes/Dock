using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class WorkspaceDockFactory : Factory
{
    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);

        if (window is not null)
        {
            window.Title = dockable.Title ?? "Dock Workspace";
        }

        return window;
    }
}
