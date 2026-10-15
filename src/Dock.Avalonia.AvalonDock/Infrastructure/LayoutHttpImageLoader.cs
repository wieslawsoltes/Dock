using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutHttpImageLoader(HttpClient client, bool ownsClient) : ILayoutImageLoader
{
    private readonly CancellationTokenSource _lifetime = new();
    private volatile bool _disposed;

    public bool IsDisposed => _disposed;

    public async Task<Bitmap> LoadAsync(Uri uri, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
        // Buffering the complete response keeps HttpClient.Timeout effective
        // while the body is downloaded, as well as while awaiting the headers.
        using HttpResponseMessage response = await client.GetAsync(uri, linked.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using Stream source = await response.Content.ReadAsStreamAsync(linked.Token).ConfigureAwait(false);
        linked.Token.ThrowIfCancellationRequested();
        return new Bitmap(source);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lifetime.Cancel();
        _lifetime.Dispose();
        if (ownsClient) client.Dispose();
    }
}
