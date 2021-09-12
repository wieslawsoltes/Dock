using Avalonia;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : StyledElement
    {
        public static readonly DirectProperty<MainWindowViewModel, DockControl?> DockControlProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, DockControl?>(
                nameof(DockControl), 
                o => o.DockControl, 
                (o, v) => o.DockControl = v);

        public static readonly DirectProperty<MainWindowViewModel, IFactory?> FactoryProperty =
            AvaloniaProperty.RegisterDirect<MainWindowViewModel, IFactory?>(
                nameof(Factory), 
                o => o.Factory, 
                (o, v) => o.Factory = v);

        private DockControl? _dockControl;
        private IFactory? _factory;

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

        public MainWindowViewModel()
        {
            DockControl = null;
            Factory = new DockFactory("Demo");
        }

        public void AttachDockControl(DockControl dockControl)
        {
            DockControl = dockControl;
                
            if (DockControl.Layout != null)
            {
                Factory?.InitLayout(DockControl.Layout);
            }
        }

        public void DetachDockControl()
        {
            if (DockControl == null)
            {
                return;
            }

            DockControl.Layout?.Close.Execute(null);
            DockControl = null;
        }

        public void FileNew()
        {
            if (DockControl == null)
            {
                return;
            }
            if (DockControl.Layout is { } root)
            {
                root.Close.Execute(null);
            }
            Factory = new DockFactory("Demo");
            var layout = Factory?.CreateLayout();
            if (layout != null)
            {
                Factory?.InitLayout(layout);
                DockControl.Layout = layout;
            }
        }
    }
}
