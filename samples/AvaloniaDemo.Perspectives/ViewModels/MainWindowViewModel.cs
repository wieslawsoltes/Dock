using Avalonia;
using Dock.Avalonia.Controls;
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
            DockControl = dockControl;

            var layout = DockControl.Layout;
            if (layout != null)
            {
                var factory = new DockFactory();
                factory?.InitLayout(layout);
            }
        }

        public void DetachDockControl()
        {
            if (DockControl?.Layout is { } layout)
            {
                layout.Close.Execute(null);
                DockControl = null;
            }
        }

        public void FileNew()
        {
            if (DockControl != null)
            {
                if (DockControl.Layout is { } root)
                {
                    root.Close.Execute(null);
                }
                var factory = new DockFactory();
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
            if (DockControl?.Layout?.ActiveDockable is IDock active && dock != active)
            {
                active.Close.Execute(null);
                DockControl.Layout.Navigate.Execute(dock);
                DockControl.Layout.Factory?.SetFocusedDockable(DockControl.Layout, dock);
                DockControl.Layout.DefaultDockable = dock;
            }
        }
    }
}
