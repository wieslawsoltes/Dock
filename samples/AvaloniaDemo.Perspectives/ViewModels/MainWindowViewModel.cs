using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Serializer;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : StyledElement
    {
        public static readonly StyledProperty<DockControl> DockControlProperty =
            AvaloniaProperty.Register<MainWindowViewModel, DockControl>(nameof(DockControl));

        public DockControl DockControl
        {
            get => GetValue(DockControlProperty);
            set => SetValue(DockControlProperty, value);
        }

        public void AttachDockControl(DockControl dockControl)
        {
            if (dockControl != null)
            {
                DockControl = dockControl;

                var path = Path.Combine(AppContext.BaseDirectory, "Layout.json");
                if (File.Exists(path))
                {
                    var root = new DockSerializer(typeof(AvaloniaList<>)).Load<RootDock>(path);
                    if (root != null)
                    {
                        DockControl.Layout = root;
                    }
                }

                var layout = DockControl.Layout;
                if (layout != null)
                {
                    var factory = new DemoFactory();
                    factory?.InitLayout(layout);
                }
            }
        }

        public void DetachDockControl()
        {
            if (DockControl?.Layout is IDock layout)
            {
                layout.Close();
                var path = Path.Combine(AppContext.BaseDirectory, "Layout.json");
                new DockSerializer(typeof(AvaloniaList<>)).Save(path, DockControl.Layout);
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
                var factory = new DemoFactory();
                var layout = factory?.CreateLayout();
                factory?.InitLayout(layout);
                DockControl.Layout = layout;
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
                    var path = result.FirstOrDefault();
                    if (path != null)
                    {
                        var layout = new DockSerializer(typeof(AvaloniaList<>)).Load<RootDock>(path);
                        if (layout != null)
                        {
                            if (DockControl.Layout is IDock root)
                            {
                                root.Close();
                            }
                            var factory = new DemoFactory();
                            factory?.InitLayout(layout);
                            DockControl.Layout = layout;
                        }
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
                    new DockSerializer(typeof(AvaloniaList<>)).Save(result, DockControl.Layout);
                }
            }
        }

        public void SaveWindowLayout(IDock dock)
        {
            if (dock != null && dock.Owner is IDock owner)
            {
                var clone = (IDock)dock.Clone();
                if (clone != null)
                {
                    owner.Factory?.AddDockable(owner, clone);
                    ApplyWindowLayout(clone);
                }
            }
        }

        public void ApplyWindowLayout(IDock dock)
        {
            if (dock != null)
            {
                if (DockControl.Layout is IDock root)
                {
                    root.Navigate(dock);
                    root.Factory?.SetFocusedDockable(root, dock);
                    root.DefaultDockable = dock;
                }
            }
        }

        public void ManageWindowLayouts(IDock dock)
        {
            // TODO:
        }

        public void ResetWindowLayout(IDock dock)
        {
            // TODO:
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
