// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using ReactiveUI.Avalonia;

namespace DockXamlReactiveUISample;

[RequiresUnreferencedCode("Requires unreferenced code for App.")]
[RequiresDynamicCode("Requires dynamic code for App.")]
internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
    }
}
