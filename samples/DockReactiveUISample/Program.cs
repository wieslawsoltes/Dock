// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics.CodeAnalysis;
using Dock.Settings;

namespace DockReactiveUISample;

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
            .UseReactiveUI()
            .ShowDockablePreviewOnDrag()
            .SetDragPreviewOpacity(0.6)
            // .UseManagedWindows()
            .LogToTrace();
}
