using System;
using System.Runtime.CompilerServices;
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
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public void Unavailable_Replacement_Fallback_Does_Not_Cache_Previous_Content(bool tool, int failure)
    {
        var recycling = new ControlRecycling();
        var owner = CreateTemplateOwner(tool, "owner", new Func<IServiceProvider, object>(_ => new TextBox()), recycling);
        var template = (IRecyclingDataTemplate)owner;
        var previous = template.Build(owner, null);
        var unavailable = new TextBox();
        var host = new UnsupportedParent(unavailable);
        var attempts = 0;
        var failedAttempts = failure == 0 ? 2 : 3;
        SetContent(owner, new Func<IServiceProvider, object>(_ =>
        {
            attempts++;
            if (attempts > failedAttempts)
            {
                return new TextBox { Text = "Replacement" };
            }

            return attempts == failedAttempts && failure != 2 ? null! : unavailable;
        }));

        Assert.Same(previous, template.Build(owner, previous));
        Assert.Equal(failedAttempts, attempts);
        Assert.Same(host, unavailable.GetVisualParent());

        var replacement = Assert.IsType<TextBox>(template.Build(owner, null));
        Assert.Equal("Replacement", replacement.Text);
        Assert.NotSame(previous, replacement);
        Assert.Equal(failedAttempts + 1, attempts);
        Assert.Same(replacement, template.Build(owner, null));
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Explicit_Id_Recycling_Reuses_Content_For_Replacement_Dockable(bool tool, bool replaceTemplate)
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = true };
        var content = new Func<IServiceProvider, object>(_ => new TextBox());
        var first = CreateTemplateOwner(tool, "persisted-id", content, recycling);
        var replacementContent = replaceTemplate ? new Func<IServiceProvider, object>(_ => new TextBox()) : content;
        var replacement = CreateTemplateOwner(tool, "persisted-id", replacementContent, recycling);

        var control = ((IDataTemplate)first).Build(first);

        Assert.Same(control, ((IDataTemplate)replacement).Build(replacement));
    }

    [AvaloniaTheory]
    [InlineData(false, 0, false)]
    [InlineData(false, 0, true)]
    [InlineData(false, 1, false)]
    [InlineData(false, 1, true)]
    [InlineData(false, 2, false)]
    [InlineData(false, 2, true)]
    [InlineData(true, 0, false)]
    [InlineData(true, 0, true)]
    [InlineData(true, 1, false)]
    [InlineData(true, 1, true)]
    [InlineData(true, 2, false)]
    [InlineData(true, 2, true)]
    public void Recycling_Scope_And_Key_RoundTrips_Use_The_Control_For_The_Current_Template(
        bool tool, int change, bool replaceContent)
    {
        var firstScope = new ControlRecycling { TryToUseIdAsKey = change == 1 };
        var secondScope = new ControlRecycling();
        var owner = CreateTemplateOwner(tool, "first", new Func<IServiceProvider, object>(
            _ => new TextBox { Text = "Original template" }), firstScope);
        var template = (IRecyclingDataTemplate)owner;
        var first = Assert.IsType<TextBox>(template.Build(owner, null));
        first.Text = "First edited view";

        SelectScope(true);
        if (replaceContent)
        {
            SetContent(owner, new Func<IServiceProvider, object>(
                _ => new TextBox { Text = "Replacement template" }));
        }

        var second = Assert.IsType<TextBox>(template.Build(owner, null));
        Assert.NotSame(first, second);
        second.Text = "Second edited view";

        SelectScope(false);
        var returned = Assert.IsType<TextBox>(template.Build(owner, null));
        if (replaceContent)
        {
            Assert.NotSame(first, returned);
            Assert.Equal("Replacement template", returned.Text);
        }
        else
        {
            Assert.Same(first, returned);
            Assert.Equal("First edited view", returned.Text);
        }

        for (var visit = 0; visit < 3; visit++)
        {
            SelectScope(true);
            Assert.Same(second, template.Build(owner, null));
            Assert.Equal("Second edited view", second.Text);
            SelectScope(false);
            Assert.Same(returned, template.Build(owner, null));
        }

        void SelectScope(bool useSecond)
        {
            switch (change)
            {
                case 0:
                    ControlRecyclingDataTemplate.SetControlRecycling(owner, useSecond ? secondScope : firstScope);
                    break;
                case 1:
                    owner.Id = useSecond ? "second" : "first";
                    break;
                case 2:
                    firstScope.TryToUseIdAsKey = useSecond;
                    break;
            }
        }
    }

    [AvaloniaTheory]
    [InlineData(false, 0, false)]
    [InlineData(false, 0, true)]
    [InlineData(false, 1, false)]
    [InlineData(false, 1, true)]
    [InlineData(false, 2, false)]
    [InlineData(false, 2, true)]
    [InlineData(true, 0, false)]
    [InlineData(true, 0, true)]
    [InlineData(true, 1, false)]
    [InlineData(true, 1, true)]
    [InlineData(true, 2, false)]
    [InlineData(true, 2, true)]
    public void Replaced_Template_And_Released_Recycling_Scope_Are_Not_Retained(
        bool tool, int replacement, bool releaseScope)
    {
        var (owner, content, control, scope) = BuildAndReplaceTemplate(tool, replacement, releaseScope);
        for (var attempt = 0; attempt < 10
             && (content.IsAlive || (releaseScope && (control.IsAlive || scope.IsAlive))); attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.False(content.IsAlive);
        Assert.Equal(!releaseScope, control.IsAlive);
        Assert.Equal(!releaseScope, scope.IsAlive);
        if (!releaseScope)
        {
            SetContent(owner, new Func<IServiceProvider, object>(
                _ => new TextBox { Text = "After template collection" }));
            var rebuilt = Assert.IsType<TextBox>(((IDataTemplate)owner).Build(owner));
            Assert.NotSame(control.Target, rebuilt);
            Assert.Equal("After template collection", rebuilt.Text);
        }

        GC.KeepAlive(owner);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DockableBase Owner, WeakReference Content, WeakReference Control, WeakReference Scope)
        BuildAndReplaceTemplate(bool tool, int replacement, bool releaseScope)
    {
        var state = new object();
        var content = new Func<IServiceProvider, object>(_ => new TextBox { Tag = state });
        var recycling = new ControlRecycling();
        var owner = CreateTemplateOwner(tool, "owner", content, recycling);
        var template = (IRecyclingDataTemplate)owner;
        var control = template.Build(owner, null);
        SetContent(owner, replacement switch
        {
            0 => null,
            1 => new TextBox(),
            _ => new Func<IServiceProvider, object>(_ => throw new InvalidOperationException("Unavailable"))
        });
        if (replacement == 2)
        {
            Assert.Throws<InvalidOperationException>(() => template.Build(owner, null));
        }
        else
        {
            template.Build(owner, null);
        }

        if (releaseScope)
        {
            recycling.Clear();
            ControlRecyclingDataTemplate.SetControlRecycling(owner, null);
        }
        return (owner, new WeakReference(content), new WeakReference(control), new WeakReference(recycling));
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

    private sealed class UnsupportedParent : Control
    {
        public UnsupportedParent(Control child)
        {
            VisualChildren.Add(child);
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
