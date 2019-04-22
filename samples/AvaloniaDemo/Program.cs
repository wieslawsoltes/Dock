using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Logging.Serilog;
using AvaloniaDemo.Factories;
using AvaloniaDemo.Model;
using AvaloniaDemo.ViewModels;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Serializer;

namespace AvaloniaDemo
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var serializer = new DockJsonSerializer(typeof(ObservableCollection<>));
            var vm = new MainWindowViewModel();
            var factory = new DemoDockFactory(new DemoData());
            IDock layout = null;

            string path = serializer.GetBasePath("Layout.json");
            if (serializer.Exists(path))
            {
                layout = serializer.Load<RootDock>(path);
            }

            BuildAvaloniaApp().Start<MainWindow>(() =>
            {
                vm.Factory = factory;
                vm.Layout = layout ?? vm.Factory.CreateLayout();
                vm.Factory.InitLayout(vm.Layout);
                return vm;
            });

            if (vm.Layout is IDock dock)
            {
                dock.Close();
            }

            serializer.Save(path, vm.Layout);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .LogToDebug();
    }
}
