using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutUriImage : Image
{
    private readonly Uri _uri;
    private readonly ILayoutImageLoader _loader;
    private CancellationTokenSource? _pending;

    internal LayoutUriImage(Uri uri, ILayoutImageLoader loader)
    {
        _uri = uri;
        _loader = loader;
        BeginLoad();
    }

    protected override Type StyleKeyOverride => typeof(Image);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Source is null && _pending is null) BeginLoad();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelPending();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty) CancelPending();
    }

    private void BeginLoad()
    {
        if (_loader.IsDisposed) return;
        CancellationTokenSource pending = new();
        _pending = pending;
        // The request must not hold an image, window, or manager alive while the
        // remote endpoint is slow. Completion only acquires the target on the UI thread.
        _ = LoadAsync(new(this), _loader, _uri, pending);
    }

    private void CancelPending()
    {
        CancellationTokenSource? pending = _pending;
        _pending = null;
        pending?.Cancel();
    }

    private static async Task LoadAsync(WeakReference<LayoutUriImage> target, ILayoutImageLoader loader, Uri uri,
        CancellationTokenSource pending)
    {
        Bitmap? bitmap = null;
        Exception? error = null;
        try { bitmap = await loader.LoadAsync(uri, pending.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (pending.IsCancellationRequested || loader.IsDisposed) { }
        catch (Exception exception) { error = exception; }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                if (!target.TryGetTarget(out LayoutUriImage? image) || !ReferenceEquals(image._pending, pending))
                {
                    bitmap?.Dispose();
                    return;
                }
                image._pending = null;
                if (pending.IsCancellationRequested || loader.IsDisposed)
                {
                    bitmap?.Dispose();
                    return;
                }
                if (bitmap is not null) image.SetCurrentValue(SourceProperty, bitmap);
                else if (error is not null)
                    Logger.TryGet(LogEventLevel.Warning, "AvalonDock")?.Log(image,
                        "Unable to load image {Uri}: {Error}", uri, error.Message);
            }
            finally { pending.Dispose(); }
        });
    }
}
