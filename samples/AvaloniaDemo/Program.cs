using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Logging.Serilog;
using AvaloniaDemo.Serializer;
using AvaloniaDemo.ViewModels;
using Dock.Model;
using Dock.Model.Controls;
//using ReactiveUI;

namespace AvaloniaDemo
{
    internal class Program
    {
        private static void Print(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Print(ex.InnerException);
            }
        }

        static Program()
        {
            ModelSerializer.Serializer = new NewtonsoftJsonSerializer(typeof(ObservableCollection<>));
            //ModelSerializer.Serializer = new NewtonsoftJsonSerializer(typeof(ReactiveList<>));
        }

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                var vm = new MainWindowViewModel();
                var factory = new DemoDockFactory();
                IDock layout = null;

                string path = ModelSerializer.GetBasePath("Layout.json");
                if (ModelSerializer.Exists(path))
                {
                    layout = ModelSerializer.Load<RootDock>(path);
                }

                BuildAvaloniaApp().Start<MainWindow>(() =>
                {
                    vm.Factory = factory;
                    vm.Layout = layout ?? vm.Factory.CreateLayout();
                    vm.Factory.InitLayout(vm.Layout, vm);
                    return vm;
                });

                if (vm.Layout is IDock dock)
                {
                    dock.Close();
                }

                ModelSerializer.Save(path, vm.Layout);
            }
            catch (Exception ex)
            {
                Print(ex);
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .LogToDebug();
    }
}
