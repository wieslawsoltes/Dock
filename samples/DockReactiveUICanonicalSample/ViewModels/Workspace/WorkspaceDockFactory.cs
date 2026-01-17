using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class WorkspaceDockFactory : Factory
{
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
