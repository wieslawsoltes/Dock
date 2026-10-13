using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DependencyPropertyBindingTests
{
    [AvaloniaTheory]
    [InlineData("template")]
    [InlineData("source")]
    [InlineData("command")]
    [InlineData("control-model")]
    [InlineData("item-title")]
    public void Compatibility_Properties_Preserve_Current_Values_And_Replace_Explicit_Bindings(string kind)
    {
        int titleIndex = 0;
        (AvaloniaObject Target, AvaloniaProperty Property, Func<object> CreateValue) configuration = kind switch
        {
            "template" => (new DockingManager(), DockingManager.LayoutItemTemplateProperty,
                () => new FuncDataTemplate<object>((_, _) => new Border())),
            "source" => (new DockingManager(), DockingManager.DocumentsSourceProperty, () => new object[0]),
            "command" => (new LayoutDocumentItem(), LayoutItem.CloseCommandProperty, () => new TestCommand()),
            "control-model" => (new LayoutDocumentControl(), LayoutDocumentControl.ModelProperty, () => new LayoutDocument()),
            "item-title" => (new LayoutDocumentItem(), LayoutItem.TitleProperty, () => $"title{++titleIndex}"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        (AvaloniaObject target, AvaloniaProperty property, Func<object> createValue) = configuration;
        object initial = createValue();
        object current = createValue();
        object later = createValue();
        ValueSource source = new() { Value = initial };
        CompiledBindingPath path = new CompiledBindingPathBuilder().Property(ValueSource.ValueProperty,
            PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor).Build();
        using IDisposable oneWay = target.Bind(property, new CompiledBindingExtension(path) { Source = source });
        Assert.Same(initial, target.GetValue(property));

        target.SetCurrentValue(property, current);
        Assert.Same(initial, source.Value);
        Assert.NotNull(BindingOperations.GetBindingExpressionBase(target, property));
        source.Value = later;
        Assert.Same(later, target.GetValue(property));

        target[property] = current;
        Assert.Null(BindingOperations.GetBindingExpressionBase(target, property));
        source.Value = initial;
        Assert.Same(current, target.GetValue(property));

        using IDisposable twoWay = target.Bind(property,
            new CompiledBindingExtension(path) { Source = source, Mode = BindingMode.TwoWay });
        target[property] = current;
        Assert.Same(current, source.Value);
        Assert.NotNull(BindingOperations.GetBindingExpressionBase(target, property));
        target.ClearValue(property);
        Assert.Null(target.GetValue(property));
        Assert.Same(current, source.Value);
        Assert.Null(BindingOperations.GetBindingExpressionBase(target, property));
    }

    private sealed class ValueSource : AvaloniaObject
    {
        public static readonly StyledProperty<object?> ValueProperty = AvaloniaProperty.Register<ValueSource, object?>(nameof(Value));
        public object? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }

    private sealed class TestCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
