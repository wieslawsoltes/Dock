using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaDemo.Models;
using AvaloniaDemo.Serializer;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : StyledElement
    {
        private DockControl _dockControl;
        private IFactory _factory;
        private IDockJsonSerializer _serializer;
        private string _path;

        public MainWindowViewModel()
        {
            _factory = new DemoFactory(new DemoData());
            _serializer = new DockJsonSerializer(typeof(AvaloniaList<>));
            _path = Path.Combine(AppContext.BaseDirectory, "Layout.json");
        }

        public void AttachDockControl(DockControl dockControl)
        {
            if (dockControl != null)
            {
                _dockControl = dockControl;

                if (File.Exists(_path))
                {
                    var layout = _serializer.Load<RootDock>(_path);
                    if (layout != null)
                    {
                        _dockControl.Layout = layout;
                    }
                }

                if (_dockControl.Layout != null)
                {
                    _factory.InitLayout(_dockControl.Layout);
                }
            }
        }

        public void DetachDockControl()
        {
            if (_dockControl != null)
            {
                if (_dockControl.Layout != null)
                {
                    _dockControl.Layout.Close();
                    _serializer.Save(_path, _dockControl.Layout);
                }

                _dockControl = null;
            }
        }

        public void FileNew()
        {
            if (_dockControl != null)
            {
                if (_dockControl.Layout is IDock root)
                {
                    root.Close();
                }
                _factory = new DemoFactory(new DemoData());
                _dockControl.Layout = _factory.CreateLayout();
                _factory.InitLayout(_dockControl.Layout);
            }
        }

        public async void FileOpen()
        {
            if (_dockControl != null)
            {
                var dlg = new OpenFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                var result = await dlg.ShowAsync(GetWindow());
                if (result != null)
                {
                    IDock layout = _serializer?.Load<RootDock>(result.FirstOrDefault());
                    if (_dockControl.Layout is IDock root)
                    {
                        root.Close();
                    }
                    _dockControl.Layout = layout;
                    _factory.InitLayout(_dockControl.Layout);
                }
            }
        }

        public async void FileSaveAs()
        {
            if (_dockControl != null)
            {
                var dlg = new SaveFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                dlg.InitialFileName = "Layout";
                dlg.DefaultExtension = "json";
                var result = await dlg.ShowAsync(GetWindow());
                if (result != null)
                {
                    _serializer.Save(result, _dockControl.Layout);
                }
            }
        }

        private Window GetWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }
    }
}
