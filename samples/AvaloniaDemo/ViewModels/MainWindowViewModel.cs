using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaDemo.Models;
using AvaloniaDemo.Serializer;
using Dock.Model;
using Dock.Model.Controls;
using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private IDockJsonSerializer _serializer;
        private IFactory _factory;
        private IDockable _layout;

        public IDockJsonSerializer Serializer
        {
            get => _serializer;
            set => this.RaiseAndSetIfChanged(ref _serializer, value);
        }

        public IFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IDockable Layout
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
