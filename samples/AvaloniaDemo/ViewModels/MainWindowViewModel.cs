using AvaloniaDemo.Models;
using Dock.Model;
using Dock.Model.Core;
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
                if (root.Close.CanExecute(null))
                {
                    root.Close.Execute(null);
                }
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
