using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContentPropertyBindingContractTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (bool tool in new[] { false, true })
        foreach (bool title in new[] { false, true })
        foreach (BindingMode mode in new[] { BindingMode.Default, BindingMode.OneWay, BindingMode.TwoWay })
            yield return new object[] { tool, title, mode };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Native_Backed_Properties_Retain_Compiled_Bindings_Until_Explicit_Disposal(bool tool, bool title, BindingMode mode)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        AvaloniaProperty property = title ? (AvaloniaProperty)LayoutContent.TitleProperty : LayoutContent.ContentIdProperty;
        ValueSource source = new() { Value = "initial" };
        CompiledBindingPath path = new CompiledBindingPathBuilder().Property(ValueSource.ValueProperty,
            PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor).Build();
        using IDisposable binding = content.Bind(property, new CompiledBindingExtension(path) { Source = source, Mode = mode });
        bool twoWay = mode == BindingMode.TwoWay;
        AssertState("initial", "initial");

        if (title) content.Title = "clr";
        else content.ContentId = "clr";
        AssertState("clr", twoWay ? "clr" : "initial");
        source.Value = "after-clr";
        AssertState("after-clr", "after-clr");

        content.SetValue(property, "property");
        AssertState("property", twoWay ? "property" : "after-clr");
        source.Value = "after-property";
        AssertState("after-property", "after-property");

        content.SetCurrentValue(property, "current");
        AssertState("current", twoWay ? "current" : "after-property");
        source.Value = "after-current";
        AssertState("after-current", "after-current");

        content.ClearValue(property);
        AssertState("after-current", "after-current");
        if (title) content.ClearValue(LayoutContent.TitleProperty);
        else content.ClearValue(LayoutContent.ContentIdProperty);
        AssertState(null, twoWay ? null : "after-current");
        source.Value = "after-clear";
        AssertState("after-clear", "after-clear");

        if (title) content.DockModel.Title = "native";
        else content.DockModel.Id = "native";
        AssertState("native", twoWay ? "native" : "after-clear");
        int notifications = 0;
        content.PropertyChanging += (_, _) => notifications++;
        content.PropertyChanged += (_, _) => notifications++;
        content.CoerceValue(property);
        Assert.Equal(0, notifications);
        AssertState("native", twoWay ? "native" : "after-clear");

        binding.Dispose();
        Assert.Null(BindingOperations.GetBindingExpressionBase(content, property));
        content.SetValue(property, "replacement");
        source.Value = "detached-source";
        Assert.Equal("replacement", content.GetValue(property));
        Assert.Equal("replacement", title ? content.DockModel.Title : content.DockModel.Id);

        void AssertState(string? expectedTarget, string? expectedSource)
        {
            Assert.Equal(expectedTarget, content.GetValue(property));
            Assert.Equal(expectedTarget, title ? content.DockModel.Title : content.DockModel.Id);
            Assert.Equal(expectedSource, source.Value);
            Assert.NotNull(BindingOperations.GetBindingExpressionBase(content, property));
        }
    }

    private sealed class ValueSource : AvaloniaObject
    {
        public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<ValueSource, string?>(nameof(Value));
        public string? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
}
