using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace AvalonDock.Infrastructure;

internal interface ILayoutImageLoader : IDisposable
{
    bool IsDisposed { get; }
    Task<Bitmap> LoadAsync(Uri uri, CancellationToken cancellationToken);
}
