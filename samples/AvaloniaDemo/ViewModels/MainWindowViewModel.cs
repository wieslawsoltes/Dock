using AvaloniaDemo.Models;
using Dock.Model;
using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private IFactory? _factory;
        private IDockable? _layout;

        public IFactory? Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IDockable? Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public void FileNew()
        {
            if (Layout is IDock root)
            {
                root.Close();
            }
            Factory = new DemoFactory(new DemoData());
            var layout = Factory?.CreateLayout();
            if (layout != null)
            {
                Layout = layout;
                Factory?.InitLayout(Layout);
            }
        }
    }
}
