using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AvalonDock.Converters;

/// <summary>Creates an Avalonia image control from a file or compiled asset URI.</summary>
/// <remarks>A relative asset URI may use an absolute base URI as the converter parameter.
/// The returned control owns a bitmap source that the caller can dispose when no longer needed.</remarks>
public class UriSourceToBitmapImageConverter : IValueConverter
{
    /// <summary>Loads a URI into an image, preserving the current binding target when the value is null.</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return BindingOperations.DoNothing;
        Uri uri = (Uri)value;
        Uri? baseUri = parameter as Uri;
        if (!uri.IsAbsoluteUri && baseUri is not null) uri = new Uri(baseUri, uri);
        Bitmap bitmap;
        if (uri.IsAbsoluteUri && uri.IsFile) bitmap = new Bitmap(uri.LocalPath);
        else
        {
            // AssetLoader resolves avares resources without runtime XAML or type discovery.
            if (uri.IsAbsoluteUri && uri.Scheme != "avares")
                throw new NotSupportedException("Image URIs must refer to local files or avares assets.");
            using Stream stream = AssetLoader.Open(uri, baseUri);
            bitmap = new Bitmap(stream);
        }
        return new Image { Source = bitmap };
    }

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
