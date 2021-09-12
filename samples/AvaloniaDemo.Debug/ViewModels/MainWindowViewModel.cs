using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : StyledElement
    {
        public static readonly DirectProperty<MainWindowViewModel, DockControl?> DockControlProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, DockControl?>(nameof(DockControl), o => o.DockControl, (o, v) => o.DockControl = v);

        public static readonly DirectProperty<MainWindowViewModel, IFactory?> FactoryProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, IFactory?>(nameof(Factory), o => o.Factory, (o, v) => o.Factory = v);

        public static readonly DirectProperty<MainWindowViewModel, string> PathProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, string>(nameof(Path), o => o.Path, (o, v) => o.Path = v);

        private DockControl? _dockControl;
        private IFactory? _factory;
        private string _path = string.Empty;

        public DockControl? DockControl
        {
            get => _dockControl;
            set => SetAndRaise(DockControlProperty, ref _dockControl, value);
        }

        public IFactory? Factory
        {
            get => _factory;
            set => SetAndRaise(FactoryProperty, ref _factory, value);
        }

        public string Path
        {
            get => _path;
            set => SetAndRaise(PathProperty, ref _path, value);
        }

        public MainWindowViewModel()
        {
            DockControl = null;
            Factory = new DemoFactory("Demo");
            Path = System.IO.Path.Combine(AppContext.BaseDirectory, "Layout.json");
        }

        public void AttachDockControl(DockControl dockControl)
        {
            if (dockControl != null)
            {
                DockControl = dockControl;
                
                if (DockControl.Layout != null)
                {
                    Factory?.InitLayout(DockControl.Layout);
                }
            }
        }

        public void DetachDockControl()
        {
            if (DockControl != null)
            {
                if (DockControl.Layout != null)
                {
                    DockControl.Layout.Close.Execute(null);
                }

                DockControl = null;
            }
        }

        public void FileNew()
        {
            if (DockControl != null)
            {
                if (DockControl.Layout is IDock root)
                {
                    root.Close.Execute(null);
                }
                Factory = new DemoFactory("Demo");
                var layout = Factory?.CreateLayout();
                if (layout != null)
                {
                    Factory?.InitLayout(layout);
                    DockControl.Layout = layout;
                }
            }
        }
        
        private Window? GetWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }
    }
}
