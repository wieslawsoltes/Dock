using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;

namespace AvalonDock.Infrastructure;

// Icon selectors receive the owning menu item, as in AvalonDock's menu contract.
internal sealed class LayoutMenuIcon : IValueConverter
{
    public IDataTemplate? DefaultTemplate { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is MenuItem container
            ? new LayoutHeaderTemplate { Kind = LayoutHeaderKind.MenuIcon, DefaultTemplate = DefaultTemplate, SelectionContainer = container }
            : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
