using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaDemo.Models;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Serializer;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : StyledElement
    {
        public static readonly DirectProperty<MainWindowViewModel, DockControl?> DockControlProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, DockControl?>(nameof(DockControl), o => o.DockControl, (o, v) => o.DockControl = v);

        public static readonly DirectProperty<MainWindowViewModel, IFactory?> FactoryProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, IFactory?>(nameof(Factory), o => o.Factory, (o, v) => o.Factory = v);

        public static readonly DirectProperty<MainWindowViewModel, IDockSerializer?> SerializerProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, IDockSerializer?>(nameof(Serializer), o => o.Serializer, (o, v) => o.Serializer = v);

        public static readonly DirectProperty<MainWindowViewModel, string> PathProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, string>(nameof(Path), o => o.Path, (o, v) => o.Path = v);

        private DockControl? _dockControl;
        private IFactory? _factory;
        private IDockSerializer? _serializer;
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

        public IDockSerializer? Serializer
        {
            get => _serializer;
            set => SetAndRaise(SerializerProperty, ref _serializer, value);
        }

        public string Path
        {
            get => _path;
            set => SetAndRaise(PathProperty, ref _path, value);
        }

        public MainWindowViewModel()
        {
            DockControl = null;
            Factory = new DemoFactory(new DemoData());
            Serializer = new DockSerializer(typeof(AvaloniaList<>));
            Path = System.IO.Path.Combine(AppContext.BaseDirectory, "Layout.json");
        }

        public void AttachDockControl(DockControl dockControl)
        {
            if (dockControl != null)
            {
                DockControl = dockControl;

                if (File.Exists(Path))
                {
                    var layout = Serializer?.Load<RootDock>(Path);
                    if (layout != null)
                    {
                        DockControl.Layout = layout;
                    }
                }

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
                    DockControl.Layout.Close();
                    Serializer?.Save(Path, DockControl.Layout);
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
                    root.Close();
                }
                Factory = new DemoFactory(new DemoData());
                var layout = Factory?.CreateLayout();
                if (layout != null)
                {
                    DockControl.Layout = layout;
                    Factory?.InitLayout(DockControl.Layout);
                }
            }
        }

        public async void FileOpen()
        {
            if (DockControl != null)
            {
                var dlg = new OpenFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                var result = await dlg.ShowAsync(GetWindow());
                if (result != null)
                {
                    IDock? layout = Serializer?.Load<RootDock>(result.FirstOrDefault());
                    if (layout != null)
                    {
                        if (DockControl.Layout is IDock root)
                        {
                            root.Close();
                        }
                        DockControl.Layout = layout;
                        Factory?.InitLayout(DockControl.Layout);
                    }
                }
            }
        }

        public async void FileSaveAs()
        {
            if (DockControl != null)
            {
                var dlg = new SaveFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                dlg.InitialFileName = "Layout";
                dlg.DefaultExtension = "json";
                var result = await dlg.ShowAsync(GetWindow());
                if (result != null)
                {
                    Serializer?.Save(result, DockControl.Layout);
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
