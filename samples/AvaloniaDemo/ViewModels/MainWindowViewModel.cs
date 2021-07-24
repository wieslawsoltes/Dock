using System.Diagnostics;
using System.Windows.Input;
using AvaloniaDemo.Models;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IFactory? _factory;
        private IRootDock? _layout;

        public IRootDock? Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public ICommand NewLayout { get; }

        public MainWindowViewModel()
        {
            _factory = new DemoFactory(new DemoData());

            DebugFactoryEvents(_factory);

            Layout = _factory?.CreateLayout();
            if (Layout is { })
            {
                _factory?.InitLayout(Layout);
                if (Layout is { } root)
                {
                    root.Navigate.Execute("Home");
                }
            }

            NewLayout = ReactiveCommand.Create(ResetLayout);
        }

        private void DebugFactoryEvents(IFactory factory)
        {
            factory.ActiveDockableChanged += (_, args) =>
            {
                Debug.WriteLine($"[ActiveDockableChanged] {args.Dockable?.Title}");
            };

            factory.FocusedDockableChanged += (_, args) =>
            {
                Debug.WriteLine($"[FocusedDockableChanged] {args.Dockable?.Title}");
            };

            factory.DockableAdded += (_, args) =>
            {
                Debug.WriteLine($"[DockableAdded] {args.Dockable?.Title}");
            };

            factory.DockableRemoved += (_, args) =>
            {
                Debug.WriteLine($"[DockableRemoved] {args.Dockable?.Title}");
            };

            factory.DockableClosed += (_, args) =>
            {
                Debug.WriteLine($"[DockableClosed] {args.Dockable?.Title}");
            };

            factory.DockableMoved += (_, args) =>
            {
                Debug.WriteLine($"[DockableMoved] {args.Dockable?.Title}");
            };

            factory.DockableSwapped += (_, args) =>
            {
                Debug.WriteLine($"[DockableSwapped] {args.Dockable?.Title}");
            };

            factory.DockablePinned += (_, args) =>
            {
                Debug.WriteLine($"[DockablePinned] {args.Dockable?.Title}");
            };

            factory.DockableUnpinned += (_, args) =>
            {
                Debug.WriteLine($"[DockableUnpinned] {args.Dockable?.Title}");
            };

            factory.WindowOpened += (_, args) =>
            {
                Debug.WriteLine($"[WindowOpened] {args.Window?.Title}");
            };

            factory.WindowClosed += (_, args) =>
            {
                Debug.WriteLine($"[WindowClosed] {args.Window?.Title}");
            };

            factory.WindowClosing += (_, args) =>
            {
                //args.Cancel = true;
                Debug.WriteLine($"[WindowClosing] {args.Window?.Title}");
            };

            factory.WindowAdded += (_, args) =>
            {
                Debug.WriteLine($"[WindowAdded] {args.Window?.Title}");
            };

            factory.WindowRemoved += (_, args) =>
            {
                Debug.WriteLine($"[WindowRemoved] {args.Window?.Title}");
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
                Layout = layout as IRootDock;
                _factory?.InitLayout(layout);
            }
        }
    }
}
