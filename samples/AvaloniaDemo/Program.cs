// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using Avalonia;
using Avalonia.Logging.Serilog;
using AvaloniaDemo.ViewModels;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Serializer;

namespace AvaloniaDemo
{
    class Program
    {
        static void Print(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Print(ex.InnerException);
            }
        }

#if NET461
       [STAThread]
#endif
        static void Main(string[] args)
        {
#if !NET461
            Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA);
#endif
            try
            {
                var vm = new MainWindowViewModel();
                var factory = new DemoDockFactory();
                IDock layout = null;

                string path = DockSerializer.GetBasePath("Layout.json");
                if (DockSerializer.Exists(path))
                {
                    layout = DockSerializer.Load<RootDock>(path);
                }

                BuildAvaloniaApp().Start<MainWindow>(() =>
                {
                    // NOTE: Initialize layout after main window was created so child windows can be created.
                    vm.Factory = factory;
                    vm.Layout = layout ?? vm.Factory.CreateLayout();
                    vm.Factory.InitLayout(vm.Layout, vm);
                    return vm;
                });

                vm.Factory.CloseLayout(vm.Layout);

                DockSerializer.Save(path, vm.Layout);
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
