using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.ViewModels;
using Dock.Avalonia.Controls;

namespace AvaloniaDemo.Views
{
    public class MainView : UserControl
    {
        private readonly DockControl? _dockControl;

        public MainView()
        {
            InitializeComponent();
            _dockControl = this.FindControl<DockControl>("dockControl");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (DataContext is MainWindowViewModel mainWindowViewModel && _dockControl != null)
            {
                mainWindowViewModel.AttachDockControl(_dockControl);
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (DataContext is MainWindowViewModel mainWindowViewModel && _dockControl != null)
            {
                mainWindowViewModel.DetachDockControl();
            }
        }
    }
}
