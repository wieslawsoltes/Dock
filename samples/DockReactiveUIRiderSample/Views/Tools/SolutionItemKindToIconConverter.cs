using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DockReactiveUIRiderSample.Models;

namespace DockReactiveUIRiderSample.Views.Tools;

public sealed class SolutionItemKindToIconConverter : IValueConverter
{
    private static readonly Geometry s_solutionIcon = Geometry.Parse("M3 3H13V13H3Z M6 6H10V10H6Z");
    private static readonly Geometry s_projectIcon = Geometry.Parse("M4 3H12V11H4Z M6 5H10V9H6Z");
    private static readonly Geometry s_folderIcon = Geometry.Parse("M2 5H6L8 7H14V13H2Z");
    private static readonly Geometry s_documentIcon = Geometry.Parse("M5 2C3.895 2 3 2.895 3 4V14C3 15.105 3.895 16 5 16H11C12.105 16 13 15.105 13 14V6.414C13 6.015 12.842 5.634 12.561 5.354L9.646 2.439C9.365 2.158 8.984 2 8.586 2H5ZM4 4C4 3.448 4.448 3 5 3H8V5.5C8 6.328 8.672 7 9.5 7H12V14C12 14.552 11.552 15 11 15H5C4.448 15 4 14.552 4 14V4Z");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            SolutionItemKind.Solution => s_solutionIcon,
            SolutionItemKind.Project => s_projectIcon,
            SolutionItemKind.Folder => s_folderIcon,
            SolutionItemKind.Document => s_documentIcon,
            _ => s_documentIcon
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
