using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.DeferredContentControl;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class TemplateHelperTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Shared_Template_Caches_An_Independent_Control_Per_Dockable(bool tool, bool useIds)
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = useIds };
        var builds = 0;
        var content = new Func<IServiceProvider, object>(_ =>
        {
            builds++;
            return new TextBox();
        });
        var first = CreateTemplateOwner(tool, "first", content, recycling);
        var second = CreateTemplateOwner(tool, "second", content, recycling);

        var firstControl = Assert.IsType<TextBox>(((IDataTemplate)first).Build(first));
        firstControl.Text = "First editor state";
        var secondControl = Assert.IsType<TextBox>(((IDataTemplate)second).Build(second));
        secondControl.Text = "Second editor state";

        Assert.NotSame(firstControl, secondControl);
        Assert.Same(firstControl, ((IDataTemplate)first).Build(first));
        Assert.Same(secondControl, ((IDataTemplate)second).Build(second));
        Assert.Equal("First editor state", firstControl.Text);
        Assert.Equal("Second editor state", secondControl.Text);
        Assert.Equal(2, builds);
        Assert.True(recycling.Remove(first));
        Assert.NotSame(firstControl, ((IDataTemplate)first).Build(first));
        Assert.Same(secondControl, ((IDataTemplate)second).Build(second));
        Assert.Equal(3, builds);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Replacing_Template_Content_Updates_The_Cached_Control(bool tool, bool useIds)
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = useIds };
        var firstTemplate = new Func<IServiceProvider, object>(_ => new TextBox { Text = "first" });
        var secondTemplate = new Func<IServiceProvider, object>(_ => new TextBox { Text = "second" });
        var owner = CreateTemplateOwner(tool, "owner", firstTemplate, recycling);
        var template = (IRecyclingDataTemplate)owner;
        var first = Assert.IsType<TextBox>(template.Build(owner, null));

        SetContent(owner, secondTemplate);
        var second = Assert.IsType<TextBox>(template.Build(owner, first));
        Assert.NotSame(first, second);
        Assert.Equal("second", second.Text);
        Assert.Same(second, template.Build(owner, null));

        SetContent(owner, null);
        Assert.Null(template.Build(owner, second));
        SetContent(owner, firstTemplate);
        var restored = Assert.IsType<TextBox>(template.Build(owner, null));
        Assert.Equal("first", restored.Text);
        Assert.NotSame(second, restored);
        Assert.Same(restored, template.Build(owner, null));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_Replacement_Template_Does_Not_Revive_Previous_Content(bool tool)
    {
        var recycling = new ControlRecycling();
        var owner = CreateTemplateOwner(tool, "owner", new Func<IServiceProvider, object>(_ => new TextBox()), recycling);
        var template = (IRecyclingDataTemplate)owner;
        var previous = template.Build(owner, null);
        Assert.NotNull(previous);

        SetContent(owner, new Func<IServiceProvider, object>(_ => null!));

        Assert.Null(template.Build(owner, previous));
        Assert.Null(template.Build(owner, null));
        Assert.Null(template.Build(owner, null));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Failed_Replacement_Template_Is_Retried_Instead_Of_Reviving_Previous_Content(bool tool)
    {
        var recycling = new ControlRecycling();
        var owner = CreateTemplateOwner(tool, "owner", new Func<IServiceProvider, object>(_ => new TextBox()), recycling);
        var template = (IRecyclingDataTemplate)owner;
        var previous = template.Build(owner, null);
        var attempts = 0;
        SetContent(owner, new Func<IServiceProvider, object>(_ =>
        {
            if (++attempts == 1)
            {
                throw new InvalidOperationException("Template failed");
            }
            return new TextBox { Text = "Replacement" };
        }));

        Assert.Throws<InvalidOperationException>(() => template.Build(owner, previous));
        var replacement = Assert.IsType<TextBox>(template.Build(owner, null));
        Assert.Equal("Replacement", replacement.Text);
        Assert.NotSame(previous, replacement);
        Assert.Same(replacement, template.Build(owner, null));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Id_Recycling_Reuses_Content_For_Replacement_Dockable(bool tool)
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = true };
        var content = new Func<IServiceProvider, object>(_ => new TextBox());
        var first = CreateTemplateOwner(tool, "persisted-id", content, recycling);
        var replacement = CreateTemplateOwner(tool, "persisted-id", content, recycling);

        var control = ((IDataTemplate)first).Build(first);

        Assert.Same(control, ((IDataTemplate)replacement).Build(replacement));
    }

    private static DockableBase CreateTemplateOwner(bool tool, string id, object content, ControlRecycling recycling)
    {
        DockableBase owner = tool ? new Tool { Id = id } : new Document { Id = id };
        SetContent(owner, content);
        ControlRecyclingDataTemplate.SetControlRecycling(owner, recycling);
        return owner;
    }

    private static void SetContent(DockableBase owner, object? content)
    {
        if (owner is Document document)
        {
            document.Content = content;
        }
        else if (owner is Tool tool)
        {
            tool.Content = content;
        }
    }

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
    public void Document_Build_Detaches_Direct_Control_From_DeferredContentControl()
    {
        var control = new Border();
        var deferredHost = new DeferredContentControl { Content = control };
        var window = new Window { Content = deferredHost };
        var document = new Document { Content = control };

        try
        {
            window.Show();
            window.UpdateLayout();

            var result = document.Build(null, null);

            Assert.Same(control, result);
            Assert.Null(deferredHost.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Document_Build_Detaches_Content_Realized_Through_DeferredContentControl_With_Different_Host_Content()
    {
        var control = new Border();
        var document = new Document { Content = control };
        var deferredHost = new DeferredContentControl
        {
            Content = document,
            ContentTemplate = document
        };
        var window = new Window { Content = deferredHost };

        try
        {
            window.Show();
            deferredHost.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Same(control, deferredHost.Presenter?.Child);

            var result = document.Build(null, null);

            Assert.Same(control, result);
            Assert.Null(control.GetVisualParent());
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
    public void Tool_Build_Detaches_Direct_Control_From_DeferredContentControl()
    {
        var control = new Border();
        var deferredHost = new DeferredContentControl { Content = control };
        var window = new Window { Content = deferredHost };
        var tool = new Tool { Content = control };

        try
        {
            window.Show();
            window.UpdateLayout();

            var result = tool.Build(null, null);

            Assert.Same(control, result);
            Assert.Null(deferredHost.Content);
        }
        finally
        {
            window.Close();
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
