using System.Runtime.CompilerServices;
using ReactiveUI.Reactive.Builder;

namespace Dock.Tests;

internal static class ReactiveUITestInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }
}
