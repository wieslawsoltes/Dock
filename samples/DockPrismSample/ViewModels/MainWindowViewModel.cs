using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Dock.Model.Controls;
using Dock.Model.Core;
using Prism.Commands;
using Prism.Mvvm;

namespace DockPrismSample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
[RequiresDynamicCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
public class MainWindowViewModel : BindableBase
{
    private readonly IFactory _factory;
    private IRootDock? _layout;
    private string _title = string.Empty;

    public MainWindowViewModel(IFactory dockFactoryService)
    {
        _factory = dockFactoryService;

        DebugFactoryEvents();

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
            layout.Navigate.Execute("Home");
        }

        Title = "Prism.Avalonia Sample";
        Layout = layout;
    }

    public IRootDock? Layout { get => _layout; set => SetProperty(ref _layout, value); }

    public DelegateCommand NewLayout => new(ResetLayout);

    public string Title { get => _title; set => SetProperty(ref _title, value); }

    private void DebugFactoryEvents()
    {
        _factory.ActiveDockableChanged += (_, args)
            => Debug.WriteLine($"[ActiveDockableChanged] Title='{args.Dockable?.Title}'");

        _factory.FocusedDockableChanged += (_, args)
            => Debug.WriteLine($"[FocusedDockableChanged] Title='{args.Dockable?.Title}'");

        _factory.DockableAdded += (_, args)
            => Debug.WriteLine($"[DockableAdded] Title='{args.Dockable?.Title}'");

        _factory.DockableRemoved += (_, args)
            => Debug.WriteLine($"[DockableRemoved] Title='{args.Dockable?.Title}'");

        _factory.DockableClosed += (_, args)
            => Debug.WriteLine($"[DockableClosed] Title='{args.Dockable?.Title}'");

        _factory.DockableMoved += (_, args)
            => Debug.WriteLine($"[DockableMoved] Title='{args.Dockable?.Title}'");

        _factory.DockableDocked += (_, args)
            => Debug.WriteLine($"[DockableDocked] Title='{args.Dockable?.Title}', Operation='{args.Operation}'");

        _factory.DockableUndocked += (_, args)
            => Debug.WriteLine($"[DockableUndocked] Title='{args.Dockable?.Title}', Operation='{args.Operation}'");

        _factory.DockableSwapped += (_, args)
            => Debug.WriteLine($"[DockableSwapped] Title='{args.Dockable?.Title}'");

        _factory.DockablePinned += (_, args)
            => Debug.WriteLine($"[DockablePinned] Title='{args.Dockable?.Title}'");

        _factory.DockableUnpinned += (_, args)
            => Debug.WriteLine($"[DockableUnpinned] Title='{args.Dockable?.Title}'");

        _factory.WindowOpened += (_, args)
            => Debug.WriteLine($"[WindowOpened] Title='{args.Window?.Title}'");

        _factory.WindowClosed += (_, args)
            => Debug.WriteLine($"[WindowClosed] Title='{args.Window?.Title}'");

        _factory.WindowClosing += (_, args) =>
        {
            // NOTE: Set to True to cancel window closing.
#if false
                args.Cancel = true;
#endif
            Debug.WriteLine($"[WindowClosing] Title='{args.Window?.Title}', Cancel={args.Cancel}");
        };

        _factory.WindowAdded += (_, args)
            => Debug.WriteLine($"[WindowAdded] Title='{args.Window?.Title}'");

        _factory.WindowRemoved += (_, args)
            => Debug.WriteLine($"[WindowRemoved] Title='{args.Window?.Title}'");

        _factory.WindowMoveDragBegin += (_, args) =>
        {
            // NOTE: Set to True to cancel window dragging.
#if false
                args.Cancel = true;
#endif
            Debug.WriteLine($"[WindowMoveDragBegin] Title='{args.Window?.Title}', Cancel={args.Cancel}, X='{args.Window?.X}', Y='{args.Window?.Y}'");
        };

        _factory.WindowMoveDrag += (_, args)
            => Debug.WriteLine($"[WindowMoveDrag] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");

        _factory.WindowMoveDragEnd += (_, args)
            => Debug.WriteLine($"[WindowMoveDragEnd] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");

        _factory.WindowActivated += (_, args)
            => Debug.WriteLine($"[WindowActivated] Title='{args.Window?.Title}'");

        _factory.DockableActivated += (_, args)
            => Debug.WriteLine($"[DockableActivated] Title='{args.Dockable?.Title}'");

        _factory.WindowDeactivated += (_, args)
            => Debug.WriteLine($"[WindowDeactivated] Title='{args.Window?.Title}'");

        _factory.DockableDeactivated += (_, args)
            => Debug.WriteLine($"[DockableDeactivated] Title='{args.Dockable?.Title}'");
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
            dock.Close.Execute(null);
    }

    public void ResetLayout()
    {
        if (Layout is not null && Layout.Close.CanExecute(null))
            Layout.Close.Execute(null);

        var layout = _factory.CreateLayout();
        if (layout is not null)
            _factory.InitLayout(layout);

        Layout = layout;
    }
}
