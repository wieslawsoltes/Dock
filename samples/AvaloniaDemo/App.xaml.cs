using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.Factories;
using AvaloniaDemo.Model;
using AvaloniaDemo.Serializer;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;
using Dock.Model;
using Dock.Model.Controls;
using ReactiveUI.Legacy;

namespace AvaloniaDemo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var vm = new MainWindowViewModel();
            var factory = new DemoDockFactory(new DemoData());
            IDock layout = null;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var serializer = new DockJsonSerializer(typeof(ReactiveList<>));
#pragma warning restore CS0618 // Type or member is obsolete
                string path = serializer.GetBasePath("Layout.json");
                if (serializer.Exists(path))
                {
                    layout = serializer.Load<RootDock>(path);
                }

                var window = new MainWindow
                {
                    DataContext = vm
                };

                vm.Factory = factory;
                vm.Layout = layout ?? vm.Factory.CreateLayout();
                vm.Factory.InitLayout(vm.Layout);

                window.Closing += (sender, e) =>
                {
                    if (vm.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                    // TODO: Save main window position, size and state.
                };

                desktopLifetime.MainWindow = window;

                desktopLifetime.Exit += (sennder, e) =>
                {
                    if (vm.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                    serializer.Save(path, vm.Layout);
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                var view = new MainView()
                {
                    DataContext = vm
                };

                vm.Factory = factory;
                vm.Layout = layout ?? vm.Factory.CreateLayout();
                vm.Factory.InitLayout(vm.Layout);

                singleViewLifetime.MainView = view;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
