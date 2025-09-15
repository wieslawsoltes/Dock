// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DockPrismSample;

[RequiresUnreferencedCode("Requires unreferenced code for App.")]
[RequiresDynamicCode("Requires unreferenced code for App.")]
internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
