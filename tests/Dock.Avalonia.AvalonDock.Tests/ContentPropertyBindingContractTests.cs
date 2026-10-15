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
    public void Native_Backed_Properties_Use_WPF_Assignment_Current_Value_And_Clear_Lifetimes(bool tool, bool title, BindingMode mode)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        AvaloniaProperty property = title ? (AvaloniaProperty)LayoutContent.TitleProperty : LayoutContent.ContentIdProperty;
        ValueSource source = new() { Value = "initial" };
        CompiledBindingPath path = new CompiledBindingPathBuilder().Property(ValueSource.ValueProperty,
            PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor).Build();
        bool twoWay = mode == BindingMode.TwoWay;
        using IDisposable clrBinding = Bind("initial");
        AssertState("initial", "initial", true);

        if (title) content.Title = "clr";
        else content.ContentId = "clr";
        AssertState("clr", twoWay ? "clr" : "initial", twoWay);
        source.Value = "after-clr";
        AssertState(twoWay ? "after-clr" : "clr", "after-clr", twoWay);

        using IDisposable propertyBinding = Bind("property-source");
        ((AvaloniaObject)content).SetValue(property, "property");
        AssertState("property", twoWay ? "property" : "property-source", twoWay);
        source.Value = "after-property";
        AssertState(twoWay ? "after-property" : "property", "after-property", twoWay);

        using IDisposable currentBinding = Bind("current-source");
        content.SetCurrentValue(property, "current");
        AssertState("current", twoWay ? "current" : "current-source", true);
        source.Value = "after-current";
        AssertState("after-current", "after-current", true);

        if (title) content.DockModel.Title = "native";
        else content.DockModel.Id = "native";
        AssertState("native", twoWay ? "native" : "after-current", true);
        int notifications = 0;
        content.PropertyChanging += (_, _) => notifications++;
        content.PropertyChanged += (_, _) => notifications++;
        content.CoerceValue(property);
        Assert.Equal(0, notifications);
        AssertState("native", twoWay ? "native" : "after-current", true);

        ((AvaloniaObject)content).ClearValue(property);
        AssertState(null, twoWay ? "native" : "after-current", false);
        source.Value = "after-untyped-clear";
        AssertState(null, "after-untyped-clear", false);

        using IDisposable typedClearBinding = Bind("typed-clear-source");
        if (title) content.ClearValue(LayoutContent.TitleProperty);
        else content.ClearValue(LayoutContent.ContentIdProperty);
        AssertState(null, "typed-clear-source", false);
        source.Value = "after-typed-clear";
        AssertState(null, "after-typed-clear", false);

        using IDisposable equalBinding = Bind("equal");
        if (title) content.Title = "equal";
        else content.ContentId = "equal";
        AssertState("equal", "equal", twoWay);

        IDisposable Bind(string value)
        {
            source.Value = value;
            return content.Bind(property, new CompiledBindingExtension(path) { Source = source, Mode = mode });
        }

        void AssertState(string? expectedTarget, string? expectedSource, bool bound)
        {
            Assert.Equal(expectedTarget, content.GetValue(property));
            Assert.Equal(expectedTarget, title ? content.DockModel.Title : content.DockModel.Id);
            Assert.Equal(expectedSource, source.Value);
            Assert.Equal(bound, BindingOperations.GetBindingExpressionBase(content, property) is not null);
        }
    }

    private sealed class ValueSource : AvaloniaObject
    {
        public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<ValueSource, string?>(nameof(Value));
        public string? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
}
