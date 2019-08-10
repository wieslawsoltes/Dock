using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaDemo.Factories;
using AvaloniaDemo.Model;
using AvaloniaDemo.Serializer;
using Dock.Model;
using Dock.Model.Controls;
using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private IDockJsonSerializer _serializer;
        private IDockFactory _factory;
        private IView _layout;
        private string _currentView;

        public IDockJsonSerializer Serializer
        {
            get => _serializer;
            set => this.RaiseAndSetIfChanged(ref _serializer, value);
        }

        public IDockFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IView Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        public void FileNew()
        {
            if (Layout is IDock root)
            {
                root.Close();
            }
            Factory = new DemoDockFactory(new DemoData());
            Layout = Factory.CreateLayout();
            Factory.InitLayout(Layout);
        }

        public async void FileOpen()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null)
            {
                IDock layout = _serializer?.Load<RootDock>(result.FirstOrDefault());
                if (Layout is IDock root)
                {
                    root.Close();
                }
                Layout = layout;
                Factory.InitLayout(Layout);
            }
        }

        public async void FileSaveAs()
        {
            var dlg = new SaveFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.InitialFileName = "Layout";
            dlg.DefaultExtension = "json";
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null)
            {
                Serializer.Save(result, Layout);
            }
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
