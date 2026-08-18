using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;

namespace Dock.Controls.Recycling.UnitTests.Controls;

public class ControlRecyclingDataTemplateTests
{
    [AvaloniaFact]
    public void Build_Returns_Null_When_No_Parent()
    {
        var template = new ControlRecyclingDataTemplate();
        Assert.Null(template.Build(new object(), null));
    }

    [AvaloniaFact]
    public void Build_Detaches_Control_From_ContentPresenter()
    {
        var control = new TextBlock();
        var presenter = new ContentPresenter { Content = control };
        var window = new Window { Content = presenter };
        var template = new ControlRecyclingDataTemplate();

        try
        {
            window.Show();
            window.UpdateLayout();

            var result = template.Build(control, null);

            Assert.Same(control, result);
            Assert.Null(presenter.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Build_Detaches_Control_From_ContentPresenter_Hosted_By_Templated_Control_With_Different_Content()
    {
        var control = new TextBlock();
        var hostContent = new object();
        var host = new ManualPresenterHost { Content = hostContent };
        var window = new Window { Content = host };
        var template = new ControlRecyclingDataTemplate();

        try
        {
            window.Show();
            host.ApplyTemplate();
            window.UpdateLayout();
            host.Presenter!.Content = control;
            window.UpdateLayout();

            Assert.Same(control, host.Presenter.Child);

            var result = template.Build(control, null);

            Assert.Same(control, result);
            Assert.Null(control.GetVisualParent());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Build_Uses_Parent_ControlRecycling()
    {
        var parent = new Control();
        var recycling = new ControlRecycling();
        ControlRecyclingDataTemplate.SetControlRecycling(parent, recycling);
        parent.DataTemplates.Add(new FuncDataTemplate<string>((_, _) => new TextBlock(), true));
        var template = new ControlRecyclingDataTemplate { Parent = parent };

        var control = template.Build("test", null);

        Assert.NotNull(control);
        Assert.True(recycling.TryGetValue("test", out var cached));
        Assert.Same(control, cached);
    }

    [AvaloniaFact]
    public void Build_Falls_Back_To_Parent_Template()
    {
        var parent = new Control();
        parent.DataTemplates.Add(new FuncDataTemplate<int>((_, _) => new TextBlock(), true));
        var template = new ControlRecyclingDataTemplate { Parent = parent };

        var control = template.Build(5, null);

        Assert.NotNull(control);
    }
}
