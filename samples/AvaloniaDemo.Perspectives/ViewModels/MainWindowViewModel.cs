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
        public static readonly DirectProperty<MainWindowViewModel, DockControl> DockControlProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, DockControl>(nameof(DockControl), o => o.DockControl, (o, v) => o.DockControl = v);

        private DockControl _dockControl;

        public DockControl DockControl
        {
            get { return _dockControl; }
            set { SetAndRaise(DockControlProperty, ref _dockControl, value); }
        }

        public void AttachDockControl(DockControl dockControl)
        {
            if (dockControl != null)
            {
                DockControl = dockControl;

                var path = Path.Combine(AppContext.BaseDirectory, "Layout.json");
                if (File.Exists(path))
                {
                    var layout = new DockSerializer(typeof(AvaloniaList<>)).Load<RootDock>(path);
                    if (layout != null)
                    {
                        var factory = new DemoFactory();
                        factory.InitLayout(layout);
                        DockControl.Layout = layout;
                    }
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
                var layout = factory.CreateLayout();
                factory.InitLayout(layout);
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
                            factory.InitLayout(layout);
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

        public void SaveWindowLayout(IDockable dockable)
        {
            if (dockable != null && dockable.Owner is IDock owner)
            {
                var clone = dockable.Clone();
                if (clone != null)
                {
                    owner.Factory.AddDockable(owner, clone);
                    owner.Factory.SetAvtiveDockable(clone);
                }
            }
        }

        public void ApplyWindowLayout(IDockable dockable)
        {
            if (dockable != null && dockable.Owner is IDock dock)
            {
                dock.Factory.SetAvtiveDockable(dockable);
            }
        }

        public void ManageWindowLayouts(IDock dock)
        {
            // TODO:
        }

        public void ResetWindowLayout(IDockable dockable)
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
