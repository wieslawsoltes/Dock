using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DeferredContentControlTests
{
    private sealed class CountingTemplate : IRecyclingDataTemplate
    {
        private readonly Func<Control> _factory;

        public CountingTemplate(Func<Control> factory)
        {
            _factory = factory;
        }

        public int BuildCount { get; private set; }

        public Control? Build(object? param)
        {
            return Build(param, null);
        }

        public Control? Build(object? data, Control? existing)
        {
            if (existing is not null)
            {
                return existing;
            }

            BuildCount++;
            return _factory();
        }

        public bool Match(object? data)
        {
            return true;
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Defers_Content_Materialization_Until_Dispatcher_Run()
    {
        var template = new CountingTemplate(() => new Border
        {
            Child = new TextBlock { Text = "Deferred" }
        });
        var control = new DeferredContentControl
        {
            Content = new object(),
            ContentTemplate = template
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            Assert.NotNull(control.Presenter);
            Assert.Null(control.Presenter!.Child);
            Assert.Equal(0, template.BuildCount);

            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.NotNull(control.Presenter.Child);
            Assert.Equal(1, template.BuildCount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Batches_Content_Changes_Into_A_Single_Build()
    {
        var template = new CountingTemplate(() => new TextBlock());
        var control = new DeferredContentControl
        {
            ContentTemplate = template
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();

        try
        {
            control.Content = "First";
            control.Content = "Second";
            window.UpdateLayout();

            Assert.Equal(0, template.BuildCount);
            Assert.Null(control.Presenter?.Child);

            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(1, template.BuildCount);
            var textBlock = Assert.IsType<TextBlock>(control.Presenter!.Child);
            Assert.Equal("Second", textBlock.DataContext);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Reattaches_And_Applies_Deferred_Content_Changes()
    {
        var template = new CountingTemplate(() => new TextBlock());
        var control = new DeferredContentControl
        {
            Content = "First",
            ContentTemplate = template
        };
        var firstWindow = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        firstWindow.Show();
        control.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        firstWindow.UpdateLayout();

        Assert.Equal(1, template.BuildCount);

        firstWindow.Content = null;
        firstWindow.Close();
        control.Content = "Second";

        Assert.Equal(1, template.BuildCount);

        var secondWindow = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        secondWindow.Show();

        try
        {
            Dispatcher.UIThread.RunJobs();
            secondWindow.UpdateLayout();

            Assert.Equal(2, template.BuildCount);
            var textBlock = Assert.IsType<TextBlock>(control.Presenter!.Child);
            Assert.Equal("Second", textBlock.DataContext);
        }
        finally
        {
            secondWindow.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentContentControl_Defers_Document_Template_Materialization()
    {
        var buildCount = 0;
        var document = new Document
        {
            Content = (Func<IServiceProvider, object>)(_ =>
            {
                buildCount++;
                return new TextBlock { Text = "Document" };
            })
        };
        var control = new DocumentContentControl
        {
            DataContext = document
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            Assert.Equal(0, buildCount);
            Assert.DoesNotContain(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Document");

            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(1, buildCount);
            Assert.Contains(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Document");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolContentControl_Defers_Tool_Template_Materialization()
    {
        var buildCount = 0;
        var tool = new Tool
        {
            Content = (Func<IServiceProvider, object>)(_ =>
            {
                buildCount++;
                return new TextBlock { Text = "Tool" };
            })
        };
        var control = new ToolContentControl
        {
            DataContext = tool
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            Assert.Equal(0, buildCount);
            Assert.DoesNotContain(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Tool");

            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(1, buildCount);
            Assert.Contains(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Tool");
        }
        finally
        {
            window.Close();
        }
    }
}
