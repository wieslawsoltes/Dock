using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Views.Views
{
    public class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
