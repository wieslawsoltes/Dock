using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using ReactiveUI.Avalonia;

namespace DockOverlayReactiveUISample;

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
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.UseReactiveUI()
			.LogToTrace();
}
