using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class TemplateHelperTests
{
    [AvaloniaFact]
    public void Document_Build_Detaches_Direct_Control_From_Parent()
    {
        var control = new Border();
        var host = new ContentControl { Content = control };
        var document = new Document { Content = control };

        var result = document.Build(null, null);

        Assert.Same(control, result);
        Assert.Null(host.Content);
    }

    [AvaloniaFact]
    public void Tool_Build_Detaches_Direct_Control_From_Parent()
    {
        var control = new Border();
        var host = new ContentControl { Content = control };
        var tool = new Tool { Content = control };

        var result = tool.Build(null, null);

        Assert.Same(control, result);
        Assert.Null(host.Content);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Build_Detaches_Direct_Control_From_Parent()
    {
        var window = new DockWindow();
        var document = new ManagedDockWindowDocument(window);
        var control = new Border();
        var host = new ContentControl { Content = control };

        document.Content = control;

        var result = document.Build(null, null);

        Assert.Same(control, result);
        Assert.Null(host.Content);
    }
}
