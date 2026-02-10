using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
    public void Document_Build_Detaches_Direct_Control_From_ContentPresenter()
    {
        var control = new Border();
        var presenter = new ContentPresenter { Content = control };
        var window = new Window { Content = presenter };
        var document = new Document { Content = control };

        try
        {
            window.Show();
            window.UpdateLayout();

            var result = document.Build(null, null);

            Assert.Same(control, result);
            Assert.Null(presenter.Content);
            Assert.Null(presenter.Child);
        }
        finally
        {
            window.Close();
        }
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

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Build_Detaches_Direct_Control_From_ContentPresenter()
    {
        var window = new DockWindow();
        var document = new ManagedDockWindowDocument(window);
        var control = new Border();
        var presenter = new ContentPresenter { Content = control };
        var host = new Window { Content = presenter };

        document.Content = control;

        try
        {
            host.Show();
            host.UpdateLayout();

            var result = document.Build(null, null);

            Assert.Same(control, result);
            Assert.Null(presenter.Content);
            Assert.Null(presenter.Child);
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaFact]
    public void Document_Build_Supports_Func_Returning_Control()
    {
        var document = new Document
        {
            Content = new Func<IServiceProvider, object>(_ => new StackPanel())
        };

        var result = document.Build(null, null);

        Assert.IsType<StackPanel>(result);
    }

    [AvaloniaFact]
    public void Tool_Build_Supports_Func_Returning_Control()
    {
        var tool = new Tool
        {
            Content = new Func<IServiceProvider, object>(_ => new StackPanel())
        };

        var result = tool.Build(null, null);

        Assert.IsType<StackPanel>(result);
    }
}
