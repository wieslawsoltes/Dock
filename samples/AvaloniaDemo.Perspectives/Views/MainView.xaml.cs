using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.ViewModels;
using Dock.Avalonia.Controls;

namespace AvaloniaDemo.Views
{
    public class MainView : UserControl
    {
        private DockControl _dockControl;

        public MainView()
        {
            this.InitializeComponent();
            _dockControl = this.FindControl<DockControl>("dockControl");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (this.DataContext is MainWindowViewModel mainWindowViewModel)
            {
                if (_dockControl != null)
                {
                    mainWindowViewModel.AttachDockControl(_dockControl);
                }
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (this.DataContext is MainWindowViewModel mainWindowViewModel)
            {
                if (_dockControl != null)
                {
                    mainWindowViewModel.DetachDockControl();
                }
            }
        }
    }
}
