using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Converters;
using AvalonDock.Layout;
using ReactiveUI;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ConverterInputs : ReactiveObject
{
    private object? _value;
    private object? _label;
    private object? _side;
    private bool _flag;
    private bool _canClose;
    public object? Value { get => _value; set => this.RaiseAndSetIfChanged(ref _value, value); }
    public object? Label { get => _label; set => this.RaiseAndSetIfChanged(ref _label, value); }
    public object? Side { get => _side; set => this.RaiseAndSetIfChanged(ref _side, value); }
    public bool Flag { get => _flag; set => this.RaiseAndSetIfChanged(ref _flag, value); }
    public bool CanClose { get => _canClose; set => this.RaiseAndSetIfChanged(ref _canClose, value); }
}

public class ConverterTests
{
    [AvaloniaFact]
    public void Unprefixed_Command_Converters_Execute_The_Native_Backed_Container_Commands()
    {
        using Workspace workspace = new();
        workspace.Inputs.Value = workspace.Tool;
        LayoutAnchorableItem item = (LayoutAnchorableItem)workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        Assert.Same(item.ActivateCommand, workspace.Button("Activate").Command);
        Assert.Same(item.HideCommand, workspace.Button("Hide").Command);
        Assert.Same(item.AutoHideCommand, workspace.Button("AutoHide").Command);
        workspace.Click("Activate");
        Assert.Same(workspace.Tool, workspace.Manager.Layout.ActiveContent);
        workspace.Click("AutoHide");
        Assert.True(workspace.Tool.IsAutoHidden);
        workspace.Click("AutoHide");
        Assert.False(workspace.Tool.IsAutoHidden);
        workspace.Tool.Float();
        Assert.Same(item, workspace.Manager.GetLayoutItemFromModel(workspace.Tool));
        workspace.Click("Hide");
        Assert.True(workspace.Tool.IsHidden);
        workspace.Tool.Show();
        Assert.True(workspace.Tool.IsVisible);
    }

    [AvaloniaFact]
    public void Command_Conversion_Uses_Current_Overrides_And_New_Layout_Ownership()
    {
        using Workspace workspace = new();
        IValueConverter[] converters = { new ActivateCommandLayoutItemFromLayoutModelConverter(), new HideCommandLayoutItemFromLayoutModelConverter(), new AutoHideCommandLayoutItemFromLayoutModelConverter() };
        LayoutAnchorableItem item = (LayoutAnchorableItem)workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        using ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> replacement = ReactiveCommand.Create(() => { });
        item.ActivateCommand = item.HideCommand = item.AutoHideCommand = replacement;
        foreach (IValueConverter converter in converters)
        {
            Assert.Same(replacement, Convert(converter, workspace.Tool, typeof(ICommand)));
            Assert.Null(Convert(converter, new LayoutAnchorable(), typeof(ICommand)));
            Assert.Null(Convert(converter, new object(), typeof(ICommand)));
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(null, typeof(LayoutContent), null, CultureInfo.InvariantCulture));
        }
        item.HideCommand = null;
        Assert.Null(Convert(converters[1], workspace.Tool, typeof(ICommand)));
        Assert.Same(BindingOperations.DoNothing, Convert(converters[1], workspace.Document, typeof(ICommand)));
        Assert.Same(BindingOperations.DoNothing, Convert(converters[2], workspace.Document, typeof(ICommand)));
        LayoutAnchorable next = new();
        workspace.Manager.Layout = new() { RootPanel = new(new LayoutAnchorablePane(next)) };
        Assert.Null(Convert(converters[0], workspace.Tool, typeof(ICommand)));
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(next)!.ActivateCommand, Convert(converters[0], next, typeof(ICommand)));
    }

    [AvaloniaFact]
    public void Compiled_Value_Converters_Preserve_DoNothing_And_Adapt_Visibility_To_Boolean()
    {
        using Workspace workspace = new();
        workspace.Inputs.Label = "First label";
        TextBlock label = workspace.View.FindControl<TextBlock>("KeptLabel")!;
        Assert.Equal("First label", label.Text);
        workspace.Inputs.Label = null;
        Assert.Equal("First label", label.Text);
        workspace.Inputs.Label = "Next label";
        Assert.Equal("Next label", label.Text);
        workspace.Inputs.Flag = true;
        Assert.True(workspace.View.FindControl<TextBlock>("VisibleLabel")!.IsVisible);
        Assert.False(workspace.View.FindControl<TextBlock>("InverseLabel")!.IsVisible);
        Assert.True(workspace.View.FindControl<TextBlock>("HideEntry")!.IsVisible);
        workspace.Inputs.CanClose = true;
        Assert.False(workspace.View.FindControl<TextBlock>("HideEntry")!.IsVisible);
        workspace.Inputs.Flag = false;
        Assert.False(workspace.View.FindControl<TextBlock>("VisibleLabel")!.IsVisible);
        Assert.True(workspace.View.FindControl<TextBlock>("InverseLabel")!.IsVisible);
        Assert.Equal(90, workspace.View.FindControl<ProgressBar>("Angle")!.Value);
        workspace.Inputs.Side = AnchorSide.Top;
        Assert.Equal(90, workspace.View.FindControl<ProgressBar>("Angle")!.Value);
        Assert.Equal(Orientation.Horizontal, workspace.View.FindControl<StackPanel>("Oriented")!.Orientation);
    }

    [Theory]
    [InlineData(AnchorSide.Left, true)]
    [InlineData(AnchorSide.Right, true)]
    [InlineData(AnchorSide.Top, false)]
    [InlineData(AnchorSide.Bottom, false)]
    [InlineData((AnchorSide)123, false)]
    public void Side_Converters_Preserve_Upstream_Default_And_DoNothing_Semantics(AnchorSide side, bool vertical)
    {
        AnchorSideToAngleConverter angle = new();
        AnchorSideToOrientationConverter orientation = new();
        Assert.Equal(vertical ? Orientation.Vertical : Orientation.Horizontal, Convert(orientation, side, typeof(Orientation)));
        if (vertical) Assert.Equal(90d, Convert(angle, side, typeof(double)));
        else Assert.Same(BindingOperations.DoNothing, Convert(angle, side, typeof(double)));
        Assert.Throws<NotImplementedException>(() => angle.ConvertBack(null, typeof(AnchorSide), null, CultureInfo.InvariantCulture));
        Assert.Throws<NotImplementedException>(() => orientation.ConvertBack(null, typeof(AnchorSide), null, CultureInfo.InvariantCulture));
        Assert.Throws<InvalidCastException>(() => Convert(angle, "Left", typeof(double)));
        Assert.Throws<NullReferenceException>(() => Convert(orientation, null, typeof(Orientation)));
    }

    [Theory]
    [InlineData(true, null, true, false)]
    [InlineData(false, null, false, true)]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, false)]
    [InlineData(false, false, false, true)]
    public void Visibility_Converters_Handle_Boolean_Parameters_And_Reverse_Conversion(bool value, object? parameter, bool visible, bool inverse)
    {
        BoolToVisibilityConverter normal = new();
        InverseBoolToVisibilityConverter inverted = new();
        Assert.Equal(visible, Convert(normal, value, typeof(bool), parameter));
        Assert.Equal(inverse, Convert(inverted, value, typeof(bool), parameter));
        Assert.Equal(value, normal.ConvertBack(value, typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Equal(!value, inverted.ConvertBack(value, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Visibility_Converters_Keep_Their_Distinct_Invalid_Input_Contracts()
    {
        BoolToVisibilityConverter normal = new();
        InverseBoolToVisibilityConverter inverse = new();
        Assert.Equal(false, Convert(normal, null, typeof(bool)));
        Assert.Equal(true, Convert(normal, null, typeof(bool), true));
        Assert.Equal(true, Convert(normal, "invalid", typeof(bool)));
        Assert.Equal(true, Convert(normal, false, typeof(object)));
        Assert.Throws<ArgumentException>(() => Convert(inverse, null, typeof(bool)));
        Assert.Throws<ArgumentException>(() => Convert(inverse, true, typeof(object)));
        Assert.Throws<ArgumentException>(() => normal.ConvertBack("invalid", typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Throws<ArgumentException>(() => inverse.ConvertBack(true, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Hide_Menu_Conversion_Preserves_Unset_And_Invalid_Input_Values()
    {
        AnchorableContextMenuHideVisibilityConverter converter = new();
        foreach (object?[] values in new[]
        {
            new object?[] { true, false }, new object?[] { "unchanged", false },
            new object?[] { AvaloniaProperty.UnsetValue, true }, new object?[] { true, AvaloniaProperty.UnsetValue },
            new object?[] { "unchanged", "invalid" }, new object?[] { "one" }, new object?[] { "three", true, false }
        })
        {
            Assert.Same(values[0], converter.Convert(values, typeof(bool), null, CultureInfo.InvariantCulture));
            Assert.Same(values[0], ((IMultiValueConverter)converter).Convert(values, typeof(bool), null, CultureInfo.InvariantCulture));
        }
        Assert.Equal(false, converter.Convert(new object?[] { true, true }, typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Throws<IndexOutOfRangeException>(() => converter.Convert(Array.Empty<object>(), typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(null, new[] { typeof(bool) }, null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Null_Converter_Preserves_Values_And_Binding_Sentinels()
    {
        NullToDoNothingConverter converter = new();
        Assert.Same(BindingOperations.DoNothing, Convert(converter, null, typeof(object)));
        object content = new();
        Assert.Same(content, Convert(converter, content, typeof(object)));
        Assert.Same(AvaloniaProperty.UnsetValue, Convert(converter, AvaloniaProperty.UnsetValue, typeof(object)));
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(null, typeof(object), null, CultureInfo.InvariantCulture));
    }

    private static object? Convert(IValueConverter converter, object? value, Type type, object? parameter = null) =>
        converter.Convert(value, type, parameter, CultureInfo.InvariantCulture);

    private sealed class Workspace : IDisposable
    {
        internal ConverterInputs Inputs { get; } = new() { Side = AnchorSide.Left };
        internal ConverterFixture View { get; } = new();
        internal DockingManager Manager { get; }
        internal LayoutDocument Document { get; }
        internal LayoutAnchorable Tool { get; }
        internal Workspace()
        {
            View.DataContext = Inputs;
            Manager = View.FindControl<DockingManager>("Manager")!;
            Document = Manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            Tool = Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
            View.Show();
            Dispatcher.UIThread.RunJobs();
        }
        internal Button Button(string name) => View.FindControl<Button>(name + "Button")!;
        internal void Click(string name)
        {
            Button button = Button(name);
            Dispatcher.UIThread.RunJobs();
            Point point = button.TranslatePoint(new(button.Bounds.Width / 2, button.Bounds.Height / 2), View)!.Value;
            View.MouseDown(point, MouseButton.Left);
            View.MouseUp(point, MouseButton.Left);
        }
        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
