// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using Avalonia;
using Avalonia.Logging.Serilog;

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
                BuildAvaloniaApp().Start<MainWindow>();
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
