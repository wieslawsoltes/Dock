using Avalonia.ReactiveUI;
using Avalonia.Web.Blazor;

namespace AvaloniaDemo.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        WebAppBuilder.Configure<AvaloniaDemo.App>()
            .UseReactiveUI()
            .SetupWithSingleViewLifetime();
    }
}
