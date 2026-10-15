using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using AvalonDock.Converters;

namespace PackageConsumer;

internal static class HttpImageProbe
{
    internal static void Verify()
    {
        using HttpClient client = new(new AssetHandler());
        using UriSourceToBitmapImageConverter converter = new(client);
        Image image = (Image)converter.Convert(new Uri("https://package.invalid/icon.png"), typeof(Image), null, CultureInfo.InvariantCulture);
        Dispatcher.UIThread.RunJobs();
        using Bitmap bitmap = image.Source as Bitmap ?? throw new Exception("The packaged converter did not publish its network bitmap.");
        if (bitmap.PixelSize.Width != 1 || bitmap.PixelSize.Height != 1)
            throw new Exception("The packaged network image has the wrong size.");
        converter.Dispose();
        if (!ReferenceEquals(bitmap, image.Source))
            throw new Exception("Disposing the packaged converter discarded a completed bitmap.");
    }

    private sealed class AssetHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.Scheme != "https") throw new Exception("The packaged converter changed the requested URI.");
            using Stream asset = AssetLoader.Open(new Uri("avares://Dock.Avalonia.AvalonDock.PackageSmoke/Assets/converter.png"));
            using MemoryStream bytes = new();
            asset.CopyTo(bytes);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes.ToArray()) });
        }
    }
}
