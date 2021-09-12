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
using Dock.Model.Core;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : StyledElement
    {
        public static readonly StyledProperty<DockControl?> DockControlProperty =
            AvaloniaProperty.Register<MainWindowViewModel, DockControl?>(nameof(DockControl));

        public DockControl? DockControl
        {
            get => GetValue(DockControlProperty);
            set => SetValue(DockControlProperty, value);
        }

        public void AttachDockControl(DockControl dockControl)
        {
            if (dockControl != null)
            {
                DockControl = dockControl;

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
                layout.Close.Execute(null);
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
                var factory = new DemoFactory();
                var layout = factory?.CreateLayout();
                if (layout != null)
                {
                    factory?.InitLayout(layout);
                    DockControl.Layout = layout;
                }
            }
        }

        public void ApplyWindowLayout(IDock dock)
        {
            if (DockControl.Layout?.ActiveDockable is IDock active && dock != active)
            {
                active.Close.Execute(null);
                DockControl.Layout.Navigate.Execute(dock);
                DockControl.Layout.Factory?.SetFocusedDockable(DockControl.Layout, dock);
                DockControl.Layout.DefaultDockable = dock;
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
