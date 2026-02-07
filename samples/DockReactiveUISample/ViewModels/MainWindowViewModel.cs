using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using DockReactiveUISample.Models;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace DockReactiveUISample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
[RequiresDynamicCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
public class MainWindowViewModel : ReactiveObject
{
    private readonly IFactory? _factory;
    private IRootDock? _layout;
    private string _globalStatus = "Global: (none)";

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public string GlobalStatus
    {
        get => _globalStatus;
        set => this.RaiseAndSetIfChanged(ref _globalStatus, value);
    }

    public ICommand NewLayout { get; }

    public MainWindowViewModel()
    {
        _factory = new DockFactory(new DemoData());

        DebugFactoryEvents(_factory);

        var layout = _factory?.CreateLayout();
        if (layout is not null)
        {
            _factory?.InitLayout(layout);
            layout.Navigate.Execute("Home");
        }
        Layout = layout;
        GlobalStatus = layout is null
            ? "Global: (none)"
            : FormatGlobalStatus(_factory?.GlobalDockTrackingState ?? GlobalDockTrackingState.Empty);

        NewLayout = ReactiveCommand.Create(ResetLayout);
    }

    private void DebugFactoryEvents(IFactory factory)
    {
        factory.ActiveDockableChanged += (_, args) =>
        {
            Debug.WriteLine($"[ActiveDockableChanged] Title='{args.Dockable?.Title}', Root='{args.RootDock?.Id}', Window='{args.Window?.Id}'");
        };

        factory.FocusedDockableChanged += (_, args) =>
        {
            Debug.WriteLine($"[FocusedDockableChanged] Title='{args.Dockable?.Title}', Root='{args.RootDock?.Id}', Window='{args.Window?.Id}'");
        };

        factory.GlobalDockTrackingChanged += (_, args) =>
        {
            GlobalStatus = FormatGlobalStatus(args.Current);
            Debug.WriteLine($"[GlobalDockTrackingChanged] Reason='{args.Reason}', Dockable='{args.Current.Dockable?.Title}', Root='{args.Current.RootDock?.Id}', Window='{args.Current.Window?.Id}'");
        };

        factory.DockableAdded += (_, args) =>
        {
            Debug.WriteLine($"[DockableAdded] Title='{args.Dockable?.Title}'");
        };

        factory.DockableRemoved += (_, args) =>
        {
            Debug.WriteLine($"[DockableRemoved] Title='{args.Dockable?.Title}'");
        };

        factory.DockableClosed += (_, args) =>
        {
            Debug.WriteLine($"[DockableClosed] Title='{args.Dockable?.Title}'");
        };

        factory.DockableMoved += (_, args) =>
        {
            Debug.WriteLine($"[DockableMoved] Title='{args.Dockable?.Title}'");
        };

        factory.DockableDocked += (_, args) =>
        {
            Debug.WriteLine($"[DockableDocked] Title='{args.Dockable?.Title}', Operation='{args.Operation}'");
        };

        factory.DockableUndocked += (_, args) =>
        {
            Debug.WriteLine($"[DockableUndocked] Title='{args.Dockable?.Title}', Operation='{args.Operation}'");
        };

        factory.DockableSwapped += (_, args) =>
        {
            Debug.WriteLine($"[DockableSwapped] Title='{args.Dockable?.Title}'");
        };

        factory.DockablePinned += (_, args) =>
        {
            Debug.WriteLine($"[DockablePinned] Title='{args.Dockable?.Title}'");
        };

        factory.DockableUnpinned += (_, args) =>
        {
            Debug.WriteLine($"[DockableUnpinned] Title='{args.Dockable?.Title}'");
        };

        factory.WindowOpened += (_, args) =>
        {
            Debug.WriteLine($"[WindowOpened] Title='{args.Window?.Title}'");
        };

        factory.WindowClosed += (_, args) =>
        {
            Debug.WriteLine($"[WindowClosed] Title='{args.Window?.Title}'");
        };

        factory.WindowClosing += (_, args) =>
        {
            // NOTE: Set to True to cancel window closing.
#if false
                args.Cancel = true;
#endif
            Debug.WriteLine($"[WindowClosing] Title='{args.Window?.Title}', Cancel={args.Cancel}");
        };

        factory.WindowAdded += (_, args) =>
        {
            Debug.WriteLine($"[WindowAdded] Title='{args.Window?.Title}'");
        };

        factory.WindowRemoved += (_, args) =>
        {
            Debug.WriteLine($"[WindowRemoved] Title='{args.Window?.Title}'");
        };

        factory.WindowMoveDragBegin += (_, args) =>
        {
            // NOTE: Set to True to cancel window dragging.
#if false
                args.Cancel = true;
#endif
            Debug.WriteLine($"[WindowMoveDragBegin] Title='{args.Window?.Title}', Cancel={args.Cancel}, X='{args.Window?.X}', Y='{args.Window?.Y}'");
        };

        factory.WindowMoveDrag += (_, args) =>
        {
            Debug.WriteLine($"[WindowMoveDrag] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
        };

        factory.WindowMoveDragEnd += (_, args) =>
        {
            Debug.WriteLine($"[WindowMoveDragEnd] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
        };

        factory.WindowActivated += (_, args) =>
        {
            Debug.WriteLine($"[WindowActivated] Title='{args.Window?.Title}'");
        };

        factory.DockableActivated += (_, args) =>
        {
            Debug.WriteLine($"[DockableActivated] Title='{args.Dockable?.Title}'");
        };

        factory.WindowDeactivated += (_, args) =>
        {
            Debug.WriteLine($"[WindowDeactivated] Title='{args.Window?.Title}'");
        };

        factory.DockableDeactivated += (_, args) =>
        {
            Debug.WriteLine($"[DockableDeactivated] Title='{args.Dockable?.Title}'");
        };
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock)
        {
            if (dock.Close.CanExecute(null))
            {
                dock.Close.Execute(null);
            }
        }
    }

    public void ResetLayout()
    {
        if (Layout is not null)
        {
            if (Layout.Close.CanExecute(null))
            {
                Layout.Close.Execute(null);
            }
        }

        var layout = _factory?.CreateLayout();
        if (layout is not null)
        {
            _factory?.InitLayout(layout);
            Layout = layout;
        }
    }

    private static string FormatGlobalStatus(GlobalDockTrackingState state)
    {
        var dockableTitle = state.Dockable?.Title ?? "(none)";
        var rootId = state.RootDock?.Id ?? "(none)";
        var windowTitle = state.Window?.Title ?? "(main)";
        var host = state.HostWindow?.GetType().Name ?? "(main)";
        return $"Dockable: {dockableTitle} | Root: {rootId} | Window: {windowTitle} | Host: {host}";
    }
}
