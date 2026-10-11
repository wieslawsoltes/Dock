using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvalonDock.Infrastructure;

namespace AvalonDock.Converters;

/// <summary>Creates an Avalonia image control from a file, compiled asset, HTTP, or HTTPS URI.</summary>
/// <remarks>A relative asset URI may use an absolute base URI as the converter parameter.
/// Network images load asynchronously; detaching cancels a pending request, and
/// reattaching retries it. Failures leave the image empty and are logged to Avalonia.
/// The returned control owns a bitmap source that the caller can dispose when no longer needed.
/// Dispose a programmatically owned converter to cancel its requests and release its HTTP client.</remarks>
public class UriSourceToBitmapImageConverter : IValueConverter, IDisposable
{
    private readonly ILayoutImageLoader _loader;
    private bool _disposed;

    /// <summary>Creates a converter that owns and reuses its HTTP client.</summary>
    public UriSourceToBitmapImageConverter() => _loader = new LayoutHttpImageLoader(new HttpClient(), true);

    /// <summary>Creates a converter using an application-configured HTTP client.</summary>
    /// <param name="httpClient">The client used for network images. The application retains ownership.</param>
    public UriSourceToBitmapImageConverter(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _loader = new LayoutHttpImageLoader(httpClient, false);
    }

    /// <summary>Loads a URI into an image, preserving the current binding target when the value is null.</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (value is null) return BindingOperations.DoNothing;
        Uri uri = (Uri)value;
        Uri? baseUri = parameter as Uri;
        if (!uri.IsAbsoluteUri && baseUri is not null) uri = new Uri(baseUri, uri);
        if (uri.IsAbsoluteUri && uri.Scheme is "http" or "https") return new LayoutUriImage(uri, _loader);
        Bitmap bitmap;
        if (uri.IsAbsoluteUri && uri.IsFile) bitmap = new Bitmap(uri.LocalPath);
        else
        {
            // AssetLoader resolves avares resources without runtime XAML or type discovery.
            if (uri.IsAbsoluteUri && uri.Scheme != "avares")
                throw new NotSupportedException("Image URIs must use file, avares, HTTP, or HTTPS.");
            using Stream stream = AssetLoader.Open(uri, baseUri);
            bitmap = new Bitmap(stream);
        }
        return new Image { Source = bitmap };
    }

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    /// <summary>Cancels pending network loads and disposes an owned HTTP client.
    /// An application-supplied client and completed bitmap sources are not disposed.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _loader.Dispose();
    }
}
