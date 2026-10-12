using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using AvalonDock.Converters;
using Xunit;

namespace Dock.Avalonia.AvalonDock.ImageTests;

public class HttpImageTests
{
    [AvaloniaTheory]
    [InlineData("http", false)]
    [InlineData("https", false)]
    [InlineData("https", true)]
    public async Task Network_Conversion_Returns_Before_Download_And_Publishes_On_The_UI_Thread(string scheme, bool relative)
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler);
        client.DefaultRequestHeaders.Add("X-Image-Test", "configured client");
        using UriSourceToBitmapImageConverter converter = new(client);
        Uri absolute = new(scheme + "://images.invalid/icons/icon.png");
        Uri uri = relative ? new("icon.png", UriKind.Relative) : absolute;
        Image image = Convert(converter, uri, relative ? new Uri(scheme + "://images.invalid/icons/") : null);
        Assert.Null(image.Source);
        Assert.Equal(absolute, handler.Requests[0].Uri);
        Assert.Equal("configured client", handler.Requests[0].Header);
        await Dispatcher.UIThread.InvokeAsync(() => { });
        Task<Bitmap> loaded = ObserveBitmap(image);
        handler.Requests[0].Complete(ImageResponse());
        using Bitmap bitmap = await loaded;
        Assert.Equal(new PixelSize(1, 1), bitmap.PixelSize);
        image.Stretch = global::Avalonia.Media.Stretch.None;
        image.Measure(new Size(100, 100));
        Assert.Equal(new Size(1, 1), image.DesiredSize);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Real_Http_And_Trusted_Https_Responses_And_Redirects_Load(bool tls, bool redirect)
    {
        await using ImageHttpServer server = new(ImageBytes(), tls);
        using HttpClient client = server.CreateClient();
        using UriSourceToBitmapImageConverter converter = tls ? new(client) : new();
        using LogCapture log = new();
        Image image = Convert(converter, new(server.BaseUri, redirect ? "redirect" : "icon.png"));
        using Bitmap bitmap = await ObserveBitmap(image, log.Warning.Task);
        Assert.Equal(new PixelSize(1, 1), bitmap.PixelSize);
        Assert.Equal(redirect ? new[] { "/redirect", "/icon.png" } : new[] { "/icon.png" }, server.Requests);
    }

    [AvaloniaFact]
    public async Task The_Default_Client_Rejects_An_Untrusted_Https_Certificate()
    {
        await using ImageHttpServer server = new(ImageBytes(), true);
        using LogCapture log = new();
        using UriSourceToBitmapImageConverter converter = new();
        Image image = Convert(converter, new(server.BaseUri, "icon.png"));
        await log.Warning.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Null(image.Source);
        Assert.Empty(server.Requests);
    }

    [AvaloniaTheory]
    [InlineData("status")]
    [InlineData("invalid image")]
    [InlineData("transport")]
    public async Task Download_Failures_Leave_The_Image_Empty_And_Are_Logged(string failure)
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler);
        using UriSourceToBitmapImageConverter converter = new(client);
        using LogCapture log = new();
        Uri uri = new("https://images.invalid/icon.png");
        Image image = Convert(converter, uri);
        if (failure == "transport") handler.Requests[0].Fail(new HttpRequestException("Connection failed"));
        else handler.Requests[0].Complete(failure == "status"
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[] { 1, 2, 3 }) });
        object?[] values = await log.Warning.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(uri, values[0]);
        Assert.Null(image.Source);
    }

    [AvaloniaFact]
    public async Task Client_Timeout_Cancels_A_Slow_Request()
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler) { Timeout = TimeSpan.FromMilliseconds(100) };
        using UriSourceToBitmapImageConverter converter = new(client);
        using LogCapture log = new();
        Image image = Convert(converter, new("https://images.invalid/icon.png"));
        await handler.Requests[0].Canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        handler.Requests[0].Fail(new OperationCanceledException());
        await log.Warning.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Null(image.Source);
    }

    [AvaloniaFact]
    public async Task Client_Timeout_Also_Covers_The_Response_Body()
    {
        await using ImageHttpServer server = new(ImageBytes(), false, pauseBody: true);
        using HttpClient client = server.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(250);
        using UriSourceToBitmapImageConverter converter = new(client);
        using LogCapture log = new();
        Image image = Convert(converter, new(server.BaseUri, "icon.png"));
        await server.BodyStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await log.Warning.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Null(image.Source);
    }

    [AvaloniaFact]
    public async Task Detaching_Cancels_A_Request_And_Reattachment_Does_Not_Accept_The_Old_Result()
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler);
        using UriSourceToBitmapImageConverter converter = new(client);
        Image image = Convert(converter, new("https://images.invalid/icon.png"));
        Window window = new() { Content = image };
        try
        {
            window.Show();
            window.Content = null;
            await handler.Requests[0].Canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
            window.Content = image;
            Assert.Equal(2, handler.Requests.Count);
            Task<Bitmap> loaded = ObserveBitmap(image);
            handler.Requests[1].Complete(ImageResponse());
            using Bitmap current = await loaded;
            handler.Requests[0].Complete(ImageResponse());
            await handler.Requests[0].Returned.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            Assert.Same(current, image.Source);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task An_Application_Source_Replacement_Cancels_The_Old_Download()
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler);
        using UriSourceToBitmapImageConverter converter = new(client);
        Image image = Convert(converter, new("https://images.invalid/icon.png"));
        using MemoryStream bytes = new(ImageBytes());
        using Bitmap replacement = new(bytes);
        image.Source = replacement;
        await handler.Requests[0].Canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        handler.Requests[0].Complete(ImageResponse());
        await handler.Requests[0].Returned.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Same(replacement, image.Source);
    }

    [AvaloniaFact]
    public async Task Completed_Bitmaps_Survive_Converter_Disposal_And_View_Reattachment()
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler);
        using UriSourceToBitmapImageConverter converter = new(client);
        Image image = Convert(converter, new("https://images.invalid/icon.png"));
        Task<Bitmap> loaded = ObserveBitmap(image);
        handler.Requests[0].Complete(ImageResponse());
        using Bitmap bitmap = await loaded;
        converter.Dispose();
        Window window = new() { Content = image };
        try
        {
            window.Show();
            window.Content = null;
            window.Content = image;
            Assert.Single(handler.Requests);
            Assert.Same(bitmap, image.Source);
            Assert.Equal(new PixelSize(1, 1), bitmap.PixelSize);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Pending_Downloads_Do_Not_Retain_Unused_Images()
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler);
        using UriSourceToBitmapImageConverter converter = new(client);
        WeakReference<Image> reference = CreateUnusedImage(converter);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Assert.False(reference.TryGetTarget(out _));
        handler.Requests[0].Complete(ImageResponse());
        await handler.Requests[0].Returned.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        GC.KeepAlive(converter);
    }

    [AvaloniaFact]
    public async Task Disposing_A_Converter_Cancels_Loads_Without_Disposing_A_Borrowed_Client()
    {
        using ControlledHandler handler = new();
        using HttpClient client = new(handler);
        UriSourceToBitmapImageConverter converter = new(client);
        Image image = Convert(converter, new("https://images.invalid/icon.png"));
        converter.Dispose(); converter.Dispose();
        await handler.Requests[0].Canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        handler.Requests[0].Fail(new OperationCanceledException());
        Assert.Null(image.Source);
        Assert.Throws<ObjectDisposedException>(() => converter.Convert(null, typeof(Image), null, CultureInfo.InvariantCulture));
        Task<HttpResponseMessage> stillUsable = client.GetAsync("https://images.invalid/another.png");
        handler.Requests[1].Complete(ImageResponse());
        using HttpResponseMessage response = await stillUsable;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Throws<ArgumentNullException>(() => new UriSourceToBitmapImageConverter(null!));
    }

    private static Image Convert(UriSourceToBitmapImageConverter converter, Uri uri, Uri? baseUri = null) =>
        Assert.IsAssignableFrom<Image>(converter.Convert(uri, typeof(Image), baseUri, CultureInfo.InvariantCulture));

    private static async Task<Bitmap> ObserveBitmap(Image image, Task<object?[]>? warning = null)
    {
        if (image.Source is Bitmap existing) return existing;
        TaskCompletionSource<Bitmap> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void Changed(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Image.SourceProperty && image.Source is Bitmap bitmap)
            {
                Assert.True(Dispatcher.UIThread.CheckAccess());
                ready.TrySetResult(bitmap);
            }
        }
        image.PropertyChanged += Changed;
        try
        {
            if (warning is not null && await Task.WhenAny(ready.Task, warning).WaitAsync(TimeSpan.FromSeconds(10)) == warning)
                throw new Xunit.Sdk.XunitException(string.Join("; ", await warning));
            return await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally { image.PropertyChanged -= Changed; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Image> CreateUnusedImage(UriSourceToBitmapImageConverter converter) =>
        new(Convert(converter, new("https://images.invalid/icon.png")));

    private static byte[] ImageBytes()
    {
        using Stream asset = AssetLoader.Open(new Uri("avares://Dock.Avalonia.AvalonDock.ImageTests/Assets/converter.png"));
        using MemoryStream bytes = new();
        asset.CopyTo(bytes);
        return bytes.ToArray();
    }

    private static HttpResponseMessage ImageResponse() => new(HttpStatusCode.OK) { Content = new ByteArrayContent(ImageBytes()) };

    private sealed class ControlledHandler : HttpMessageHandler
    {
        internal List<Request> Requests { get; } = new();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request pending = new(request.RequestUri!, request.Headers.TryGetValues("X-Image-Test", out var values) ? string.Join(",", values) : null);
            Requests.Add(pending);
            using CancellationTokenRegistration registration = cancellationToken.Register(() => pending.Canceled.TrySetResult());
            try { return await pending.Response.Task.ConfigureAwait(false); }
            finally { pending.Returned.TrySetResult(); }
        }
    }

    private sealed class Request(Uri uri, string? header)
    {
        internal Uri Uri { get; } = uri;
        internal string? Header { get; } = header;
        internal TaskCompletionSource<HttpResponseMessage> Response { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Canceled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Returned { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal void Complete(HttpResponseMessage response) => Response.TrySetResult(response);
        internal void Fail(Exception error) => Response.TrySetException(error);
    }

    private sealed class LogCapture : ILogSink, IDisposable
    {
        private readonly ILogSink? _previous = Logger.Sink;
        internal TaskCompletionSource<object?[]> Warning { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal LogCapture() => Logger.Sink = this;
        public bool IsEnabled(LogEventLevel level, string area) => level == LogEventLevel.Warning && area == "AvalonDock";
        public void Log(LogEventLevel level, string area, object? source, string messageTemplate) => Log(level, area, source, messageTemplate, Array.Empty<object?>());
        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        { if (IsEnabled(level, area)) Warning.TrySetResult(propertyValues); }
        public void Dispose() => Logger.Sink = _previous;
    }
}
