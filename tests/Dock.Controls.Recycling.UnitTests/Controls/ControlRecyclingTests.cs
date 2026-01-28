using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling;
using Avalonia.Controls.Recycling.Model;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace Dock.Controls.Recycling.UnitTests.Controls;

public class ControlRecyclingTests
{
    private class IdData(string id) : AvaloniaObject, IControlRecyclingIdProvider
    {
        public string? Id { get; } = id;
        public string? GetControlRecyclingId() => Id;
    }

    private class RecyclingIdData(string id, string value) : AvaloniaObject, IControlRecyclingIdProvider
    {
        public string Id { get; } = id;
        public string Value { get; } = value;
        public string? GetControlRecyclingId() => Id;
    }

    private class TrackingRecyclingTemplate : IRecyclingDataTemplate
    {
        public int BuildCalls { get; private set; }

        public Control? Build(object? data) => Build(data, null);

        public bool Match(object? data) => data is RecyclingIdData;

        public Control? Build(object? data, Control? existing)
        {
            BuildCalls++;
            var control = existing ?? new TextBlock();
            control.Tag = data is RecyclingIdData recyclingData ? recyclingData.Value : null;
            return control;
        }
    }

    private sealed class TestViewModel : INotifyPropertyChanged
    {
        private object? _item;

        public object? Item
        {
            get => _item;
            set
            {
                if (ReferenceEquals(_item, value))
                {
                    return;
                }

                _item = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Item)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    [AvaloniaFact]
    public void Add_And_TryGetValue_Work()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        var control = new TextBlock();

        recycling.Add(data, control);

        var found = recycling.TryGetValue(data, out var cached);
        Assert.True(found);
        Assert.Same(control, cached);
    }

    [AvaloniaFact]
    public void TryGetValue_With_Null_Returns_False()
    {
        var recycling = new ControlRecycling();
        var result = recycling.TryGetValue(null, out var cached);
        Assert.False(result);
        Assert.Null(cached);
    }

    [AvaloniaFact]
    public void Clear_Removes_Items()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        recycling.Add(data, new TextBlock());
        recycling.Clear();
        Assert.False(recycling.TryGetValue(data, out _));
    }

    [AvaloniaFact]
    public void Build_Returns_Null_For_Null_Data()
    {
        var recycling = new ControlRecycling();
        Assert.Null(recycling.Build(null, null, null));
    }

    [AvaloniaFact]
    public void Build_Uses_DataTemplate_And_Caches_Result()
    {
        var recycling = new ControlRecycling();
        var parent = new Control();
        parent.DataTemplates.Add(new FuncDataTemplate<int>((_, _) => new Border { Background = Brushes.Red }, true));

        var result1 = recycling.Build(10, null, parent) as Control;
        var result2 = recycling.Build(10, null, parent) as Control;

        Assert.NotNull(result1);
        Assert.Same(result1, result2);
    }

    [AvaloniaFact]
    public void Build_Updates_Cached_Control_When_Recycling_Template_Is_Used()
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = true };
        var parent = new Control();
        var template = new TrackingRecyclingTemplate();
        parent.DataTemplates.Add(template);

        var data1 = new RecyclingIdData("a", "first");
        var data2 = new RecyclingIdData("a", "second");

        var result1 = recycling.Build(data1, null, parent) as Control;
        var result2 = recycling.Build(data2, null, parent) as Control;

        Assert.Same(result1, result2);
        Assert.Equal("second", result1?.Tag);
        Assert.Equal(2, template.BuildCalls);
    }

    [AvaloniaFact]
    public void Build_Uses_Id_When_Enabled()
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = true };
        var parent = new Control();
        parent.DataTemplates.Add(new FuncDataTemplate<IdData>((_, _) => new Border(), true));

        var data1 = new IdData("a");
        var data2 = new IdData("a");

        var result1 = recycling.Build(data1, null, parent);
        var result2 = recycling.Build(data2, null, parent);

        Assert.Same(result1, result2);
    }

    [AvaloniaFact]
    public void Build_Ignores_Id_When_Disabled()
    {
        var recycling = new ControlRecycling { TryToUseIdAsKey = false };
        var parent = new Control();
        parent.DataTemplates.Add(new FuncDataTemplate<IdData>((_, _) => new Border(), true));

        var data1 = new IdData("a");
        var data2 = new IdData("a");

        var result1 = recycling.Build(data1, null, parent);
        var result2 = recycling.Build(data2, null, parent);

        Assert.NotSame(result1, result2);
    }

    [AvaloniaFact]
    public void Build_Removes_Cached_Control_From_Visual_Parent()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        var control = new TextBlock { Background = Brushes.Red };
        var parentPanel = new StackPanel();
        
        // Add the control to the parent panel
        parentPanel.Children.Add(control);
        
        // Cache the control
        recycling.Add(data, control);
        
        // Build should return the cached control and remove it from its parent
        var result = recycling.Build(data, null, null);
        
        Assert.Same(control, result);
        Assert.Empty(parentPanel.Children); // Parent should no longer contain the control
    }

    [AvaloniaFact]
    public void Build_Removes_Cached_Control_From_ContentPresenter()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        var control = new TextBlock { Background = Brushes.Red };
        var presenter = new ContentPresenter { Content = control };
        var window = new Window { Content = presenter };

        try
        {
            window.Show();
            window.UpdateLayout();

            recycling.Add(data, control);

            var result = recycling.Build(data, null, null);

            Assert.Same(control, result);
            Assert.Null(presenter.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Build_Handles_Cached_Control_Without_Visual_Parent()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        var control = new TextBlock { Background = Brushes.Blue };
        
        // Cache the control without adding it to any parent first
        recycling.Add(data, control);
        
        // Build should return the cached control
        var result = recycling.Build(data, null, null);
        
        Assert.Same(control, result);
    }

    [AvaloniaFact]
    public void Build_Reparents_Control_From_Recycling_ContentPresenter_And_Preserves_Binding()
    {
        var data = new object();
        var updated = new object();
        var viewModel = new TestViewModel { Item = data };
        var recycling = new ControlRecycling();
        var template = new FuncDataTemplate<object>((value, _) => new Border { Tag = value }, true);

        var presenterA = new ContentPresenter();
        presenterA.DataTemplates.Add(template);
        presenterA.Bind(ContentPresenter.ContentProperty, new Binding(nameof(TestViewModel.Item)) { Source = viewModel });
        ControlRecyclingDataTemplate.SetControlRecycling(presenterA, recycling);
        presenterA.ContentTemplate = new ControlRecyclingDataTemplate { Parent = presenterA };

        var presenterB = new ContentPresenter();
        presenterB.DataTemplates.Add(template);
        ControlRecyclingDataTemplate.SetControlRecycling(presenterB, recycling);
        presenterB.ContentTemplate = new ControlRecyclingDataTemplate { Parent = presenterB };

        var window = new Window
        {
            Content = new StackPanel
            {
                Children =
                {
                    presenterA,
                    presenterB
                }
            }
        };

        try
        {
            window.Show();
            window.UpdateLayout();

            var cached = presenterA.Child;
            Assert.NotNull(cached);

            presenterB.Content = data;
            window.UpdateLayout();

            Assert.Same(cached, presenterB.Child);
            Assert.Null(presenterA.Child);
            Assert.NotNull(BindingOperations.GetBindingExpressionBase(presenterA, ContentPresenter.ContentProperty));

            viewModel.Item = updated;
            window.UpdateLayout();

            Assert.Same(updated, presenterA.Content);
            Assert.NotNull(presenterA.Child);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Build_Reuses_Cached_Control_Successfully()
    {
        var recycling = new ControlRecycling();
        var data = new object();
        var control = new TextBlock { Background = Brushes.Green };
        
        // Cache the control
        recycling.Add(data, control);
        
        // Build should return the cached control
        var result1 = recycling.Build(data, null, null);
        var result2 = recycling.Build(data, null, null);
        
        Assert.Same(control, result1);
        Assert.Same(control, result2);
    }
}
